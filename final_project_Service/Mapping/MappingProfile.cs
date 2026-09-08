using AutoMapper;
using final_project_Core.DTO;
using final_project_Core.Entities;

namespace final_project_Service.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ChargingSpot, ChargingSpotDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.StationName, opt => opt.MapFrom(src => src.Station != null ? src.Station.Name : ""));

            CreateMap<ChargingSession, ChargingSessionDto>()
                .ForMember(dest => dest.StationName, opt => opt.MapFrom(
                    src => src.Spot != null && src.Spot.Station != null ? src.Spot.Station.Name : ""));

            CreateMap<StationSummary, StationDto>();
            CreateMap<Amenity, AmenityDto>();
            CreateMap<Announcement, AnnouncementDto>();

            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.StationName, opt => opt.MapFrom(src => src.Station != null ? src.Station.Name : ""));
        }
    }
}
