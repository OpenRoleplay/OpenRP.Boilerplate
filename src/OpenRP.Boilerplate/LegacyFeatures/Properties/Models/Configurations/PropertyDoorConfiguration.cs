using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OpenRP.Framework.Database.Models;

namespace OpenRP.Boilerplate.LegacyFeatures.Properties.Models.Configurations
{
    public class PropertyDoorConfiguration : IEntityTypeConfiguration<PropertyDoorModel>
    {
        public void Configure(EntityTypeBuilder<PropertyDoorModel> builder)
        {
            // Specify the table name if desired.
            builder.ToTable("PropertyDoors");

            // Configure the primary key.
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id)
                   .ValueGeneratedOnAdd();

            // Configure the Name property.
            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(100); // Adjust the max length as needed.

            // Configure simple scalar properties.
            builder.Property(p => p.X)
                   .IsRequired();
            builder.Property(p => p.Y)
                   .IsRequired();
            builder.Property(p => p.Z)
                   .IsRequired();
            builder.Property(p => p.Angle)
                   .IsRequired();
            // Interior is optional so no extra configuration is strictly required.

            // Configure the relationship to the PropertyModel.
            // (Assumes that PropertyModel has a collection navigation property called "Doors".)
            builder.HasOne(p => p.Property)
                   .WithMany(p => p.PropertyDoors) // Change "Doors" to whatever name is used in PropertyModel.
                   .HasForeignKey(p => p.PropertyId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Configure the self-referencing one-to-one relationship for LinkedToPropertyDoor.
            builder.HasOne(p => p.LinkedToPropertyDoor)
                   .WithOne() // No inverse navigation property on the linked door.
                   .HasForeignKey<PropertyDoorModel>(p => p.LinkedToPropertyDoorId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
