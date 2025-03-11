using SampSharp.Streamer.Entities;

namespace OpenRP.Boilerplate.LegacyFeatures.ChickenCoop.Services
{
    public interface IChickenCoopManager
    {
        ChickenCoop.Components.ChickenCoop CreateChickenCoop(DynamicObject dynamicObject);
        IEnumerable<ChickenCoop.Components.ChickenCoop> GetAllChickenCoops();
    }
}
