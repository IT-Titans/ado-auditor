using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components;

public class DetailLabelComponent : IComponent
{
    private readonly string _label;

    private readonly string _value;

    public DetailLabelComponent(string label, string value)
    {
        _label = label;
        _value = value;
    }

    public void Compose(IContainer container)
    {
        container.Text(text =>
        {
            text
                .Span($"{_label}")
                .Bold();
            text.Span("   ");
            text.Span(_value);
        });
    }
}