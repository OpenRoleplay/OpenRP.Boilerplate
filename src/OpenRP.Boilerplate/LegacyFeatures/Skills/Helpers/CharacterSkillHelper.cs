using OpenRP.Framework.Database.Models;
using OpenRP.Boilerplate.Data;

namespace OpenRP.Boilerplate.LegacyFeatures.Skills.Helpers
{
    public static class CharacterSkillHelper
    {
        public static uint GetTotalExperienceForNextLevel(this CharacterModel character, DataContext context, ulong skillId)
        {
            // Retrieve the CharacterSkillModel entry for this character and skill
            var characterSkill = context.CharacterSkills
                .SingleOrDefault(cs => cs.CharacterId == character.Id && cs.SkillId == skillId);

            // If the character does not have this skill, return -1
            if (characterSkill == null)
                return 0;

            // Retrieve the SkillModel entry to get any skill-specific data (e.g., base XP requirement)
            var skill = context.Skills.SingleOrDefault(s => s.Id == skillId);

            // If the skill doesn't exist, return -1
            if (skill == null)
                return 0;

            // Calculate the experience needed based on the current level
            uint currentLevel = characterSkill.Level;

            // Example formula for experience needed to level up (can be adjusted as needed)
            uint experienceNeeded = SkillHelper.CalculateExperienceForLevel(currentLevel + 1);

            return experienceNeeded;
        }

        public static uint GetTotalExperienceForNextLevel(this CharacterSkillModel characterSkill)
        {
            // Calculate the experience needed based on the current level
            uint currentLevel = characterSkill.Level;

            // Example formula for experience needed to level up (can be adjusted as needed)
            uint experienceNeeded = SkillHelper.CalculateExperienceForLevel(currentLevel + 1);

            return experienceNeeded;
        }
    }
}
