using OpenRP.Boilerplate.LegacyFeatures.Skills.Dialogs;
using SampSharp.Entities;
using SampSharp.Entities.SAMP.Commands;
using SampSharp.Entities.SAMP;
using OpenRP.Boilerplate.LegacyFeatures.Players.Helpers;
using OpenRP.Boilerplate.Data;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Features.Players.Extensions;

namespace OpenRP.Boilerplate.LegacyFeatures.Skills.Commands
{
    public class SkillsCommand : ISystem
    {
        [PlayerCommand]
        public void Skills(Player player, IDialogService dialogService, IEntityManager entityManager)
        {
            if (!player.IsPlayerPlayingAsCharacter())
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You don't have a character loaded.");
                return;
            }

            using (var context = new DataContext())
            {
                CharacterModel character = player.GetPlayerCurrentlyPlayingAsCharacterModel();
                // Retrieve skills and character skills from context or predefined list
                SkillsDialog.OpenSkillsDialog(player, dialogService, character);
            }
        }

    }
}
