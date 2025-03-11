using OpenRP.Framework.Features.Accounts.Services;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Characters.Services;
using OpenRP.Framework.Shared;
using OpenRP.Framework.Shared.Dialogs.Helpers;
using OpenRP.Framework.Shared.ServerEvents.Services;
using OpenRP.Framework.Features.ActorConversations.Services;
using OpenRP.Framework.Features.Discord.Services;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Features.MainMenu.Services.Dialogs;

namespace OpenRP.Boilerplate.LegacyFeatures.MainMenu.Dialogs
{
    public static class CreateCharacterLastNameDialog
    {
        public static void Open(Player player, IDialogService dialogService, IActorConversationWithPlayerManager actorConversationWithPlayerManager, IMainMenuDialogService mainMenuDialogService, IDiscordService discordService, IServerEventAggregator serverEventAggregator, ITempCharacterService tempCharacterService, IAccountService accountService)
        {
            InputDialog characterDialog = new InputDialog();

            characterDialog.Caption = DialogHelper.GetTitle("Character Creation", "Last name");
            characterDialog.Content = ChatColor.White + "Pick a last name for your character. The first name of your character can be up to 35 characters long.";
            characterDialog.Button1 = DialogHelper.Next;
            characterDialog.Button2 = DialogHelper.Previous;

            void CreateCharacterLastNameDialogHandler(InputDialogResponse r)
            {
                if (r.Response == DialogResponse.LeftButton)
                {
                    CharacterCreation charCreationComponent = player.GetComponent<CharacterCreation>();

                    if (string.IsNullOrEmpty(r.InputText))
                    {
                        MessageDialog firstNameRequired = new MessageDialog(DialogHelper.GetTitle("Character Creation", "Last name"), ChatColor.White + "The last name for your character is required!", DialogHelper.Retry);

                        void LastNameRequiredDialogHandler(MessageDialogResponse r)
                        {
                            Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                        };

                        dialogService.Show(player.Entity, firstNameRequired, LastNameRequiredDialogHandler);
                    }
                    else if (r.InputText.Length > 35)
                    {
                        MessageDialog lastNameTooLongDialog = new MessageDialog(DialogHelper.GetTitle("Character Creation", "Last name"), ChatColor.White + "The last name for your character may not be longer than 35 characters.", DialogHelper.Retry);
                        
                        void LastNameTooLongDialogHandler(MessageDialogResponse r)
                        {
                            Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                        };

                        dialogService.Show(player.Entity, lastNameTooLongDialog, LastNameTooLongDialogHandler);
                    } else
                    {
                        charCreationComponent.CreatingCharacter.LastName = r.InputText.Trim();

                        CreateCharacterDateOfBirthDialog.Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                    }
                }
                else
                {
                    CreateCharacterMiddleNameDialog.Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                }
            }

            dialogService.Show(player.Entity, characterDialog, CreateCharacterLastNameDialogHandler);
        }
    }
}
