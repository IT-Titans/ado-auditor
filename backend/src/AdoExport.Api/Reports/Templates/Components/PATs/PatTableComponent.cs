using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Controllers.Sorting;
using AdoExport.Api.Reports.Design;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.PATs;

public class PatTableComponent : IComponent
{
    private readonly PatDetailsDto[] _model;

    private readonly bool _showUser;

    public PatTableComponent(PatDetailsDto[] model, bool showUser)
    {
        Guard.IsNotNull(model);

        _model = model;
        _showUser = showUser;
    }

    public void Compose(IContainer container)
    {
        container
            .PaddingBottom(ComponentStyles.Sections.PaddingBottom())
            .Column(column =>
            {
                if (_model.Length == 0)
                {
                    column
                        .Item()
                        .PaddingVertical(8)
                        .AlignLeft()
                        .Text(Properties.Reports.Pats_No_Pats_Found);
                }
                else
                {
                    column
                        .Item()
                        .Table(table =>
                        {
                            const float borderWidth = 0.5f;

                            DefineColumns(table);

                            AddHeader(table, borderWidth);

                            AddCells(table);
                        });
                }
            });

        return;

        void DefineColumns(TableDescriptor table)
        {
            table
                .ColumnsDefinition(columns =>
                {
                    // Name
                    columns.RelativeColumn();

                    if (_showUser)
                    {
                        // User
                        columns.ConstantColumn(80);
                    }

                    // Scopes
                    columns.ConstantColumn(60);

                    // Created At
                    columns.ConstantColumn(80);

                    // User Criticals
                    columns.ConstantColumn(40);
                    // User Warnings
                    columns.ConstantColumn(40);
                    // User Oks
                    columns.ConstantColumn(40);
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
                    .Text(Properties.Reports.PatsTable_Heading_Name)
                    .Style(Typography.TableHeading);

                if (_showUser)
                {
                    header
                        .Cell()
                        .PaddingBottom(tableHeaderPaddingBottom)
                        .BorderBottom(borderWidth)
                        .Text(Properties.Reports.PatsTable_Heading_User)
                        .Style(Typography.TableHeading);
                }

                header
                    .Cell()
                    .PaddingBottom(tableHeaderPaddingBottom)
                    .BorderBottom(borderWidth)
                    .Text(Properties.Reports.PatsTable_Heading_Scopes)
                    .Style(Typography.TableHeading);

                header
                    .Cell()
                    .PaddingBottom(tableHeaderPaddingBottom)
                    .BorderBottom(borderWidth)
                    .Text(Properties.Reports.PatsTable_Heading_CreatedAt)
                    .Style(Typography.TableHeading);

                header
                    .Cell()
                    .ColumnSpan(3)
                    .PaddingBottom(tableHeaderPaddingBottom)
                    .BorderBottom(borderWidth)
                    .Row(row =>
                    {
                        row
                            .RelativeItem()
                            .AlignCenter()
                            .Component(new SeverityIconComponent(Severities.Critical));
                        row
                            .RelativeItem()
                            .AlignCenter()
                            .Component(new SeverityIconComponent(Severities.Warning));
                        row
                            .RelativeItem()
                            .AlignCenter()
                            .Component(new SeverityIconComponent(Severities.Ok));
                    });
            });
        }

        void AddCells(TableDescriptor table)
        {
            foreach (var pat in _model.OrderByTotals())
            {
                table
                    .Cell()
                    .PreventPageBreak()
                    .PaddingBottom(2)
                    .AlignLeft()
                    .AlignTop()
                    .SectionLink(Sections.PatByIdSectionId(pat.Id))
                    .Text(pat.Name);

                if (_showUser)
                {
                    table
                        .Cell()
                        .PreventPageBreak()
                        .PaddingBottom(2)
                        .AlignLeft()
                        .AlignTop()
                        .SectionLink(Sections.UserOverviewSectionId(pat.User.UserId))
                        .Text(pat.User.Name);
                }

                table
                    .Cell()
                    .PreventPageBreak()
                    .PaddingBottom(2)
                    .AlignLeft()
                    .AlignTop()
                    .Component(new PatScopesComponent(pat.Scopes));

                table
                    .Cell()
                    .PreventPageBreak()
                    .PaddingBottom(2)
                    .AlignLeft()
                    .AlignTop()
                    .Text(DateTime.Parse(pat.CreatedAt).FormatToDateWithTime());

                table
                    .Cell()
                    .PreventPageBreak()
                    .PaddingBottom(2)
                    .AlignCenter()
                    .AlignTop()
                    .Component(new AffectedCountComponent(Severities.Critical, pat.AffectedPatRulesCounts.TotalCriticals));
                table
                    .Cell()
                    .PreventPageBreak()
                    .PaddingBottom(2)
                    .AlignCenter()
                    .AlignTop()
                    .Component(new AffectedCountComponent(Severities.Warning, pat.AffectedPatRulesCounts.TotalWarnings));
                table
                    .Cell()
                    .PreventPageBreak()
                    .PaddingBottom(2)
                    .AlignCenter()
                    .AlignTop()
                    .Component(new AffectedCountComponent(Severities.Ok, pat.AffectedPatRulesCounts.TotalOks));
            }
        }
    }
}