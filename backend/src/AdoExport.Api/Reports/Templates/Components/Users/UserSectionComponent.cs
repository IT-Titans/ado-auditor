using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Reports.Design;
using AdoExport.Api.Reports.Templates.Components.PATs;
using AdoExport.Api.Reports.Templates.Components.Rules;
using AdoExport.Api.Repositories;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Users;

public class UserSectionComponent : IComponent
{
    private readonly GetUserById _model;

    public UserSectionComponent(GetUserById model)
    {
        Guard.IsNotNull(model);

        _model = model;
    }

    public void Compose(IContainer container)
    {
        var user = _model.User;
        container
            .PaddingBottom(ComponentStyles.Sections.PaddingBottom())
            .Section(Sections.UserOverviewSectionId(user.Id))
            .Column(column =>
            {
                column
                    .Item()
                    .Component(new SectionHeadingComponent(
                        $"{Properties.Reports.UserOverviewSection_Title} - {user.UserName}",
                        Headings.H2,
                        [
                            new SeverityCounts()
                            {
                                Severity = Severities.Critical,
                                Count = _model.Rules.Count(rule => rule.Severity == Severities.Critical),
                            },
                            new SeverityCounts()
                            {
                                Severity = Severities.Warning,
                                Count = _model.Rules.Count(rule => rule.Severity == Severities.Warning),
                            },
                            new SeverityCounts()
                            {
                                Severity = Severities.Ok,
                                Count = _model.Rules.Count(rule => rule.Severity == Severities.Ok),
                            }
                        ]));

                column
                    .Item()
                    .Component(new UserDetailsSectionComponent(_model.User));

                column
                    .Item()
                    .Component(new AffectedRulesSectionComponent(_model.Rules));

                column
                    .Item()
                    .Component(new SectionHeadingComponent(Properties.Reports.User_Pats_Section_Title, Headings.H3));

                column
                    .Item()
                    .Component(new PatTableComponent(_model.Pats, showUser: false));
            });
    }
}