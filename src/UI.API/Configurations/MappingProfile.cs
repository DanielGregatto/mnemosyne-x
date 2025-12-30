using AutoMapper;
using Domain.DTO.Responses;
using Identity.Model;
using UI.API.Models.Requests;

namespace UI.API.Configurations
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, ProfileDto>().ReverseMap();

            CreateMap<ApplicationUser, ProfilePersonalInfoRequest>().ReverseMap();

            CreateMap<ApplicationUser, ProfileAddressRequest>().ReverseMap();
        }
    }
}