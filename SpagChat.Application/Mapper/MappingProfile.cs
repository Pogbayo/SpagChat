using AutoMapper;
using SpagChat.Domain.Entities;
using SpagChat.Application.DTO.Users;
using SpagChat.Application.DTO.ChatRooms;
using SpagChat.Application.DTO.Messages;
using SpagChat.Application.DTO.ChatRoomUsers;

namespace SpagChat.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ApplicationUser -> ApplicationUserDto
            CreateMap<ApplicationUser, ApplicationUserDto>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName ?? ""))
                .ForMember(dest => dest.DpUrl, opt => opt.MapFrom(src => src.DpUrl ?? ""));

            // ChatRoom -> ChatRoomDto
            CreateMap<ChatRoom, ChatRoomDto>()
              .ForMember(dest => dest.LastMessageContent, opt => opt.MapFrom(src =>
               src.Messages != null && src.Messages.Any()
             ? src.Messages.OrderByDescending(m => m.Timestamp).First().Content
             : string.Empty))
             .ForMember(dest => dest.LastMessageTimestamp, opt => opt.MapFrom(src =>
              src.Messages != null && src.Messages.Any()
             ? src.Messages.OrderByDescending(m => m.Timestamp).First().Timestamp
             : (DateTime?)null));


            // ChatRoom -> ChatRoomPreviewDto
            CreateMap<ChatRoom, ChatRoomPreviewDto>()
                .ForMember(dest => dest.LastMessage, opt => opt.MapFrom(src =>
                    src.Messages != null && src.Messages.Any()
                        ? src.Messages.OrderByDescending(m => m.Timestamp).First().Content
                        : ""))
                .ForMember(dest => dest.LastMessageTimestamp, opt => opt.MapFrom(src =>
                    src.Messages != null && src.Messages.Any()
                        ? src.Messages.OrderByDescending(m => m.Timestamp).First().Timestamp
                        : (DateTime?)null));

            // ChatRoomUser -> ChatRoomUserDto
            CreateMap<ChatRoomUser, ChatRoomUserDto>();

            // Message -> MessageDto
            CreateMap<Message, MessageDto>()
              .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.Sender))
              .ForMember(dest => dest.IsEdited, opt => opt.MapFrom(src => src.IsEdited))
              .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
              .ForMember(dest => dest.ReadByUsers, opt => opt.MapFrom(src => src.ReadBy));  // ← Changed this

            // MessageReadBy -> MessageReadByDto
            CreateMap<MessageReadBy, MessageReadByDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User!.UserName))
                .ForMember(dest => dest.ReadAt, opt => opt.MapFrom(src => src.ReadAt));

            CreateMap<SendMessageDto, Message>();

            // CreateChatRoomDto -> ChatRoom
            CreateMap<CreateChatRoomDto, ChatRoom>();
        }
    }
}
