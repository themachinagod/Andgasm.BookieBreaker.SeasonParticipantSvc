using AutoMapper;

namespace Andgasm.BookieBreaker.SeasonParticipant.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            MapHelpInstructionToResource();
        }

        private void MapHelpInstructionToResource()
        {
            //CreateMap<Club, HelpInstructionResource>()
            //    .ForMember(dest => dest.InternalId,
            //               src => src.MapFrom(y => y.Id))
            //    .ForMember(dest => dest.ExternalId,
            //               src => src.MapFrom(y => y.ExternalId))
            //    .ForMember(dest => dest.HostKey,
            //               src => src.MapFrom(y => y.HostKey))
            //    .ForMember(dest => dest.LookupKey,
            //               src => src.MapFrom(y => y.LookupKey))
            //    .ForMember(dest => dest.TooltipText,
            //               src => src.MapFrom(y => y.TooltipText))
            //    .ReverseMap();
        }
    }
}
