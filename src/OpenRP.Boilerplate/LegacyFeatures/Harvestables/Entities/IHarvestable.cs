using SampSharp.Entities.SAMP;

namespace OpenRP.Boilerplate.LegacyFeatures.Harvestables.Entities
{
    public interface IHarvestable
    {
        /// <summary>
        /// The name of the resource (e.g. "hemp").
        /// </summary>
        string ResourceName { get; }

        /// <summary>
        /// Performs the harvest logic for this resource.
        /// </summary>
        /// <param name="player">The player harvesting.</param>
        void Harvest(Player player);

        /// <summary>
        /// Performs the harvested logic for this resource.
        /// </summary>
        /// <param name="player">The player harvesting.</param>
        void Harvested(Player player);
    }
}
