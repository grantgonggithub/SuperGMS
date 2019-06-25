using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TestModel.Models.Mapping
{
    public class itemMap : IEntityTypeConfiguration<item>
    {

        public void Configure(EntityTypeBuilder<item> builder)
        {
            // Primary Key
            builder.HasKey(t => t.ItemGID);

            // Properties
            builder.Property(t => t.ItemGID)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.ItemID)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.ItemName)
                .HasMaxLength(100);

            builder.Property(t => t.ItemDesc)
                .HasMaxLength(100);

            builder.Property(t => t.UDF01)
                .HasMaxLength(100);

            builder.Property(t => t.UDF02)
                .HasMaxLength(100);

            builder.Property(t => t.UDF03)
                .HasMaxLength(100);

            // Table & Column Mappings
            builder.ToTable("item");
            builder.Property(t => t.ItemGID).HasColumnName("ItemGID");
            builder.Property(t => t.ItemID).HasColumnName("ItemID").IsConcurrencyToken();
            builder.Property(t => t.ItemName).HasColumnName("ItemName");
            builder.Property(t => t.ItemDesc).HasColumnName("ItemDesc");
            builder.Property(t => t.Weight).HasColumnName("Weight").HasColumnType(" decimal(18,6) ");
            builder.Property(t => t.Volumn).HasColumnName("Volumn").HasColumnType(" decimal(18,6) ");
            builder.Property(t => t.UDF01).HasColumnName("UDF01");
            builder.Property(t => t.UDF02).HasColumnName("UDF02");
            builder.Property(t => t.UDF03).HasColumnName("UDF03");
            builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate");
            builder.Property(t => t.UpdatedDate).HasColumnName("UpdatedDate");
            builder.Property(t => t.TS)
                    .ValueGeneratedOnUpdate().IsConcurrencyToken();
        }
    }
}
