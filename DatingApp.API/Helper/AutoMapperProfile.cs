using System.Linq;
using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Models;

namespace DatingApp.API.Helper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserForListDto>()
                    .ForMember(x => x.PhotoUrl, opt => {
                        opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url);
                    })
                    .ForMember(x => x.Age, opt => {
                        opt.MapFrom(d => d.DateOfBirth.CalculateAge());
                    });
            CreateMap<User, UserForDetaileDto>()
                    .ForMember(x => x.PhotoUrl, opt => {
                        opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url);
                    })
                    .ForMember(x => x.Age, opt => {
                        opt.MapFrom(d => d.DateOfBirth.CalculateAge());
                    });
            CreateMap<Photo, PhotoDetailedDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<PhotoForCreationDto, Photo>();
            CreateMap<UserForRegisterDto, User>();
            CreateMap<MessageFroCreationDto, Message>().ReverseMap();
            CreateMap<Message, MessageToReturn>()
                    .ForMember(x => x.SenderPhotoUrl, opt => 
                        opt.MapFrom(u => u.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
                    .ForMember(x => x.RecipientPhotoUrl, opt =>
                        opt.MapFrom(u => u.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url));
        }
    }
}