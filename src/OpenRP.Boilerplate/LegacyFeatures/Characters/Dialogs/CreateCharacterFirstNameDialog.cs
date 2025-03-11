using OpenRP.Framework.Features.Accounts.Services;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Characters.Services;
using OpenRP.Framework.Features.Discord.Services;
using OpenRP.Framework.Shared;
using OpenRP.Framework.Shared.Dialogs.Helpers;
using OpenRP.Framework.Shared.ServerEvents.Services;
using OpenRP.Framework.Features.ActorConversations.Services;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Features.MainMenu.Services.Dialogs;

namespace OpenRP.Boilerplate.LegacyFeatures.MainMenu.Dialogs
{
    public static class CreateCharacterFirstNameDialog
    {
        public static void Open(Player player, IDialogService dialogService, IActorConversationWithPlayerManager actorConversationWithPlayerManager, IMainMenuDialogService mainMenuDialogService, IDiscordService discordService, IServerEventAggregator serverEventAggregator, ITempCharacterService tempCharacterService, IAccountService accountService)
        {
            InputDialog characterDialog = new InputDialog();

            characterDialog.Caption = DialogHelper.GetTitle("Character Creation", "First name");
            characterDialog.Content = ChatColor.White + "Pick a first name for your character. The first name of your character can be up to 35 characters long.";
            characterDialog.Button1 = DialogHelper.Next;
            characterDialog.Button2 = DialogHelper.Previous;

            void CreateCharacterFirstNameDialogHandler(InputDialogResponse r)
            {
                if (r.Response == DialogResponse.LeftButton)
                {
                    player.DestroyComponents<CharacterCreation>();
                    CharacterCreation charCreationComponent = player.AddComponent<CharacterCreation>();

                    if (string.IsNullOrEmpty(r.InputText))
                    {
                        MessageDialog firstNameRequired = new MessageDialog(DialogHelper.GetTitle("Character Creation", "First name"), ChatColor.White + "The first name for your character is required!", DialogHelper.Retry);

                        void FirstNameRequiredDialogHandler(MessageDialogResponse r)
                        {
                            Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                        };

                        dialogService.Show(player.Entity, firstNameRequired, FirstNameRequiredDialogHandler);
                    }
                    else if (r.InputText.Length > 35)
                    {
                        MessageDialog firstNameTooLongDialog = new MessageDialog(DialogHelper.GetTitle("Character Creation", "First name"), ChatColor.White + "The first name for your character may not be longer than 35 characters.", DialogHelper.Retry);
                        
                        void FirstNameTooLongDialogHandler(MessageDialogResponse r)
                        {
                            Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                        };

                        dialogService.Show(player.Entity, firstNameTooLongDialog, FirstNameTooLongDialogHandler);
                    } else
                    {
                        charCreationComponent.CreatingCharacter.FirstName = r.InputText.Trim();

                        CreateCharacterMiddleNameDialog.Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                    }
                }
                else
                {
                    mainMenuDialogService.OpenMainMenuChoiceMenu(player);
                }
            }

            dialogService.Show(player.Entity, characterDialog, CreateCharacterFirstNameDialogHandler);
        }
    }
}
