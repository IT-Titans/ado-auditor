using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Reports.Design;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Rules;

public class AffectedRulesSectionComponent : IComponent
{
    private readonly RuleDto[] _model;

    public AffectedRulesSectionComponent(RuleDto[] model)
    {
        Guard.IsNotNull(model);

        _model = model;
    }

    public void Compose(IContainer container)
    {
        container
            .PaddingBottom(ComponentStyles.Sections.PaddingBottom())
            .Column(column =>
            {
                column
                    .Item()
                    .Component(new SectionHeadingComponent(Properties.Reports.AffectedRulesSection_Title, Headings.H3));

                column
                    .Item()
                    .Component(new AffectedRuleTableComponent(_model));
                // foreach (var rule in _model.OrderBySeverity())
                // {
                //     column
                //         .Item()
                //         .Component(new AffectedRuleEntryComponent(rule));
                // }
            });
    }
}