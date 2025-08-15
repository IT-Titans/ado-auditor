using AdoExport.Api.Controllers.Sorting;
using AdoExport.Api.Reports.Design;
using AdoExport.Api.Reports.Models;

using Humanizer;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Layout.TableOfContents;

public class TableOfContentsComponent : IComponent
{
    private readonly AuditPdfModel _model;

    public TableOfContentsComponent(AuditPdfModel model)
    {
        Guard.IsNotNull(model);

        _model = model;
    }

    public void Compose(IContainer container)
    {
        container
            .Decoration(decoration =>
            {
                decoration
                    .Before()
                    .PaddingBottom(ComponentStyles.Sections.PaddingBottom())
                    .Text(Properties.Reports.Toc_Headline)
                    .Style(Typography.Headline);

                decoration.Content().Column(column =>
                {
                    column.Spacing(5);

                    // starts at 1 as it is for display purposes
                    var itemCounter = 1;

                    itemCounter = DrawRules(column, itemCounter);

                    itemCounter = DrawUsers(column, itemCounter);

                    _ = DrawPats(column, itemCounter);
                });
            });
    }

    private int DrawUsers(ColumnDescriptor column, int itemCounter)
    {
        // All users overview
        column
            .Item()
            .Component(new TableOfContentsItem(new TableOfContentsItemModel()
            {
                Indentation = 0,
                ItemNumber = itemCounter++,
                SectionId = Sections.UsersOverviewSectionId(),
                Name = Properties.Reports.UsersOverviewSection_Title,
            }));

        // User details
        foreach (var user in _model.UsersOverview.Users.OrderByName())
        {
            column
                .Item()
                .Component(new TableOfContentsItem(new TableOfContentsItemModel()
                {
                    Indentation = 1,
                    ItemNumber = itemCounter++,
                    SectionId = Sections.UserOverviewSectionId(user.Id),
                    Name = user.UserName,
                }));
        }

        return itemCounter;
    }

    private int DrawRules(ColumnDescriptor column, int itemCounter)
    {
        // All rules overview
        column
            .Item()
            .Component(new TableOfContentsItem(new TableOfContentsItemModel()
            {
                Indentation = 0,
                ItemNumber = itemCounter++,
                SectionId = Sections.RulesOverviewSectionId(),
                Name = Properties.Reports.RulesOverviewSection_Title,
            }));

        foreach (var rulesByScope in _model.RulesOverview.Rules.GroupBy(rule => rule.Scope))
        {
            // Rule scopes
            var scope = rulesByScope;
            column
                .Item()
                .Component(new TableOfContentsItem(new TableOfContentsItemModel()
                {
                    Indentation = 1,
                    ItemNumber = itemCounter++,
                    SectionId = Sections.RuleCategoryId(scope.Key),
                    Name = scope.Key.Humanize(),
                }));

            // Rule details
            foreach (var rule in rulesByScope)
            {
                column
                    .Item()
                    .Component(new TableOfContentsItem(new TableOfContentsItemModel()
                    {
                        Indentation = 2,
                        ItemNumber = itemCounter++,
                        SectionId = Sections.RuleSectionId(rule.Id.ToString()),
                        Name = rule.Name,
                    }));
            }
        }

        return itemCounter;
    }

    private int DrawPats(ColumnDescriptor column, int itemCounter)
    {
        // All PATs overview
        column
            .Item()
            .Component(new TableOfContentsItem(new TableOfContentsItemModel()
            {
                Indentation = 0,
                ItemNumber = itemCounter++,
                SectionId = Sections.PatsOverviewSectionId(),
                Name = Properties.Reports.PatsOverviewSection_Title,
            }));

        foreach (var patById in _model.PatsById.Pats.Values.OrderByTotals())
        {
            // PAT details
            column
                .Item()
                .Component(new TableOfContentsItem(new TableOfContentsItemModel()
                {
                    Indentation = 1,
                    ItemNumber = itemCounter++,
                    SectionId = Sections.PatByIdSectionId(patById.Pat.Id),
                    Name = patById.Pat.Name,
                }));
        }

        return itemCounter;
    }
}