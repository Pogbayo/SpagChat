using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SpagChat.Application.DTO.Auth;
using SpagChat.Application.DTO.LoginResponse;
using SpagChat.Application.DTO.Users;
using SpagChat.Application.Interfaces.ICache;
using SpagChat.Application.Interfaces.IRepositories;
using SpagChat.Application.Interfaces.IServices;
using SpagChat.Application.Result;
using SpagChat.Domain.Entities;

namespace SpagChat.Application.Services
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly ILogger<ApplicationUserService> _logger;
        private readonly IEmailService _emailService;
        private readonly IApplicationUserRepository _applicationUserRepository;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IMapper _mapper;
        private readonly ICustomMemoryCache _cache;
        private readonly IChatRoomUserService _chatRoomUserService;
        public ApplicationUserService(IChatRoomUserService chatRoomUserService,ICustomMemoryCache cache, IMapper mapper,ITokenGenerator tokenGenerator,ILogger<ApplicationUserService> logger, IEmailService emailService, IApplicationUserRepository applicationUserRepository)
        {
            _chatRoomUserService = chatRoomUserService;
            _cache = cache;
            _mapper = mapper;
            _tokenGenerator = tokenGenerator;
            _logger = logger;
            _emailService = emailService;
            _applicationUserRepository = applicationUserRepository;
        }

        public async Task<Result<IEnumerable<ApplicationUserDto>?>> GetAllUsersAsync(int numberOfUsers)
        {
            if (numberOfUsers <= 0)
            {
                _logger.LogError("The number of users must be greater than zero.");
                return Result<IEnumerable<ApplicationUserDto>?>.FailureResponse("Invalid number of users provided.", "Number must be greater than zero.");
            }

            string cacheKey = $"AllUsers_{numberOfUsers}";

            if (!_cache.TryGetValue(cacheKey, out IEnumerable<ApplicationUserDto>? cachedUsers)) 
            {
                _logger.LogInformation("Cache not found, fetching users from database...");

                var users = await _applicationUserRepository.GetAllUsers(numberOfUsers);

                if (users == null || !users.Any())
                {
                    _logger.LogError("Users' list is empty.");
                    return Result<IEnumerable<ApplicationUserDto>?>.FailureResponse("Invalid number of users provided.", "Number must be greater than zero.");
                }

                var mappedUsers = _mapper.Map<IEnumerable<ApplicationUserDto>>(users);

                _cache.Set(cacheKey, mappedUsers, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });

                cachedUsers = mappedUsers;
            }

            else
            {
                _logger.LogInformation("Cache hit, returning cache users...");
            }

            return Result<IEnumerable<ApplicationUserDto>?>.SuccessResponse(cachedUsers!, "Users fetched successfully.");
        }


        public async Task<Result<ApplicationUserDto>> GetUserByIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogError("Invalid user ID provided.");
                return Result<ApplicationUserDto>.FailureResponse("Invalid user ID.", "Please provide a valid user ID.");
            }

            string cacheKey = $"User_{userId}";

            if (!_cache.TryGetValue(cacheKey, out ApplicationUserDto? cachedUser))
            {
                _logger.LogInformation("Cache not found, fetching user from database...");

                var user = await _applicationUserRepository.GetUserById(userId);

                if (user == null)
                {
                    _logger.LogError("User with provided ID does not exist.");
                    return Result<ApplicationUserDto>.FailureResponse("User not found.", "The user with the provided ID does not exist.");
                }

                var mappedUser = _mapper.Map<ApplicationUserDto>(user);

                _cache.Set(cacheKey, mappedUser, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });

                cachedUser = mappedUser;
            }
            else
            {
                _logger.LogInformation("Cache hit, returning cached user...");
            }

            return Result<ApplicationUserDto>.SuccessResponse(cachedUser!, "User fetched successfully.");
        }


        public async Task<Result<LoginResponseDto>> LoginAsync(LoginUserDto userDetails)
        {
            if (string.IsNullOrWhiteSpace(userDetails.Email) || string.IsNullOrWhiteSpace(userDetails.Password))
            {
                _logger.LogWarning("Invalid credentials provided.");
                return Result<LoginResponseDto>.FailureResponse("Invalid credentials.", "Email and Password cannot be empty.");
            }

            var user = await _applicationUserRepository.LoginAsync(userDetails);
            var mappedUser = _mapper.Map<ApplicationUserDto>(user);
            if (user == null)
            {
                _logger.LogWarning("User not found.");
                return Result<LoginResponseDto>.FailureResponse("User not found.", "Invalid email or password.");
            }

            var token = _tokenGenerator.GenerateAccessToken(user);

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Token generation failed");
                throw new ArgumentNullException("Token is null");
            }

            var newObject = new LoginResponseDto
            {
                Token = token,
                User = mappedUser,
            };

            _logger.LogInformation($"This is the user details,{mappedUser}");
            return Result<LoginResponseDto>.SuccessResponse(newObject, "Login successful.");
        }


        public async Task<Result<Guid>> RegisterUserAsync(CreateUserDto userdetails)
        {
            if (string.IsNullOrEmpty(userdetails.Email) || string.IsNullOrEmpty(userdetails.Password))
            {
                _logger.LogWarning("Email or Password is empty.");
                return Result<Guid>.FailureResponse("Invalid input.", "Email or Password cannot be empty.");
            }

            var user = new ApplicationUser
            {
                Email = userdetails.Email,
                UserName = userdetails.Email,
            };

            var result = await _applicationUserRepository.CreateUserAsync(user, userdetails.Password);
            await _chatRoomUserService.AddUsersToChatRoomAsync(
                Guid.Parse("42E088DE-97D6-459C-CD2D-08DDAB8890DF"),
                new List<Guid> { Guid.Parse($"{user.Id}") }

            ); if (result == null || !result.Succeeded)
            {
                _logger.LogError("User registration failed.");
                var error = result?.Errors?.FirstOrDefault()?.Description ?? "Unknown registration error.";
                return Result<Guid>.FailureResponse("Registration failed.", error);
            }

            await _emailService.SendEmailAsync(user.Email, "Welcome to SpagChat!", "Thank you for registering!");
            _cache.RemoveByPrefix("User_");

            return Result<Guid>.SuccessResponse(user.Id, "User registered successfully.");
        }

    }
}
