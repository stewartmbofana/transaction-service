namespace Transaction.Ingestor.Services;

public interface ITransactionCategorizer
{
    /// <summary>
    /// Returns the configured category name that best matches <paramref name="description"/>.
    /// If none match, returns the configured fallback category name (e.g. "Uncategorized").
    /// </summary>
    string GetCategoryName(string description);
}