using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Characters.Services;
using OpenRP.Framework.Shared.Dialogs.Helpers;
using OpenRP.Framework.Shared.ServerEvents.Services;
using OpenRP.Framework.Features.Accounts.Components;
using OpenRP.Framework.Features.ActorConversations.Services;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Features.Accounts.Services;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Shared;
using OpenRP.Framework.Shared.ServerEvents.Entities.EventArgs;
using OpenRP.Framework.Features.Discord.Services;
using OpenRP.Framework.Features.MainMenu.Services.Dialogs;

namespace OpenRP.Boilerplate.LegacyFeatures.MainMenu.Dialogs
{
    public static class CharacterSelectionDialog
    {
        public static void Open(Player player, IDialogService dialogService, IActorConversationWithPlayerManager actorConversationWithPlayerManager, IMainMenuDialogService mainMenuDialogService, IDiscordService discordService, IServerEventAggregator serverEventAggregator, ITempCharacterService tempCharacterService, IAccountService accountService)
        {
            ListDialog choiceDialog = new ListDialog(DialogHelper.GetTitle("Character Selection"), DialogHelper.Next, DialogHelper.Quit);

            Account accountComponent = player.GetComponent<Account>();
            List<CharacterModel> characterModels = accountService.GetCharactersByAccountId(accountComponent.GetAccountId());

            void CharacterSelectionDialogHandler(ListDialogResponse r)
            {
                if (r.Response == DialogResponse.LeftButton)
                {
                    if (r.ItemIndex == characterModels?.Count)
                    {
                        CreateCharacterFirstNameDialog.Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                    }
                    else {
                        CharacterModel selectedCharacter = characterModels.ElementAt(r.ItemIndex);
                        Character characterComponent = tempCharacterService.ReloadCharacter(player, selectedCharacter.Id);
                        player.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, String.Format("Logged in as {0}{1} {2}{3}!", ChatColor.CornflowerBlue, selectedCharacter.FirstName, selectedCharacter.LastName, ChatColor.White));
                        // player.OnCharacterSelected();

                        // Temporary for testing
                        player.ToggleSpectating(false);
                        player.ToggleControllable(true);
                        actorConversationWithPlayerManager.DetachPlayerFromConversationAsync("CONV_FAMILY", player);
                        player.StopAudioStream();
                        player.SetSpawnInfo(0, selectedCharacter.Skin, new Vector3(2273.5562, 82.3747, 26.4844), 358);
                        player.VirtualWorld = 0;
                        player.Name = String.Format("{0}_{1}", selectedCharacter.FirstName, selectedCharacter.LastName);
                        player.Color = Color.White;
                        player.Spawn();
                        player.Skin = selectedCharacter.Skin;

                        #if (!DEBUG)
                            discordService.SendGeneralChatMessage($"## {player.Name.Replace("_", " ")} is now playing on the server.");
                        #endif

                        var eventArgs = new OnCharacterSelectedEventArgs
                        {
                            Player = player,
                            Account = accountComponent,
                            Character = characterComponent
                        };
                        serverEventAggregator.PublishAsync(eventArgs);
                    }
                }
                else
                {
                    player.Kick();
                }
            }

            if(characterModels != null)
            {
                foreach(CharacterModel character in characterModels)
                {
                    choiceDialog.Add(String.Format("{0}{1} {2}", ChatColor.CornflowerBlue, character.FirstName, character.LastName));
                }
            }

            choiceDialog.Add(ChatColor.White + "Create a new character");

            dialogService.Show(player.Entity, choiceDialog, CharacterSelectionDialogHandler);
        }
    }
}
