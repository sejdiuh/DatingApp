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
        }
    }
}