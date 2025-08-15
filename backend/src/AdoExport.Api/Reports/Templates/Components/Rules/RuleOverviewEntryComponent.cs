using AdoExport.Api.Controllers.Models;

using Humanizer;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Rules;

public class RuleOverviewEntryComponent : IComponent
{
    private readonly RuleDto _model;

    public RuleOverviewEntryComponent(RuleDto model)
    {
        Guard.IsNotNull(model);

        _model = model;
    }

    public void Compose(IContainer container)
    {
        container
            .PreventPageBreak()
            .PaddingTop(12)
            .Section(Sections.RuleSectionId(_model.Id.ToString()))
            .Column(column =>
            {
                column
                    .Item()
                    .Row(row =>
                    {
                        row
                            .RelativeItem()
                            .AlignLeft()
                            .AlignBottom()
                            .Text(_model.Name)
                            .SemiBold();

                        row
                            .AutoItem()
                            .AlignRight()
                            .PaddingRight(4)
                            .Component(new SeverityIconComponent(_model.Severity, _model.Severity.Humanize()));

                        row
                            .AutoItem()
                            .AlignRight()
                            .Component(new AffectedCountComponent(_model.Severity, _model.TotalCounts));
                    });

                column
                    .Item()
                    .PaddingTop(6)
                    .Text(_model.Description);

                column
                    .Item()
                    .PaddingTop(6)
                    .Text(Properties.Reports.Rule_Label_Recommendation)
                    .SemiBold();
                column
                    .Item()
                    .PaddingBottom(4)
                    .Text(_model.Recommendation);
            });
    }
}