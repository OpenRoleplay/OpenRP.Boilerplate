using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OpenRP.Framework.Database.Models;

namespace OpenRP.Boilerplate.LegacyFeatures.Properties.Models.DefaultData
{
    public class PropertyDefaultData : IEntityTypeConfiguration<PropertyModel>
    {
        public void Configure(EntityTypeBuilder<PropertyModel> builder)
        {
            builder.HasData(
                new PropertyModel()
                {
                    Id = 1,
                    Name = "Palomino Creek Little Lady"
                }
            );
        }
    }
}
