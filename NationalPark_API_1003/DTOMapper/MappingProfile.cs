using AutoMapper;
using NationalPark_API_1003.Models;
using NationalPark_API_1003.Models.DTOs;

namespace NationalPark_API_1003.DTOMapper
{
    public class MappingProfile :Profile
    {
        public MappingProfile()
        {
            CreateMap<NationalPark, NationalParkDto>().ReverseMap();
            CreateMap<Trail, TrailDto>().ReverseMap();
            CreateMap<Appointment, AppointmentDto>().ReverseMap();
        }
    }
}
