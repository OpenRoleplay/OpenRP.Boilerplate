using SampSharp.Entities.SAMP;
using SampSharp.Entities;
using SampSharp.Streamer.Entities;
using OpenRP.Boilerplate.LegacyFeatures.Players.Helpers;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Services;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers;
using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Characters.Services;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Features.Players.Extensions;
using OpenRP.Framework.Shared.Chat.Services;
using OpenRP.Framework.Database.Services;
using OpenRP.Framework.Features.CDN.Services;
using OpenRP.Framework.Features.Items.Components;

namespace OpenRP.Boilerplate.LegacyFeatures.ChickenCoop.Components
{
    public class ChickenCoop : Component
    {
        IStreamerService _streamerService;
        DynamicTextLabel _textLabel;
        DynamicObject _objectLinkedTo;
        private int _chickens;
        private DateTime _nextChickenBred;
        private int _eggs;
        private DateTime _nextEggLaid;

        public ChickenCoop(IStreamerService streamerService, DynamicObject objectLinkedTo)
        {
            _chickens = 1;
            _eggs = 1;
            _nextChickenBred = DateTime.Now.AddHours(1);
            _nextEggLaid = DateTime.Now.AddMinutes(15);
            _objectLinkedTo = objectLinkedTo;

            // Text Label
            _textLabel = streamerService.CreateDynamicTextLabel(String.Empty, new Color(246, 165, 250), _objectLinkedTo.Position, 3.0f);
            UpdateTextLabel();
        }

        private void UpdateTextLabel()
        {
            // Determine correct singular/plural forms and verb
            string chickenWord = (_chickens == 1) ? "chicken" : "chickens";
            string eggWord = (_eggs == 1) ? "egg" : "eggs";
            string verbToBe = (_chickens == 1) ? "is" : "are";

            _textLabel.Text = $"** There {verbToBe} {_chickens} {chickenWord} with {_eggs} {eggWord} in the chicken coop. **\n\n" +
                "{D3D3D3}(( Use /collectegg to collect an egg. ))";
        }

        public void UpdateChickenCoop(IEntityManager entityManager, IOpenCdnService openCdnService)
        {
            if (DateTime.Now > _nextChickenBred)
            {
                _chickens++;
                _nextChickenBred = DateTime.Now.AddHours(1);
                foreach (Player player in entityManager.GetComponents<Player>())
                {
                    player.PlayOpenCdnStream(openCdnService, "sfx", "chickenActivity.mp3", _objectLinkedTo.Position, 3.0f);
                }
            }
            if (DateTime.Now > _nextEggLaid)
            {
                if (_chickens > 0)
                {
                    Random randomEggCount = new Random();
                    int eggsLaid = randomEggCount.Next(_chickens) + 1;
                    _eggs += eggsLaid;
                    _nextEggLaid = DateTime.Now.AddMinutes(15);
                    if (eggsLaid > 0)
                    {
                        foreach (Player player in entityManager.GetComponents<Player>())
                        {
                            player.PlayOpenCdnStream(openCdnService, "sfx", "chickenEggPop.mp3", _objectLinkedTo.Position, 3.0f);
                        }
                    }
                }
            }
            UpdateTextLabel();
        }

        public bool IsPlayerNearby(Player player)
        {
            if(player.IsInRangeOfPoint(3.0f, _objectLinkedTo.Position))
            {
                return true;
            }
            return false;
        }

        public void CollectEgg(Player player, IEntityManager entityManager, ITempCharacterService characterService, IInventoryService inventoryService, IChatService chatService, IDataMemoryService dataMemoryService, IOpenCdnService openCdnService)
        {
            if(player.IsPlayerPlayingAsCharacter())
            {
                Character character = player.GetComponent<Character>();
                InventoryModel characterInventory = characterService.GetCharacterInventory(character);
                ItemModel eggItem = entityManager.GetComponents<Item>().FirstOrDefault(i => i.GetId() == 36).GetItemModel();
                InventoryItemModel eggItemToAdd = inventoryService.PrepareItem(characterInventory.Id, eggItem, 1);

                if(eggItemToAdd.DoesInventoryItemFitInInventory(characterInventory, 1, dataMemoryService))
                {
                    if (player.SpecialAction == SpecialAction.Duck)
                    {
                        if (_eggs > 0)
                        {
                            int step = 0;
                            System.Timers.Timer collectEggTimer = new System.Timers.Timer(2500);
                            collectEggTimer.Enabled = true;
                            collectEggTimer.Elapsed += (entity, e) =>
                            {
                                if (IsPlayerNearby(player))
                                {
                                    if (player.SpecialAction == SpecialAction.Duck)
                                    {
                                        switch (step)
                                        {
                                            case 0:
                                                chatService.SendPlayerChatMessage(player, PlayerChatMessageType.ME, "carefully reaches into the chicken coop.");
                                                break;
                                            case 1:
                                                chatService.SendPlayerChatMessage(player, PlayerChatMessageType.DO, "The chickens cluck softly as you handle them gently.");
                                                break;
                                            case 2:
                                                chatService.SendPlayerChatMessage(player, PlayerChatMessageType.ME, "finds a warm egg nestled in the hay.");
                                                break;
                                            default:
                                                player.SendPlayerInfoMessage(PlayerInfoMessageType.INFO, "You successfully collected an egg without disturbing the coop.");
                                                inventoryService.AddItem(eggItemToAdd);
                                                _eggs--;
                                                UpdateTextLabel();
                                                collectEggTimer.Stop();
                                                collectEggTimer.Dispose();
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        chatService.SendPlayerChatMessage(player, PlayerChatMessageType.DO, "The chickens scatter nervously as you stand up, making it impossible to collect any eggs.");
                                        player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You failed to collect an egg without disturbing the coop.");
                                        foreach (Player player in entityManager.GetComponents<Player>())
                                        {
                                            player.PlayOpenCdnStream(openCdnService, "sfx", "chickenAlarmCall.mp3", _objectLinkedTo.Position, 3.0f);
                                        }
                                        collectEggTimer.Stop();
                                        collectEggTimer.Dispose();
                                    }
                                }
                                else
                                {
                                    player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You are no longer near the chicken coop!");
                                    collectEggTimer.Stop();
                                    collectEggTimer.Dispose();
                                }
                                step++;
                            };
                            collectEggTimer.Start();
                        } else
                        {
                            int step = 0;
                            System.Timers.Timer collectEggTimer = new System.Timers.Timer(2500);
                            collectEggTimer.Enabled = true;
                            collectEggTimer.Elapsed += (entity, e) =>
                            {
                                if (IsPlayerNearby(player))
                                {
                                    if (player.SpecialAction == SpecialAction.Duck)
                                    {
                                        switch (step)
                                        {
                                            case 0:
                                                chatService.SendPlayerChatMessage(player, PlayerChatMessageType.ME, "carefully reaches into the chicken coop.");
                                                break;
                                            case 1:
                                                chatService.SendPlayerChatMessage(player, PlayerChatMessageType.DO, "The chickens cluck softly as you handle them gently.");
                                                break;
                                            case 2:
                                                chatService.SendPlayerChatMessage(player, PlayerChatMessageType.ME, "finds no eggs nestled in the hay.");
                                                break;
                                            default:
                                                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "There are currently no eggs in the chicken coop.");
                                                collectEggTimer.Stop();
                                                collectEggTimer.Dispose();
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        chatService.SendPlayerChatMessage(player, PlayerChatMessageType.DO, "The chickens scatter nervously as you stand up, making it impossible to collect any eggs.");
                                        player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You failed to collect an egg without disturbing the coop.");
                                        foreach (Player player in entityManager.GetComponents<Player>())
                                        {
                                            player.PlayOpenCdnStream(openCdnService, "sfx", "chickenAlarmCall.mp3", _objectLinkedTo.Position, 3.0f);
                                        }
                                        collectEggTimer.Stop();
                                        collectEggTimer.Dispose();
                                    }
                                }
                                else
                                {
                                    player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You are no longer near the chicken coop!");
                                    collectEggTimer.Stop();
                                    collectEggTimer.Dispose();
                                }
                                step++;
                            };
                            collectEggTimer.Start();
                        }
                    }
                    else
                    {
                        int step = 0;
                        System.Timers.Timer collectEggTimer = new System.Timers.Timer(2500);
                        collectEggTimer.Enabled = true;
                        collectEggTimer.Elapsed += (entity, e) =>
                        {
                            if (IsPlayerNearby(player))
                            {
                                switch (step)
                                {
                                    case 0:
                                        chatService.SendPlayerChatMessage(player, PlayerChatMessageType.ME, "reaches into the chicken coop whilst standing, unable to see inside the chicken coop.");
                                        break;
                                    case 1:
                                        chatService.SendPlayerChatMessage(player, PlayerChatMessageType.DO, "A feisty chicken pecks aggressively at your hand!");
                                        break;
                                    case 2:
                                        chatService.SendPlayerChatMessage(player, PlayerChatMessageType.ME, "yelps and pulls their hand back quickly, startled by the chicken.");
                                        break;
                                    default:
                                        player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You failed to collect an egg without disturbing the coop.");
                                        foreach (Player player in entityManager.GetComponents<Player>())
                                        {
                                            player.PlayOpenCdnStream(openCdnService, "sfx", "chickenAlarmCall.mp3", _objectLinkedTo.Position, 3.0f);
                                        }
                                        collectEggTimer.Stop();
                                        collectEggTimer.Dispose();
                                        break;
                                    }
                                }
                            else
                            {
                                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You are no longer near the chicken coop!");
                                collectEggTimer.Stop();
                                collectEggTimer.Dispose();
                            }
                            step++;
                        };
                        collectEggTimer.Start();
                    }
                } else
                {
                    player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You do not have enough space in your inventory for this item!");
                }
            } 
            else
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You must be playing as a character in order to do this command!");
            }
        }
    }
}
