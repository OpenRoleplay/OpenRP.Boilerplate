using MySqlConnector;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Accounts.Components;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Features.Accounts.Services;
using OpenRP.Boilerplate.Configuration;

namespace OpenRP.Boilerplate.LegacyFeatures.Characters.Helpers
{
    public static class CharacterHelper
    {
        public static bool CreateCharacter(IAccountService accountService, Player player)
        {
            try
            {
                CharacterCreation charCreationComponent = player.GetComponent<CharacterCreation>();
                Account accountComponent = player.GetComponent<Account>();

                if (charCreationComponent != null && charCreationComponent.CreatingCharacter != null)
                {
                    MySqlConnection sqlConnecton = new MySqlConnection(ConfigManager.Instance.Data.ConnectionString);
                    sqlConnecton.Open();

                    MySqlCommand query = new MySqlCommand(@"INSERT INTO Characters (FirstName, MiddleName, LastName, DateOfBirth, Accent, InventoryId, AccountId) VALUES (@FirstName, @MiddleName, @LastName, @DateOfBirth, @Accent, @InventoryId, @AccountId)", sqlConnecton);

                    query.Parameters.AddWithValue("@FirstName", charCreationComponent.CreatingCharacter.FirstName);
                    query.Parameters.AddWithValue("@MiddleName", charCreationComponent.CreatingCharacter.MiddleName);
                    query.Parameters.AddWithValue("@LastName", charCreationComponent.CreatingCharacter.LastName);
                    query.Parameters.AddWithValue("@DateOfBirth", charCreationComponent.CreatingCharacter.DateOfBirth);
                    query.Parameters.AddWithValue("@Accent", null);
                    query.Parameters.AddWithValue("@InventoryId", null);
                    query.Parameters.AddWithValue("@AccountId", accountComponent.GetAccountId());
                    query.ExecuteNonQuery();

                    sqlConnecton.Close();

                    player.DestroyComponents<CharacterCreation>();

                    accountService.ReloadAccount(player, accountComponent.GetAccountId());

                    return true;
                }
                else
                {
                    Console.WriteLine("There is no character to create!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }

        public static string GetCharacterName(this CharacterModel character)
        {
            return String.Format("{0} {1}", character.FirstName, character.LastName);
        }

        public static Player GetCharacterPlayer(this CharacterModel character, IEntityManager entityManager)
        {
            foreach (Player player in entityManager.GetComponents<Player>().Where(p => p.GetComponent<Character>()?.GetCharacterModel().Id == character.Id))
            {
                return player;
            }
            return null;
        }
    }
}
