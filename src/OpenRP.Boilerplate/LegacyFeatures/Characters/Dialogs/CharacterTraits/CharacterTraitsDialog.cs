using OpenRP.Boilerplate.Data;
using OpenRP.Boilerplate.LegacyFeatures.Characters.Services;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Shared.Dialogs;
using OpenRP.Framework.Shared.Dialogs.Enums;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Shared;
using OpenRP.Framework.Features.Players.Extensions;

namespace OpenRP.Boilerplate.LegacyFeatures.Characters.Dialogs.CharacterTraits
{
    public class CharacterTraitsDialog
    {
        public static void Open(Player player, IDialogService dialogService, ICharacterService characterManager)
        {
            using (var context = new DataContext())
            {
                Character character = player.GetPlayerCurrentlyPlayingAsCharacter();

                if (character != null)
                {
                    CharacterPreferencesModel characterPreferences = characterManager.GetCharacterPreferences(character);

                    if (characterPreferences != null && characterPreferences.HardcoreMode)
                    {
                        BetterTablistDialog tablistDialog = new BetterTablistDialog("Details", "Exit", 2);
                        tablistDialog.SetTitle(TitleType.Parents, "CharacterModel Traits");
                        tablistDialog.AddHeaders("Trait", "Description");
                        tablistDialog.AddRow($"Blessed by Misfortune", $"Luck shines on those who walk the path of danger.");

                        // Dialog Response
                        void DialogHandler(TablistDialogResponse r)
                        {
                            if (r.Response != DialogResponse.LeftButton)
                            {
                                return;
                            }

                            BetterMessageDialog betterMessageDialog = new BetterMessageDialog("Go Back");
                            betterMessageDialog.SetTitle(TitleType.Children, "CharacterModel Traits", "Blessed by Misfortune");
                            betterMessageDialog.SetContent($"{ChatColor.CornflowerBlue}Name:\n{ChatColor.White}Blessed by Misfortune\n\n{ChatColor.CornflowerBlue}Description:\n{ChatColor.White}Luck shines on those who walk the path of danger.\n\n{ChatColor.CornflowerBlue}Effects:\n{ChatColor.White}This trait gives you extra luck in many luck-based activities on Open Roleplay.");

                            void TraitDialogHandler(MessageDialogResponse r)
                            {
                                dialogService.Show(player, tablistDialog, DialogHandler);
                            }

                            dialogService.Show(player, betterMessageDialog, TraitDialogHandler);
                        }

                        dialogService.Show(player, tablistDialog, DialogHandler);
                    } else
                    {
                        BetterMessageDialog betterMessageDialog = new BetterMessageDialog("Go Back");
                        betterMessageDialog.SetTitle(TitleType.Parents, "CharacterModel Traits");
                        betterMessageDialog.SetContent($"Your character has no traits at this moment!");
                        dialogService.Show(player, betterMessageDialog);
                    }
                }
            }
        }
    }
}
