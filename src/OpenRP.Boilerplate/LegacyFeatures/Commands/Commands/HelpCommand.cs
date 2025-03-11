using OpenRP.Framework.Shared.Dialogs;
using OpenRP.Framework.Shared.Dialogs.Enums;
using OpenRP.Framework.Features.Commands.Attributes;
using SampSharp.Entities;
using SampSharp.Entities.SAMP;
using System.Text;
using OpenRP.Framework.Features.Commands.Helpers;
using OpenRP.Framework.Features.Commands.Entities;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Shared;
using OpenRP.Framework.Features.Players.Extensions;
using OpenRP.Framework.Features.Permissions.Services;

namespace OpenRP.Boilerplate.LegacyFeatures.Commands.Commands
{
    public class HelpCommand : ISystem
    {
        [ServerCommand(PermissionGroups = new[] { "Default" },
            Description = "Show a list of all commands you have access to.",
            CommandGroups = new[] { "Info" })]
        public void Help(Player player, IPermissionService permissionManager, IDialogService dialogService)
        {
            Character character = player.GetPlayerCurrentlyPlayingAsCharacter();
            if (character == null)
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "Character not found.");
                return;
            }

            var availableCommands = GetAvailableCommands(character, permissionManager);
            if (!availableCommands.Any())
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, "No commands available.");
                return;
            }

            ShowGroupDialog(
                player,
                ServerCommandGroupCache.CachedServerCommandGroups,
                new HashSet<string>(availableCommands.Select(c => c.Name)),
                dialogService,
                permissionManager,
                new Stack<CommandGroupNode>()
            );
        }

        private void ShowGroupDialog(
            Player player,
            CommandGroupNode currentGroup,
            HashSet<string> availableCommandNames,
            IDialogService dialogService,
            IPermissionService permissionManager,
            Stack<CommandGroupNode> navStack)
        {
            var accessibleSubgroups = currentGroup.Subgroups
                .Where(sg => HasAccessibleContent(sg, availableCommandNames))
                .OrderBy(sg => sg.Name)
                .ToList();

            var accessibleCommands = currentGroup.Commands
                .Where(c => availableCommandNames.Contains(c.Name))
                .OrderBy(c => c.Name)
                .DistinctBy(c => c.Name.ToLower())
                .ToList();

            var dialog = new BetterListDialog("Select", "Back");
            dialog.SetTitle(TitleType.Parents, GetGroupTitle(currentGroup, navStack));

            // Add subgroups first
            foreach (var subgroup in accessibleSubgroups)
            {
                dialog.AddRow($"{ChatColor.CornflowerBlue}> {subgroup.Name}");
            }

            // Add commands
            foreach (var cmd in accessibleCommands)
            {
                dialog.AddRow($"{ChatColor.White}/{cmd.Name.ToLower()}");
            }

            dialogService.Show(player, dialog, response =>
                HandleGroupDialogResponse(response, player, currentGroup, accessibleSubgroups,
                    accessibleCommands, availableCommandNames, dialogService, permissionManager, navStack));
        }

        private void HandleGroupDialogResponse(
            ListDialogResponse response,
            Player player,
            CommandGroupNode currentGroup,
            List<CommandGroupNode> accessibleSubgroups,
            List<ServerCommandInfo> accessibleCommands,
            HashSet<string> availableCommandNames,
            IDialogService dialogService,
            IPermissionService permissionManager,
            Stack<CommandGroupNode> navStack)
        {
            if (response.Response == DialogResponse.LeftButton)
            {
                if (response.ItemIndex < accessibleSubgroups.Count)
                {
                    // Subgroup selected
                    var selectedGroup = accessibleSubgroups[response.ItemIndex];
                    navStack.Push(currentGroup);
                    ShowGroupDialog(player, selectedGroup, availableCommandNames, dialogService, permissionManager, navStack);
                }
                else
                {
                    // Command selected
                    var commandIndex = response.ItemIndex - accessibleSubgroups.Count;
                    ShowCommandDetails(player, accessibleCommands[commandIndex], dialogService, permissionManager, navStack);
                }
            }
            else if (response.Response == DialogResponse.RightButtonOrCancel && navStack.Any())
            {
                // Navigate back
                ShowGroupDialog(player, navStack.Pop(), availableCommandNames, dialogService, permissionManager, navStack);
            }
        }

        private void ShowCommandDetails(
            Player player,
            ServerCommandInfo command,
            IDialogService dialogService,
            IPermissionService permissionManager,
            Stack<CommandGroupNode> navStack)
        {
            var dialog = new BetterMessageDialog("Back", "Close");
            dialog.SetTitle(TitleType.Parents, $"Command: {command.Name}");

            var content = new StringBuilder()
                .AppendLine($"{ChatColor.CornflowerBlue}Description:{ChatColor.White}")
                .AppendLine(command.Description)
                .ToString();

            dialog.SetContent(content);

            dialogService.Show(player, dialog, response =>
            {
                if (response.Response == DialogResponse.LeftButton && navStack.Any())
                {
                    Character character = player.GetPlayerCurrentlyPlayingAsCharacter();
                    ShowGroupDialog(player, navStack.Peek(),
                        new HashSet<string>(GetAvailableCommands(character, permissionManager).Select(i => i.Name)),
                        dialogService,
                        permissionManager,
                        navStack);
                }
            });
        }

        #region Helper Methods
        private List<ServerCommandInfo> GetAvailableCommands(Character character, IPermissionService permissionManager)
        {
            var permissions = permissionManager.GetCharacterPermissionsModels(character.GetDatabaseId());
            var permissionNames = permissions.Select(p => p.Name).ToHashSet();

            return ServerCommandCache.CachedServerCommands
                .Where(c => permissionNames.Contains($"cmd.{c.Name.ToLower()}"))
                .DistinctBy(c => c.Name.ToLower())
                .ToList();
        }

        private bool HasAccessibleContent(CommandGroupNode group, HashSet<string> availableCommands)
        {
            if (group.Commands.Any(c => availableCommands.Contains(c.Name))) return true;
            return group.Subgroups.Any(sg => HasAccessibleContent(sg, availableCommands));
        }

        private string GetGroupTitle(CommandGroupNode group, Stack<CommandGroupNode> navStack)
        {
            var breadcrumbs = navStack
                .Reverse()
                .Select(g => g.Name)
                .Concat(new[] { group.Name })
                .Where(n => n != "Root");

            breadcrumbs = breadcrumbs.Prepend("Available Commands");

            return breadcrumbs.Any() ? string.Join(ChatColor.CornflowerBlue + " -> " + ChatColor.White, breadcrumbs) : "Available Commands";
        }
        #endregion
    }
}
