namespace Transaction.Worker.Services;

internal class CategoryConfig
{
    public string Name { get; set; } = default!;
    public string[] Keywords { get; set; } = [];
}

public class TransactionCategorizer : ITransactionCategorizer
{
    private readonly List<CategoryConfig> _configs;
    private readonly string _fallback;

    public TransactionCategorizer(IConfiguration configuration)
    {
        // Read configuration section "Categorization" as a simple list of { Name, Keywords[] }
        _configs = configuration.GetSection("Categorization").Get<List<CategoryConfig>>() ?? [];

        // Determine fallback category name (explicitly "Uncategorized" if present, otherwise last configured)
        _fallback = _configs.FirstOrDefault(c => string.Equals(c.Name, "Uncategorized", StringComparison.OrdinalIgnoreCase))?.Name
                    ?? _configs.LastOrDefault()?.Name
                    ?? "Uncategorized";
    }

    public string GetCategoryName(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return _fallback;

        var text = description.ToLowerInvariant();

        // Iterate categories in configuration order and find first matching keyword
        foreach (var cfg in _configs)
        {
            if (cfg.Keywords == null || cfg.Keywords.Length == 0)
                continue;

            foreach (var kw in cfg.Keywords)
            {
                if (string.IsNullOrWhiteSpace(kw))
                    continue;

                if (text.Contains(kw.ToLowerInvariant()))
                    return cfg.Name;
            }
        }

        return _fallback;
    }
}