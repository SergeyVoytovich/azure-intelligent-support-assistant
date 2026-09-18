using AutoMapper;
using Azure;
using Azure.AI.DocumentIntelligence;
using SupportAssistant.Application.Documents;

namespace SupportAssistant.Infrastructure.Documents;

public class DocumentsProfile : Profile
{
    public DocumentsProfile()
    {
        CreateMap<AnalyzeResult, DocumentAnalysis>()
            .ForMember(dst => dst.Content, opt => opt.MapFrom(src => src.Content ?? string.Empty))
            .ForMember(dst => dst.PageCount, opt => opt.MapFrom(src => src.Pages.Count))
            ;

        CreateMap<Operation<AnalyzeResult>, DocumentAnalysis>()
            .ForMember(dst => dst, opt => opt.MapFrom(src => src.Value))
            ;
    }
}
