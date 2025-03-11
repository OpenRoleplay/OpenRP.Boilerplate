using OpenRP.Boilerplate.LegacyFeatures.Characters.Services;
using SampSharp.Entities;
using SampSharp.Entities.SAMP.Commands;
using SampSharp.Entities.SAMP;

namespace OpenRP.Boilerplate.LegacyFeatures.Characters.Commands
{
    public class PreferencesCommand : ISystem
    {
        [PlayerCommand]
        public void CharacterPreferences(Player player, ICharacterService characterManager)
        {
            characterManager.OpenCharacterPreferencesDialog(player);
        }
    }
}
