using AdoExport.Api.Controllers.Sorting;
using AdoExport.Api.Reports.Design;
using AdoExport.Api.Reports.Models;

using Humanizer;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Rules;

public class RulesOverviewSectionComponent : IComponent
{
    private readonly string _sectionId;

    private readonly RulesOverview _model;

    public RulesOverviewSectionComponent(string sectionId, RulesOverview model)
    {
        Guard.IsNotNullOrWhiteSpace(sectionId);
        Guard.IsNotNull(model);

        _sectionId = sectionId;
        _model = model;
    }

    public void Compose(IContainer container)
    {
        container
            .PaddingBottom(ComponentStyles.Sections.PaddingBottom())
            .Section(_sectionId)
            .Column(column =>
            {
                column
                    .Item()
                    .Component(new SectionHeadingComponent(Properties.Reports.RulesOverviewSection_Title, Headings.H1));

                column
                    .Item()
                    .PaddingBottom(4)
                    .Text("Here is a list of all rules that your organization has been checked against");

                foreach (var rulesByScope in _model.Rules.GroupBy(rule => rule.Scope))
                {
                    column
                        .Item()
                        .Section(Sections.RuleCategoryId(rulesByScope.Key))
                        .Component(new SectionHeadingComponent($"{Properties.Reports.RulesOverviewSection_Title} - {rulesByScope.Key.Humanize()}", Headings.H2));

                    column
                        .Item()
                        .PaddingBottom(4)
                        .Text($"Here is a list of all rules within the scope '{rulesByScope.Key.Humanize()}'");

                    foreach (var rule in rulesByScope.OrderBySeverity())
                    {
                        column
                        .Item()
                        .Component(new RuleOverviewEntryComponent(rule));
                    }
                }
            });
    }
}