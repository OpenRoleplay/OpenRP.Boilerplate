using OpenRP.Framework.Features.Accounts.Services;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Characters.Services;
using OpenRP.Framework.Shared;
using OpenRP.Framework.Shared.Dialogs.Helpers;
using OpenRP.Framework.Shared.ServerEvents.Services;
using OpenRP.Boilerplate.LegacyFeatures.Characters.Helpers;
using OpenRP.Framework.Features.Discord.Services;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Features.ActorConversations.Services;
using OpenRP.Framework.Features.MainMenu.Services.Dialogs;

namespace OpenRP.Boilerplate.LegacyFeatures.MainMenu.Dialogs
{
    public static class CreateCharacterDateOfBirthDialog
    {
        public static void Open(Player player, IDialogService dialogService, IActorConversationWithPlayerManager actorConversationWithPlayerManager, IMainMenuDialogService mainMenuDialogService, IDiscordService discordService, IServerEventAggregator serverEventAggregator, ITempCharacterService tempCharacterService, IAccountService accountService)
        {
            InputDialog characterDialog = new InputDialog();

            characterDialog.Caption = DialogHelper.GetTitle("Character Creation", "Date of Birth");
            characterDialog.Content = ChatColor.White + "Pick a date of birth for your character. The appropriate format to use is DD/MM/YYYY.";
            characterDialog.Button1 = DialogHelper.Next;
            characterDialog.Button2 = DialogHelper.Previous;

            void CreateCharacterDateOfBirthDialogHandler(InputDialogResponse r)
            {
                if (r.Response == DialogResponse.LeftButton)
                {
                    CharacterCreation charCreationComponent = player.GetComponent<CharacterCreation>();

                    DateTime characterDoB;
                    if (DateTime.TryParse(r.InputText, out characterDoB))
                    {
                        MessageDialog confirmDateOfBirth = new MessageDialog(DialogHelper.GetTitle("Character Creation", "Date of Birth"), ChatColor.White + String.Format("Your chosen Date of Birth is {0}, meaning that your character would be {1} years old. Is that correct?", characterDoB.ToString("dd/MM/yyyy"), (DateTime.Today.Year - characterDoB.Year)), DialogHelper.Yes, DialogHelper.No);

                        void ConfirmDialogHandler(MessageDialogResponse confirmResponse)
                        {
                            if(confirmResponse.Response == DialogResponse.LeftButton)
                            {
                                charCreationComponent.CreatingCharacter.DateOfBirth = characterDoB;

                                // Next Step
                                CharacterHelper.CreateCharacter(accountService, player);
                                CharacterSelectionDialog.Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                            } else
                            {
                                Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                            }
                        };

                        dialogService.Show(player.Entity, confirmDateOfBirth, ConfirmDialogHandler);
                    }
                    else
                    {
                        MessageDialog incorrectFormatDialog = new MessageDialog(DialogHelper.GetTitle("Character Creation", "Date of Birth"), ChatColor.White + "Your chosen format for the Date of Birth is incorrect, please try again.", DialogHelper.Retry);
                        
                        void IncorrectFormatDialogHandler(MessageDialogResponse r)
                        {
                            Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                        };

                        dialogService.Show(player.Entity, incorrectFormatDialog, IncorrectFormatDialogHandler);
                    }
                }
                else
                {
                    CreateCharacterLastNameDialog.Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                }
            }

            dialogService.Show(player.Entity, characterDialog, CreateCharacterDateOfBirthDialogHandler);
        }
    }
}
