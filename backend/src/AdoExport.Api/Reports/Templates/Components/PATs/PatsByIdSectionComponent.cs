using AdoExport.Api.Controllers.Sorting;
using AdoExport.Api.Reports.Design;
using AdoExport.Api.Reports.Models;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.PATs;

public class PatsByIdSectionComponent : IComponent
{
    private readonly string _sectionId;

    private readonly PatsById _model;

    public PatsByIdSectionComponent(string sectionId, PatsById model)
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
                    .Component(new SectionHeadingComponent(Properties.Reports.Pat_Single_Section_Title, Headings.H2));

                foreach (var pat in _model.Pats.Values.OrderByTotals())
                {
                    column
                        .Item()
                        .Component(new PatSectionComponent(pat));
                }
            });
    }
}