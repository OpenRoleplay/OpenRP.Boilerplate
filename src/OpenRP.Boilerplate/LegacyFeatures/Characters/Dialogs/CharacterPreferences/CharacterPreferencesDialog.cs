using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Players.Extensions;
using OpenRP.Framework.Shared.Dialogs;
using OpenRP.Framework.Shared.Dialogs.Enums;
using OpenRP.Framework.Shared.Dialogs.Helpers;
using OpenRP.Boilerplate.LegacyFeatures.Characters.Services;
using SampSharp.Entities.SAMP;
using OpenRP.Boilerplate.Data;

namespace OpenRP.Boilerplate.LegacyFeatures.Characters.Dialogs.CharacterRoleplayConsent
{
    public class CharacterPreferencesDialog
    {
        public static void Open(Player player, IDialogService dialogService, ICharacterService characterManager)
        {
            using (var context = new DataContext())
            {
                Character characterPlayingAs = player.GetPlayerCurrentlyPlayingAsCharacter();

                if (characterPlayingAs != null) 
                {
                    CharacterPreferencesModel characterPreferences = characterManager.GetCharacterPreferences(characterPlayingAs);

                    BetterTablistDialog tablistDialog = new BetterTablistDialog("Proceed", "Exit", 2);
                    tablistDialog.SetTitle(TitleType.Parents, "CharacterModel Preferences");
                    tablistDialog.AddHeaders("Preference", "Value");

                    tablistDialog.AddHeaders("General");
                    int hardcoreMode = tablistDialog.AddRow("Hardcore Mode", DialogHelper.GetBooleanAsOnOrOff(characterPreferences.HardcoreMode));
                    tablistDialog.AddHeaders("Consent");
                    int characterKill = tablistDialog.AddRow("Allow CharacterModel Kill", DialogHelper.GetBooleanAsOnOrOff(characterPreferences.AllowCharacterKill));
                    int rape = tablistDialog.AddRow("Allow Rape", DialogHelper.GetBooleanAsOnOrOff(characterPreferences.AllowRape));
                    int slavery = tablistDialog.AddRow("Allow Slavery", DialogHelper.GetBooleanAsOnOrOff(characterPreferences.AllowSlavery));
                    int worldEvents = tablistDialog.AddRow("Allow World Events", DialogHelper.GetBooleanAsOnOrOff(characterPreferences.AllowWorldEvents));
                    tablistDialog.AddHeaders("CurrencyModel");
                    int defaultCurrency = tablistDialog.AddRow("Default CurrencyModel", DialogHelper.GetBooleanAsOnOrOff(characterPreferences.AllowWorldEvents));

                    // Dialog Response
                    void DialogHandler(TablistDialogResponse r)
                    {
                        if (r.Response != DialogResponse.LeftButton)
                        {
                            return;
                        }

                        int index = r.ItemIndex;

                        // Hardcore Mode
                        if (index == hardcoreMode)
                        {
                            BetterMessageDialog messageDialog = new BetterMessageDialog("Turn On", "Go Back");
                            messageDialog.SetTitle(TitleType.Children, "CharacterModel Preferences", "Hardcore Mode", "Information");
                            messageDialog.SetContent("{6495ED}Hardcore Mode{FFFFFF}\r\nHardcore Mode means you agree to all in-game interactions, such as character kills, slavery, and rape. By playing in Hardcore Mode, you help create a more dynamic roleplay environment and allow important story events that deepen your character's development.\r\n\r\nAs a reward, you gain the \"Blessed by Misfortune\" trait.\r\n\r\n{6495ED}Blessed by Misfortune{FFFFFF}\r\nLuck shines on those who walk the path of danger. This trait gives you extra luck in many luck-based activities on Open Roleplay.\r\n\r\n{FF0000}Warning: Once you enable Hardcore Mode, it remains active until your character is character killed. It will automatically disable after that. So ask yourself this: are you fine with your character inevitably being killed in the future?{FFFFFF}");

                            void HardcoreModeTurnOn(MessageDialogResponse r)
                            {
                                if (r.Response != DialogResponse.LeftButton)
                                {
                                    dialogService.Show(player, tablistDialog, DialogHandler);
                                }

                                BetterMessageDialog confirmationDialog = new BetterMessageDialog("Turn On", "Go Back");
                                confirmationDialog.SetTitle(TitleType.Children, "CharacterModel Preferences", "Hardcore Mode", "Confirmation");
                                confirmationDialog.SetContent("{6495ED}Confirm Hardcore Mode Activation{FFFFFF}\r\nAre you certain you want to enable Hardcore Mode? This means you consent to all in-game interactions, including character kills, slavery, and rape. By choosing Hardcore Mode, you contribute to a more dynamic roleplay environment and allow impactful story events that deeply affect your character's development.\r\n\r\n{6495ED}Important:{FFFFFF} Once enabled, Hardcore Mode cannot be turned off manually. It will automatically disable only if your character is character killed.\r\n\r\n{FF0000}Warning: Activating Hardcore Mode commits your character to a path where their fate is sealed. Your character may face inevitable death in the future.{FFFFFF}\r\n\r\nAre you sure that you wish to proceed?");

                                void HardcoreModeConfirmation(MessageDialogResponse r)
                                {
                                    if (r.Response != DialogResponse.LeftButton)
                                    {
                                        dialogService.Show(player, tablistDialog, DialogHandler);
                                    }

                                    ActivateHardcoreMode(player, dialogService, characterManager, characterPreferences);
                                }

                                dialogService.Show(player, confirmationDialog, HardcoreModeConfirmation);
                            }
                            
                            dialogService.Show(player, messageDialog, HardcoreModeTurnOn);
                        }
                        // CharacterModel Kill, Rape, Slavery, World Events
                        else if (index == characterKill || index == rape || index == slavery || index == worldEvents)
                        {
                            if(!characterPreferences.HardcoreMode)
                            {
                                if(index == characterKill)
                                {
                                    ToggleCharacterKill(player, dialogService, characterManager, characterPreferences);
                                }
                                else if(index == rape)
                                {
                                    ToggleRape(player, dialogService, characterManager, characterPreferences);
                                }
                                else if (index == slavery)
                                {
                                    ToggleSlavery(player, dialogService, characterManager, characterPreferences);
                                }
                                else if (index == worldEvents)
                                {
                                    ToggleWorldEvents(player, dialogService, characterManager, characterPreferences);
                                }
                            } else
                            {
                                BetterMessageDialog messageDialog = new BetterMessageDialog("Go Back");
                                messageDialog.SetTitle(TitleType.Children, "CharacterModel Preferences", "Consent");
                                messageDialog.SetContent("{6495ED}Hardcore Mode Active{FFFFFF}\r\nHardcore Mode is currently enabled. As a result, you cannot change consent settings for CharacterModel Kills, Rape, Slavery, and World Events. Your preferences are locked to maintain a consistent and immersive roleplay experience.\r\n\r\n{FF0000}Warning: Changes to consent settings are disabled while Hardcore Mode is active. Hardcore Mode will remain active until your character is character killed. You cannot disable Hardcore Mode manually.{FFFFFF}");

                                void HardcoreModeCantChangeConsent(MessageDialogResponse r)
                                {
                                    dialogService.Show(player, tablistDialog, DialogHandler);
                                }

                                dialogService.Show(player, messageDialog, HardcoreModeCantChangeConsent);
                            }
                        }
                        else
                        {
                            dialogService.Show(player, tablistDialog, DialogHandler);
                        }
                    }

                    dialogService.Show(player, tablistDialog, DialogHandler);
                }
            }
        }

        private static void ActivateHardcoreMode(Player player, IDialogService dialogService, ICharacterService characterManager, CharacterPreferencesModel characterPreferences)
        {
            characterPreferences.HardcoreMode = true;
            characterPreferences.AllowCharacterKill = true;
            characterPreferences.AllowRape = true;
            characterPreferences.AllowSlavery = true;
            characterPreferences.AllowWorldEvents = true;

            characterManager.UpdateCharacterPreferences(characterPreferences);

            BetterMessageDialog turnedOnDialog = new BetterMessageDialog("Go Back");
            turnedOnDialog.SetTitle(TitleType.Children, "CharacterModel Preferences", "Hardcore Mode", "Hardcore Mode Activated");
            turnedOnDialog.SetContent("{6495ED}Hardcore Mode Activated{FFFFFF}\r\nYou have successfully activated Hardcore Mode. This means you consent to all in-game interactions, including character kills, slavery, and rape. By choosing Hardcore Mode, you contribute to a more dynamic roleplay environment and allow impactful story events that will deeply affect your character's development.\r\n\r\nAs a reward, you have gained the Blessed by Misfortune trait.\r\n\r\n{6495ED}Blessed by Misfortune{FFFFFF}\r\nLuck shines on those who walk the path of danger. This trait gives you extra luck in many luck-based activities on Open Roleplay.\r\n\r\n{FF0000}Warning: Hardcore Mode cannot be turned off manually. It will automatically disable only if your character is character killed. Your character may face inevitable death in the future.{FFFFFF}\r\n\r\nProceed with caution and embrace the challenges that lie ahead.");

            void HardcoreModeActivated(MessageDialogResponse r)
            {
                Open(player, dialogService, characterManager);
            }

            dialogService.Show(player, turnedOnDialog, HardcoreModeActivated);
        }

        private static void ToggleCharacterKill(Player player, IDialogService dialogService, ICharacterService characterManager, CharacterPreferencesModel characterPreferences)
        {
            characterPreferences.AllowCharacterKill = !characterPreferences.AllowCharacterKill;

            characterManager.UpdateCharacterPreferences(characterPreferences);

            Open(player, dialogService, characterManager);
        }

        private static void ToggleRape(Player player, IDialogService dialogService, ICharacterService characterManager, CharacterPreferencesModel characterPreferences)
        {
            characterPreferences.AllowRape = !characterPreferences.AllowRape;

            characterManager.UpdateCharacterPreferences(characterPreferences);

            Open(player, dialogService, characterManager);
        }

        private static void ToggleSlavery(Player player, IDialogService dialogService, ICharacterService characterManager, CharacterPreferencesModel characterPreferences)
        {
            characterPreferences.AllowSlavery = !characterPreferences.AllowSlavery;

            characterManager.UpdateCharacterPreferences(characterPreferences);

            Open(player, dialogService, characterManager);
        }

        private static void ToggleWorldEvents(Player player, IDialogService dialogService, ICharacterService characterManager, CharacterPreferencesModel characterPreferences)
        {
            characterPreferences.AllowWorldEvents = !characterPreferences.AllowWorldEvents;

            characterManager.UpdateCharacterPreferences(characterPreferences);

            Open(player, dialogService, characterManager);
        }
    }
}
