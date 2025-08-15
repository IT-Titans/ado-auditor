using AdoExport.Api.Reports.Design;
using AdoExport.Api.Reports.Templates.Components.Rules;
using AdoExport.Api.Repositories;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.PATs;

public class PatSectionComponent : IComponent
{
    private readonly GetPatById _model;

    public PatSectionComponent(GetPatById model)
    {
        Guard.IsNotNull(model);

        _model = model;
    }

    public void Compose(IContainer container)
    {
        var pat = _model.Pat;
        var rules = _model.Rules;
        container
            .PaddingBottom(ComponentStyles.Sections.PaddingBottom())
            .Section(Sections.PatByIdSectionId(pat.Id))
            .Column(column =>
            {
                column
                    .Item()
                    .Component(new SectionHeadingComponent($"{Properties.Reports.PatOverviewSection_Title} - {pat.Name}", Headings.H2));

                column
                    .Item()
                    .Component(new PatDetailsSectionComponent(pat));

                column
                    .Item()
                    .Component(new AffectedRulesSectionComponent(rules));
            });
    }
}