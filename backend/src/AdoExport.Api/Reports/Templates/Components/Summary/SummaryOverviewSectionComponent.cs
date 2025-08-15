using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Controllers.Sorting;
using AdoExport.Api.Reports.Design;
using AdoExport.Api.Reports.Models;

using Humanizer;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Summary;

public class SummaryOverviewSectionComponent : IComponent
{
    private readonly string _sectionId;

    private readonly UsersOverview _model;

    public SummaryOverviewSectionComponent(string sectionId, UsersOverview model)
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
                    .Component(new SectionHeadingComponent(Properties.Reports.UsersOverviewSection_Title, Headings.H1));

                column
                    .Item()
                    .Table(table =>
                    {
                        const float borderWidth = 0.5f;

                        DefineColumns(table);

                        AddHeader(table, borderWidth);

                        AddCells(table);
                    });
            });

        return;

        void DefineColumns(TableDescriptor table)
        {
            table
                .ColumnsDefinition(columns =>
                {
                    // Name
                    columns.RelativeColumn();

                    // Member Type
                    columns.ConstantColumn(80);

                    // IsActive
                    columns.ConstantColumn(60);

                    // User Criticals
                    columns.ConstantColumn(50);
                    // User Warnings
                    columns.ConstantColumn(50);
                    // User Oks
                    columns.ConstantColumn(50);
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
                    .Text(Properties.Reports.UsersTable_Heading_Name)
                    .Style(Typography.TableHeading);

                header
                    .Cell()
                    .PaddingBottom(tableHeaderPaddingBottom)
                    .BorderBottom(borderWidth)
                    .Text(Properties.Reports.UsersTable_Heading_MemberType)
                    .Style(Typography.TableHeading);

                header
                    .Cell()
                    .PaddingBottom(tableHeaderPaddingBottom)
                    .BorderBottom(borderWidth)
                    .Text(Properties.Reports.UsersTable_Heading_IsActive)
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
            foreach (var user in _model.Users.OrderByTotals())
            {
                table
                    .Cell()
                    .PaddingBottom(2)
                    .AlignLeft()
                    .AlignTop()
                    .SectionLink(Sections.UserOverviewSectionId(user.Id))
                    .Text(user.UserName);

                table
                    .Cell()
                    .PaddingBottom(2)
                    .AlignLeft()
                    .AlignTop()
                    .Text(user.MetaType.Humanize(LetterCasing.Title));

                table
                    .Cell()
                    .PaddingBottom(2)
                    .AlignLeft()
                    .AlignTop()
                    .Text(user.IsActive ? Properties.Reports.User_IsActive : Properties.Reports.User_IsInactive);

                table
                    .Cell()
                    .PaddingBottom(2)
                    .AlignCenter()
                    .AlignTop()
                    .Component(new AffectedCountComponent(Severities.Critical, user.AffectedUserRulesCounts.TotalCriticals));
                table
                    .Cell()
                    .PaddingBottom(2)
                    .AlignCenter()
                    .AlignTop()
                    .Component(new AffectedCountComponent(Severities.Warning, user.AffectedUserRulesCounts.TotalWarnings));
                table
                    .Cell()
                    .PaddingBottom(2)
                    .AlignCenter()
                    .AlignTop()
                    .Component(new AffectedCountComponent(Severities.Ok, user.AffectedUserRulesCounts.TotalOks));
            }
        }
    }
}