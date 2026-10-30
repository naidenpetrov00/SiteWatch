namespace Application.ProductCatalog;

public sealed class ProductCatalogConflictException : Exception
{
    public ProductCatalogConflictException(string message) : base(message)
    {
    }

    public ProductCatalogConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
