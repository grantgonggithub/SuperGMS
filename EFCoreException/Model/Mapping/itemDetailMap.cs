using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TestModel.Models.Mapping
{
    public class itemDetailMap : IEntityTypeConfiguration<itemDetail>
    {
        public void Configure(EntityTypeBuilder<itemDetail> builder)
        {
            // Primary Key
            builder.HasKey(t => new { t.ItemGID,t.LineId});
            //builder.HasKey(t => new { t.GUID });
            // Properties
            builder.Property(t => t.GUID)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.ItemGID)
                .HasMaxLength(100);
            builder.Property(t => t.LineName)
                .HasMaxLength(100);

            // Table & Column Mappings
            builder.ToTable("item_detail");
            builder.Property(t => t.ItemGID).HasColumnName("ItemGID");
            builder.Property(t => t.GUID).HasColumnName("GUID");
            builder.Property(t => t.LineName).HasColumnName("LineName");
            builder.Property(t => t.LineId).HasColumnName("LINE_ID");
            // Relationships
            builder.HasOne(t => t.item)
                .WithMany(t => t.itemDetail)
                .HasForeignKey(d => d.ItemGID);
            builder.Property(t => t.TS)
         .ValueGeneratedOnUpdate().IsConcurrencyToken();
        }
    }
}
