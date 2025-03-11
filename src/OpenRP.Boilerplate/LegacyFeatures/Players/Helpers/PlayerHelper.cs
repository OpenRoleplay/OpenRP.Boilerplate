using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Accounts.Components;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Features.Players.Extensions;
using OpenRP.Framework.Features.CDN.Services;

namespace OpenRP.Boilerplate.LegacyFeatures.Players.Helpers
{
    public static class PlayerHelper
    {
        public static CharacterModel GetPlayerCurrentlyPlayingAsCharacterModel(this Player player)
        {
            Account account = player.GetPlayerCurrentlyLoggedInAccount();

            // Check if the player has a Character
            Character accountCharacter = account.GetComponent<Character>();

            // Try to fetch the character
            var character = accountCharacter.GetCharacterModel();

            return character;
        }

        public static void PlayOpenCdnStream(this Player player, IOpenCdnService cdnService, string subDir, string path)
        {
            player.PlayAudioStream(cdnService.GetLink(subDir, path));
        }

        public static void PlayOpenCdnStream(this Player player, IOpenCdnService cdnService, string subDir, string path, Vector3 position, float range)
        {
            player.PlayAudioStream(cdnService.GetLink(subDir, path), position, range);
        }
    }
}
