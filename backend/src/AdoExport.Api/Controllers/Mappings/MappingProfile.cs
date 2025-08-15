using AdoExport.Auditing;

using AutoMapper;

namespace AdoExport.Api.Controllers.Mappings;

/// <summary>
/// An implicitly used mapping profile that is automatically registered in the DI container.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<SummaryItem, Models.AuditSummaryCategoryItemDto>();
        CreateMap<Severities, Models.Severities>();
        CreateMap<Categories, Models.Categories>();
    }
}