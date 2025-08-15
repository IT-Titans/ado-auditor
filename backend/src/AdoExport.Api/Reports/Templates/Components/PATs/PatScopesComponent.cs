using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace AdoExport.Api.Reports.Templates.Components.PATs;

public class PatScopesComponent : IComponent
{
    private readonly string[] _scopes;

    public PatScopesComponent(string[] scopes)
    {
        _scopes = scopes;
    }

    public void Compose(IContainer container)
    {
        container
            .Column(column =>
            {
                foreach (var scope in _scopes)
                {
                    column
                        .Item()
                        .Text(scope);
                }
            });
    }
}