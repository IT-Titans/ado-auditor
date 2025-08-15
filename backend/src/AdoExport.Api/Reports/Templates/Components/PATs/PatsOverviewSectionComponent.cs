using AdoExport.Api.Reports.Design;
using AdoExport.Api.Reports.Models;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.PATs;

public class PatsOverviewSectionComponent : IComponent
{
    private readonly string _sectionId;

    private readonly PatsOverview _model;

    public PatsOverviewSectionComponent(string sectionId, PatsOverview model)
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
                    .Component(new SectionHeadingComponent(Properties.Reports.PatsOverviewSection_Title, Headings.H1));

                column
                    .Item()
                    .Component(new SectionHeadingComponent(Properties.Reports.User_Pats_Section_Title, Headings.H2));

                column
                    .Item()
                    .Component(new PatTableComponent(_model.Pats, showUser: false));
            });
    }
}