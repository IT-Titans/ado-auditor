using AdoExport.Api.Controllers.Sorting;
using AdoExport.Api.Reports.Design;
using AdoExport.Api.Reports.Models;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Users;

public class UsersByIdSectionComponent : IComponent
{
    private readonly string _sectionId;

    private readonly UsersById _model;

    public UsersByIdSectionComponent(string sectionId, UsersById model)
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
                foreach (var user in _model.Users.Values.OrderByName())
                {
                    column
                        .Item()
                        .Component(new UserSectionComponent(user));
                }
            });
    }
}