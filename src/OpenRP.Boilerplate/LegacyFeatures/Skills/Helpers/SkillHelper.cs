namespace OpenRP.Boilerplate.LegacyFeatures.Skills.Helpers
{
    public static class SkillHelper
    {
        public static uint CalculateExperienceForLevel(uint level)
        {
            // Example progression formula: XP = 100 * (level ^ 2)
            return 100 * (level * level);
        }
    }
}
