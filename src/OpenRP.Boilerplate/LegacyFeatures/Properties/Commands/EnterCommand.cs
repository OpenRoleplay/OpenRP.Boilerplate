using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using OpenRP.Boilerplate.LegacyFeatures.Properties.Services;
using OpenRP.Boilerplate.LegacyFeatures.Properties.Components;
using OpenRP.Framework.Features.Commands.Attributes;

namespace OpenRP.Boilerplate.LegacyFeatures.Properties.Commands
{
    public class EnterCommand : ISystem
    {
        [ServerCommand(PermissionGroups = new string[] { "Default" },
            Description = "Enter or exit a property or building. Use this command when you're near a property door to go through it.")]
        public void Enter(Player player, IPropertyManager propertyManager)
        {
            List<PropertyDoor> propertyDoors = propertyManager.GetAllPropertyDoors().ToList();

            foreach(PropertyDoor propertyDoor in propertyDoors)
            {
                if (propertyDoor.IsPlayerNearby(player))
                {
                    PropertyDoor propertyDoorLinkedTo = propertyDoor.GetPropertyDoorLinkedTo();
                    Property propertyLinkedTo = propertyDoor.GetProperty();
                    int propertyDoorLinkedToInterior = propertyDoorLinkedTo.GetPropertyDoorInterior();

                    player.Position = propertyDoorLinkedTo.GetPropertyDoorPos();
                    player.Interior = propertyDoorLinkedToInterior;
                    if (propertyDoorLinkedToInterior == 0)
                    {
                        player.VirtualWorld = 0;
                    } else
                    {
                        player.VirtualWorld = propertyLinkedTo.GetPropertyVirtualWorld();
                    }
                    break;
                }
            }
        }

        [ServerCommand(PermissionGroups = new string[] { "Default" },
            Description = "Enter or exit a property or building. Use this command when you're near a property door to go through it.")]
        public void Exit(Player player, IPropertyManager propertyManager)
        {
            Enter(player, propertyManager);
        }
    }
}
