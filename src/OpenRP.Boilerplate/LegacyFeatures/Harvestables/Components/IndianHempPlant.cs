using OpenRP.Framework.Features.BiomeGenerator.Entities;
using OpenRP.Framework.Shared;
using OpenRP.Boilerplate.LegacyFeatures.Harvestables.Entities;
using SampSharp.ColAndreas.Entities.Services;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using SampSharp.Streamer.Entities;

namespace OpenRP.Boilerplate.LegacyFeatures.Harvestables.Components
{
    public class IndianHempPlant : Component, IHarvestablePlant
    {
        private BiomeObject _generatedObject;
        private IColAndreasService _colAndreasService;
        private IStreamerService _streamerService;
        private int _remainingLeaves;

        public IndianHempPlant(BiomeObject generatedObject, IColAndreasService colAndreasService, IStreamerService streamerService)
        {
            _generatedObject = generatedObject;
            _colAndreasService = colAndreasService;
            _streamerService = streamerService;
            _remainingLeaves = 10;

            CreateDynamicObject();
            //CreateDynamicTextLabel();
        }

        public string GetTextLabelString()
        {
            return $"{ChatColor.Highlight}Indian Hemp (Apocynum Cannabinum)\n{ChatColor.White}{_remainingLeaves} leaves remaining\n\nUse {ChatColor.Highlight}/harvest hemp";
        }

        public Vector3 GetTextLabelPosition()
        {
            Vector3 textLabelPosition = new Vector3(_generatedObject.GamePosition.XY, _generatedObject.GamePosition.Z + 0.25f);
            return textLabelPosition;
        }

        public void CreateDynamicTextLabel()
        {
            Vector3 textLabelPosition = GetTextLabelPosition();
        }

        public DynamicTextLabel GetDynamicTextLabel()
        {
            return GetComponentInChildren<DynamicTextLabel>();
        }

        public void UpdateDynamicTextLabel()
        {
            DynamicTextLabel dynamicTextLabel = GetDynamicTextLabel();
            dynamicTextLabel.Text = GetTextLabelString();
        }

        public void CreateDynamicObject()
        {
            //BiomeManager.CreateGeneratorObject(_generatedObject, Entity, _colAndreasService, _streamerService);
        }

        public DynamicObject GetDynamicObject()
        {
            return GetComponentInChildren<DynamicObject>();
        }

        public bool IsPlayerNearby(Player player)
        {
            Vector3 textLabelPosition = GetTextLabelPosition();
            if (player.IsInRangeOfPoint(3.0f, textLabelPosition))
            {
                return true;
            }
            return false;
        }

        public void Harvest()
        {
            _remainingLeaves -= 1;
            UpdateDynamicTextLabel();
        }
    }
}
