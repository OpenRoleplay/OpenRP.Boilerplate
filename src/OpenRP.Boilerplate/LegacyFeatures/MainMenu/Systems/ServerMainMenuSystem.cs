using OpenRP.Framework.Features.Characters.Services;
using OpenRP.Framework.Shared.ServerEvents.Attributes;
using OpenRP.Framework.Shared.ServerEvents.Entities;
using OpenRP.Framework.Shared.ServerEvents.Services;
using OpenRP.Framework.Features.Accounts.Services;
using OpenRP.Framework.Features.ActorConversations.Services;
using OpenRP.Framework.Features.Discord.Services;
using OpenRP.Boilerplate.LegacyFeatures.MainMenu.Dialogs;
using SampSharp.Entities.SAMP;
using OpenRP.Framework.Shared.ServerEvents.Entities.EventArgs;
using OpenRP.Framework.Features.MainMenu.Services.Dialogs;

namespace OpenRP.Boilerplate.LegacyFeatures.MainMenu.Systems
{
    public class ServerMainMenuSystem : IServerSystem
    {
        private IAccountService _accountService;
        public ServerMainMenuSystem(IAccountService accountService) 
        {
            _accountService = accountService;
        }

        [ServerEvent]
        public void OnAccountLoggedIn(OnAccountLoggedInEventArgs args, IDialogService dialogService, IActorConversationWithPlayerManager actorConversationWithPlayerManager, IMainMenuDialogService mainMenuDialogService, IDiscordService discordService, IServerEventAggregator serverEventAggregator, ITempCharacterService tempCharacterService, IAccountService accountService)
        {
            CharacterSelectionDialog.Open(args.Player, dialogService, actorConversationWithPlayerManager, mainMenuDialogService, discordService, serverEventAggregator, tempCharacterService, accountService);
        }
    }
}
