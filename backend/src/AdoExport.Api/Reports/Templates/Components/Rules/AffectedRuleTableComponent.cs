using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Controllers.Sorting;
using AdoExport.Api.Reports.Design;

using Humanizer;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Rules;

public class AffectedRuleTableComponent : IComponent
{
    private readonly RuleDto[] _model;

    public AffectedRuleTableComponent(RuleDto[] model)
    {
        Guard.IsNotNull(model);

        _model = model;
    }

    public void Compose(IContainer container)
    {
        container
            .PaddingBottom(ComponentStyles.Sections.PaddingBottom())
            .Table(table =>
            {
                const float borderWidth = 0.5f;

                DefineColumns(table);

                AddHeader(table, borderWidth);

                AddCells(table);
            });

        return;

        void DefineColumns(TableDescriptor table)
        {
            table
                .ColumnsDefinition(columns =>
                {
                    // Name
                    columns.RelativeColumn();

                    // Severity
                    columns.ConstantColumn(80);

                });
        }

        void AddHeader(TableDescriptor table, float borderWidth)
        {
            table.Header(header =>
            {
                const float tableHeaderPaddingBottom = 5;
                header
                    .Cell()
                    .PaddingBottom(tableHeaderPaddingBottom)
                    .BorderBottom(borderWidth)
                    .Text(Properties.Reports.AffectedRulesTable_Heading_Rule)
                    .Style(Typography.TableHeading);

                header
                    .Cell()
                    .PaddingBottom(tableHeaderPaddingBottom)
                    .BorderBottom(borderWidth)
                    .Text(Properties.Reports.AffectedRulesTable_Heading_Severity)
                    .Style(Typography.TableHeading);
            });
        }

        void AddCells(TableDescriptor table)
        {
            foreach (var rule in _model.OrderBySeverity())
            {
                table
                    .Cell()
                    .PreventPageBreak()
                    .PaddingBottom(2)
                    .AlignLeft()
                    .AlignTop()
                    .SectionLink(Sections.RuleSectionId(rule.Id.ToString()))
                    .Text(rule.Name);

                table
                    .Cell()
                    .PreventPageBreak()
                    .PaddingBottom(2)
                    .AlignRight()
                    .AlignTop()
                    .Component(new SeverityIconComponent(rule.Severity, rule.Severity.Humanize()));
            }
        }
    }
}