using AutoMapper;
using SupportAssistant.Api.Contracts.Chats;
using SupportAssistant.Application.Chats;

namespace SupportAssistant.Api.Mapping;

public class DtoProfile : Profile
{
    protected DtoProfile()
    {
        CreateMap<ChatResult, ChatResponse>()
            .ForMember(dst => dst.Answer, opt => opt.MapFrom(src => src.Answer))
            .ForMember(dst => dst.Sources, opt => opt.MapFrom(src => src.Sources))
            .ForMember(dst => dst.EscalationRequired, opt => opt.MapFrom(src => src.EscalationRequired))
            ;
    }
}
