using OpenRP.Framework.Features.Characters.Services.Dialogs;
using OpenRP.Framework.Features.MainMenu.Services.Dialogs;
using OpenRP.Framework.Shared.ServerEvents.Attributes;
using OpenRP.Framework.Shared.ServerEvents.Entities;
using OpenRP.Framework.Shared.ServerEvents.Entities.EventArgs;

namespace OpenRP.Boilerplate.LegacyFeatures.MainMenu.Systems
{
    public class ServerMainMenuSystem : IServerSystem
    {
        [ServerEvent]
        public void OnAccountLoggedIn(OnAccountLoggedInEventArgs args, ICharacterDialogService characterDialogService, IMainMenuDialogService mainMenuDialogService)
        {
            characterDialogService.OpenCharacterSelection(args.Player, () => mainMenuDialogService.OpenMainMenuChoiceMenu(args.Player));
        }
    }
}
