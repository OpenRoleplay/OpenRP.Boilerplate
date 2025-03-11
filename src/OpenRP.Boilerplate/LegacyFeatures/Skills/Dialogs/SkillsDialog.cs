using Microsoft.EntityFrameworkCore;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Shared;
using OpenRP.Framework.Shared.Dialogs.Helpers;
using OpenRP.Boilerplate.Data;
using OpenRP.Boilerplate.LegacyFeatures.Skills.Helpers;
using SampSharp.Entities.SAMP;
using System.Text;

namespace OpenRP.Boilerplate.LegacyFeatures.Skills.Dialogs
{
    public class SkillsDialog
    {
        public static void OpenSkillsDialog(Player player, IDialogService dialogService, CharacterModel character)
        {
            using (var context = new DataContext())
            {
                string dialogTitle = DialogHelper.GetTitle("Skills");

                // Column Headers
                List<string> dialogColumnHeaders = new List<string>
                {
                    ChatColor.CornflowerBlue + "SkillModel",
                    ChatColor.CornflowerBlue + "Level",
                    ChatColor.CornflowerBlue + "XP",
                    ChatColor.CornflowerBlue + "Last Used"
                };

                // Create TablistDialog
                TablistDialog dialog = new TablistDialog(dialogTitle, "Details", "Close", dialogColumnHeaders.ToArray());

                // Add each skill to the dialog
                List<CharacterSkillModel> characterSkills = context.CharacterSkills.Where(i => i.CharacterId == character.Id).Include(i => i.Skill).ToList();
                foreach (var characterSkill in characterSkills)
                {
                    if (characterSkill.Skill != null)
                    {
                        string skillName = ChatColor.White + characterSkill.Skill.Name;
                        string level = ChatColor.White + characterSkill.Level.ToString();
                        string experience = ChatColor.White + characterSkill.Experience.ToString() + ChatColor.CornflowerBlue + "/" + ChatColor.White + characterSkill.GetTotalExperienceForNextLevel();
                        string lastUsed = ChatColor.White + (characterSkill.LastUsedDate != null ? characterSkill.LastUsedDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "N/A");

                        dialog.Add(skillName, level, experience, lastUsed);
                    }
                }

                // Handle dialog response
                void SkillsDialogHandler(TablistDialogResponse response)
                {
                    if (response.Response == DialogResponse.LeftButton)
                    {
                        int selectedIndex = response.ItemIndex;
                        CharacterSkillModel selectedCharacterSkill = characterSkills.ElementAtOrDefault(selectedIndex);
                        SkillModel selectedSkill = selectedCharacterSkill.Skill;

                        if (selectedSkill != null)
                        {
                            OpenSkillDetailsDialog(player, dialogService, selectedSkill, selectedCharacterSkill);
                        }
                    }
                }

                // Show the dialog
                dialogService.Show(player, dialog, SkillsDialogHandler);
            }
        }

        private static void OpenSkillDetailsDialog(Player player, IDialogService dialogService, SkillModel skill, CharacterSkillModel characterSkill)
        {
            string title = DialogHelper.GetTitle("SkillModel Details", skill.Name);

            StringBuilder contentBuilder = new StringBuilder();

            contentBuilder.AppendLine(ChatColor.CornflowerBlue + "Name:");
            contentBuilder.AppendLine(ChatColor.White + skill.Name);
            contentBuilder.AppendLine();
            if(!String.IsNullOrEmpty(skill.Description))
            {
                contentBuilder.AppendLine(ChatColor.CornflowerBlue + "Description:");
                contentBuilder.AppendLine(ChatColor.White + skill.Description);
                contentBuilder.AppendLine();
            }
            contentBuilder.AppendLine(ChatColor.CornflowerBlue + "Level:");
            contentBuilder.AppendLine(ChatColor.White + characterSkill.Level);
            contentBuilder.AppendLine();
            contentBuilder.AppendLine(ChatColor.CornflowerBlue + "Experience:");
            contentBuilder.AppendLine(ChatColor.White + characterSkill.Experience.ToString() + ChatColor.CornflowerBlue + "/" + ChatColor.White + characterSkill.GetTotalExperienceForNextLevel());
            contentBuilder.AppendLine();
            contentBuilder.AppendLine(ChatColor.CornflowerBlue + "Last Used:");
            if (characterSkill.LastUsedDate != null)
            {
                contentBuilder.AppendLine(ChatColor.White + characterSkill.LastUsedDate);
            } else
            {
                contentBuilder.AppendLine(ChatColor.White + "N/A");
            }
            contentBuilder.AppendLine();

            MessageDialog detailsDialog = new MessageDialog(title, contentBuilder.ToString(), "Close");

            dialogService.Show(player, detailsDialog, r => { });
        }
    }
}
