using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Reports.Design;

using Humanizer;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.Users;

public class UserDetailsSectionComponent : IComponent
{
    private readonly UserDetailsDto _model;

    public UserDetailsSectionComponent(UserDetailsDto model)
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
                    .Component(new SectionHeadingComponent(Properties.Reports.UserDetailsSection_Title, Headings.H3));

                column
                    .Item()
                    .Row(row =>
                    {
                        row
                            .RelativeItem()
                            .Component(new DetailLabelComponent(Properties.Reports.User_Label_Name, _model.UserName));

                        row
                            .RelativeItem()
                            .Component(new DetailLabelComponent(Properties.Reports.User_Label_MemberType, _model.MetaType.Humanize()));

                        row
                            .RelativeItem()
                            .Component(new DetailLabelComponent(Properties.Reports.User_Label_Status, _model.IsActive ? Properties.Reports.User_IsActive : Properties.Reports.User_IsInactive));
                    });
            });
    }
}