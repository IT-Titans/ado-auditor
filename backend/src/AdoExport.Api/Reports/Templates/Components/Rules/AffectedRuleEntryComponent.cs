using AdoExport.Api.Controllers.Models;

using Humanizer;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Rules;

public class AffectedRuleEntryComponent : IComponent
{
    private readonly RuleDto _model;

    public AffectedRuleEntryComponent(RuleDto model)
    {
        Guard.IsNotNull(model);

        _model = model;
    }

    public void Compose(IContainer container)
    {
        container
            .PaddingTop(12)
            .SectionLink(Sections.RuleSectionId(_model.Id.ToString()))
            .Row(row =>
            {
                row
                    .RelativeItem()
                    .AlignLeft()
                    .PaddingRight(4)
                    .Column(column =>
                    {
                        column
                            .Item()
                            .AlignBottom()
                            .Text(_model.Name)
                            .SemiBold();
                    });

                row
                    .AutoItem()
                    .AlignRight()
                    .PaddingRight(4)
                    .Component(new SeverityIconComponent(_model.Severity, _model.Severity.Humanize()));
            });
    }
}