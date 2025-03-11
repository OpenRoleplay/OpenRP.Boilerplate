using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OpenRP.Framework.Database.Models;

namespace OpenRP.Boilerplate.LegacyFeatures.Properties.Models.Configurations
{
    public class PropertyConfiguration : IEntityTypeConfiguration<PropertyModel>
    {
        public void Configure(EntityTypeBuilder<PropertyModel> builder)
        {
            // Specify the table name if desired.
            builder.ToTable("Properties");

            // Configure the primary key.
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                   .ValueGeneratedOnAdd();

            // Configure the Name property.
            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(100); // Adjust the maximum length as needed.

            // Configure the one-to-many relationship with PropertyDoorModel.
            // This assumes that PropertyModel.PropertyDoors is the collection navigation property,
            // and that PropertyDoorModel has a navigation property "Property" and a foreign key "PropertyId".
            builder.HasMany(p => p.PropertyDoors)
                   .WithOne(d => d.Property)
                   .HasForeignKey(d => d.PropertyId)
                   .OnDelete(DeleteBehavior.Cascade);  // Adjust delete behavior if needed.
        }
    }
}
