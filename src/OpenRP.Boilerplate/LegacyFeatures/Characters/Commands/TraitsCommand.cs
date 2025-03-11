using OpenRP.Boilerplate.LegacyFeatures.Characters.Services;
using SampSharp.Entities.SAMP.Commands;
using SampSharp.Entities.SAMP;
using SampSharp.Entities;

namespace OpenRP.Boilerplate.LegacyFeatures.Characters.Commands
{
    public class TraitsCommand : ISystem
    {
        [PlayerCommand]
        public void Traits(Player player, ICharacterService characterManager)
        {
            characterManager.OpenCharacterTraitsDialog(player);
        }
    }
}
