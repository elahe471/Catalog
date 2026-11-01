namespace Catalog.Models
{
    public sealed class CatalogRemind
    {
        public const string TableName = "CatalogReminds";

        public string Slug { get; private set; } = null!;
        public Guid UserId { get; private set; }
        public DateTime CreatedDate { get; private set; }

        private CatalogRemind() { }

        public CatalogRemind(string slug, Guid userId)
        {
            Slug = slug;
            UserId = userId;
            CreatedDate = DateTime.UtcNow;
        }
    }
}
