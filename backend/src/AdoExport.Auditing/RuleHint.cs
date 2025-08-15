namespace AdoExport.Auditing;

public class RuleHint
{
    private readonly string? _description;

    private readonly string? _recommendations;

    public required string Name { get; init; }

    public string Description
    {
        get => _description ?? "Coming soon! We're still working on providing you helpful information about this rule!";
        init => _description = value;
    }
    public string Recommendations
    {
        get => _recommendations ?? "Coming soon! We're still working on providing you helpful recommendations to follow best practices!";
        init => _recommendations = value;
    }
}