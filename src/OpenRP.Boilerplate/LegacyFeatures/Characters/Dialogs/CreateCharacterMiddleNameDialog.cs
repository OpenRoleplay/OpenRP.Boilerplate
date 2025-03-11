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
    public static class CreateCharacterMiddleNameDialog
    {
        public static void Open(Player player, IDialogService dialogService, IActorConversationWithPlayerManager actorConversationWithPlayerManager, IMainMenuDialogService mainMenuDialogService, IDiscordService discordService, IServerEventAggregator serverEventAggregator, ITempCharacterService tempCharacterService, IAccountService accountService)
        {
            MessageDialog middleNameYesOrNoDialog = new MessageDialog(DialogHelper.GetTitle("Character Creation", "Middle name"), ChatColor.White + "Would you like to set a middle name for your character? This is not required.", DialogHelper.Yes, DialogHelper.No);

            void MiddleNameYesOrNoDialogHandler(MessageDialogResponse r)
            {
                if(r.Response == DialogResponse.LeftButton)
                {
                    InputDialog characterDialog = new InputDialog();

                    characterDialog.Caption = DialogHelper.GetTitle("Character Creation", "Middle name");
                    characterDialog.Content = ChatColor.White + "Pick a middle name for your character. The middle name of your character can be up to 30 characters long. You can also skip this step by not filling a middle name in.";
                    characterDialog.Button1 = DialogHelper.Next;
                    characterDialog.Button2 = DialogHelper.Previous;

                    void CreateCharacterMiddleNameDialogHandler(InputDialogResponse r)
                    {
                        if (r.Response == DialogResponse.LeftButton)
                        {
                            CharacterCreation charCreationComponent = player.GetComponent<CharacterCreation>();

                            if (String.IsNullOrEmpty(r.InputText))
                            {
                                charCreationComponent.CreatingCharacter.MiddleName = null;

                                CreateCharacterLastNameDialog.Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                            }
                            else if (r.InputText.Length > 30)
                            {
                                MessageDialog middleNameTooLongDialog = new MessageDialog(DialogHelper.GetTitle("Character Creation", "Middle name"), ChatColor.White + "The middle name for your character may not be longer than 30 characters.", DialogHelper.Retry);

                                void MiddleNameTooLongDialogHandler(MessageDialogResponse r)
                                {
                                    Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                                };

                                dialogService.Show(player.Entity, middleNameTooLongDialog, MiddleNameTooLongDialogHandler);
                            }
                            else
                            {
                                charCreationComponent.CreatingCharacter.MiddleName = r.InputText.Trim();

                                CreateCharacterLastNameDialog.Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                            }
                        }
                        else
                        {
                            CreateCharacterFirstNameDialog.Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                        }
                    }

                    dialogService.Show(player.Entity, characterDialog, CreateCharacterMiddleNameDialogHandler);
                } else
                {
                    CreateCharacterLastNameDialog.Open(player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
                }
            };

            dialogService.Show(player.Entity, middleNameYesOrNoDialog, MiddleNameYesOrNoDialogHandler);
        }
    }
}
