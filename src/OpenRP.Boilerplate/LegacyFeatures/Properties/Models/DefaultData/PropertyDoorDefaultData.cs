using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OpenRP.Framework.Database.Models;

namespace OpenRP.Boilerplate.LegacyFeatures.Properties.Models.DefaultData
{
    public class PropertyDoorDefaultData : IEntityTypeConfiguration<PropertyDoorModel>
    {
        public void Configure(EntityTypeBuilder<PropertyDoorModel> builder)
        {
            builder.HasData(
                new PropertyDoorModel()
                {
                    Id = 1,
                    Name = "Entrance",
                    PropertyId = 1,
                    LinkedToPropertyDoorId = 2,
                    X = 2304.29f,
                    Y = 14.25f,
                    Z = 26.4844f,
                    Angle = 91.2858f,
                    Interior = null
                },
                new PropertyDoorModel()
                {
                    Id = 2,
                    Name = "Exit",
                    PropertyId = 1,
                    LinkedToPropertyDoorId = 1,
                    X = 207.072f,
                    Y = -139.896f,
                    Z = 1003.51f,
                    Angle = 357.832f,
                    Interior = 3
                }
            );
        }
    }
}
