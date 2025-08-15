using AdoExport.Api.Controllers.Models;
using AdoExport.Api.Reports.Design;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.PATs;

public class PatDetailsSectionComponent : IComponent
{
    private readonly PatDetailsDto _model;

    public PatDetailsSectionComponent(PatDetailsDto model)
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
                    .Component(new SectionHeadingComponent(Properties.Reports.PatDetailsSection_Title, Headings.H2));

                column
                    .Item()
                    .Row(row =>
                    {
                        row
                            .RelativeItem()
                            .Column(column =>
                            {
                                column
                                    .Item()
                                    .Component(new DetailLabelComponent(Properties.Reports.Pat_Label_Name, _model.Name));

                                column
                                    .Item()
                                    .Component(new DetailLabelComponent(Properties.Reports.Pat_Label_Scopes, string.Join(' ', _model.Scopes)));

                                column
                                    .Item()
                                    .SectionLink(Sections.UserOverviewSectionId(_model.User.UserId))
                                    .Component(new DetailLabelComponent(Properties.Reports.Pat_Label_CreatedBy, _model.User.Name));
                            });
                        row.ConstantItem(4);
                        row
                            .RelativeItem()
                            .Column(column =>
                            {
                                // todo: show organization
                                // column
                                //     .Item()
                                //     .Component(new DetailLabelComponent(Properties.Reports.Pat_Label_ValidForOrganization, _model));

                                column
                                    .Item()
                                    .Component(new DetailLabelComponent(Properties.Reports.Pat_Label_CreatedAt, DateTime.Parse(_model.CreatedAt).FormatToDateWithTime()));

                                column
                                    .Item()
                                    .Component(new DetailLabelComponent(Properties.Reports.Pat_Label_ExpiresAt, DateTime.Parse(_model.ExpiresIn).FormatToDateWithTime()));
                            });
                    });
            });
    }
}