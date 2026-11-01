namespace Catalog.Infrastructure.EntityConfigurations
{
    public sealed class CatalogRemindEntityTypeConfiguration : IEntityTypeConfiguration<CatalogRemind>
    {
        public void Configure(EntityTypeBuilder<CatalogRemind> builder)
        {
            builder.ToTable(CatalogRemind.TableName);

            builder.HasKey(x => new { x.Slug, x.UserId });

            builder.Property(x => x.Slug)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.CreatedDate).IsRequired();
        }
    }
}
