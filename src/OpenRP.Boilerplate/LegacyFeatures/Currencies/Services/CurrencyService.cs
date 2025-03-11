using OpenRP.Framework.Database.Models;
using OpenRP.Framework.Database.Services;
using OpenRP.Framework.Features.Characters.Components;
using OpenRP.Framework.Features.Players.Extensions;
using OpenRP.Framework.Shared.Chat.Enums;
using OpenRP.Framework.Shared.Chat.Extensions;
using OpenRP.Framework.Shared.ServerEvents.Services;
using OpenRP.Boilerplate.Data;
using OpenRP.Boilerplate.LegacyFeatures.Characters.Services;
using OpenRP.Boilerplate.LegacyFeatures.Currencies.Entities;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Helpers;
using OpenRP.Boilerplate.LegacyFeatures.Inventories.Services;
using SampSharp.Entities.SAMP;
using System.Globalization;
using OpenRP.Framework.Shared.ServerEvents.Entities.EventArgs;

namespace OpenRP.Boilerplate.LegacyFeatures.Currencies.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICharacterService _characterService;
        private readonly IInventoryService _inventoryService;
        private readonly IServerEventAggregator _serverEventAggregator;
        private readonly IDataMemoryService _dataMemoryService;
        private readonly DataContext _dataContext;

        public CurrencyService(ICharacterService characterService, IInventoryService inventoryService, DataContext dataContext, IServerEventAggregator serverEventAggregator, IDataMemoryService dataMemoryService) 
        {
            _characterService = characterService;
            _inventoryService = inventoryService;
            _dataContext = dataContext;
            _serverEventAggregator = serverEventAggregator;
            _dataMemoryService = dataMemoryService;
        }

        public async Task<BillsTransaction> GivePlayerCurrency(Player player, uint currencyId, decimal amount)
        {
            // Prepare a transaction result
            BillsTransaction transaction = new BillsTransaction
            {
                BillsAdded = new List<InventoryItemModel>(),
                BillsRemoved = new List<InventoryItemModel>()
            };

            // Ensure the player is actually playing as a character
            if (!player.IsPlayerPlayingAsCharacter())
            {
                player.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR,
                    "You must be playing as a character in order to do this!");
                return transaction;
            }

            Character character = player.GetComponent<Character>();

            // Retrieve the character's wallet inventory for currency transactions
            InventoryModel wallet = _characterService.GetCharacterWallet(character, _dataMemoryService);
            if (wallet == null)
            {
                throw new InvalidOperationException("Character wallet not found.");
            }

            // Get all currency items (bills) for the requested currency, largest to smallest
            Dictionary<ItemModel, CurrencyUnitModel> currencyUnitItemDictionary = _dataMemoryService.GetItems()
                .Where(i => i.IsItemCurrency() && i.GetItemCurrencyUnit(_dataMemoryService)?.CurrencyId == currencyId)
                .OrderByDescending(i => i.GetItemCurrencyUnit(_dataMemoryService)?.UnitValue)
                .ToDictionary(k => k, v => v.GetItemCurrencyUnit(_dataMemoryService));

            // Get bills already in the wallet for the requested currency, largest to smallest
            Dictionary<InventoryItemModel, CurrencyUnitModel> currencyUnitsInvItemDictionary = wallet.GetInventoryItems()
                .Where(i => i.GetItem(_dataMemoryService).IsItemCurrency() && i.GetItem(_dataMemoryService).GetItemCurrencyUnit(_dataMemoryService)?.CurrencyId == currencyId)
                .OrderByDescending(i => i.GetItem(_dataMemoryService).GetItemCurrencyUnit(_dataMemoryService)?.UnitValue)
                .ToDictionary(k => k, v => v.GetItem(_dataMemoryService).GetItemCurrencyUnit(_dataMemoryService));

            // ----------------------------------
            // CASE 1: Positive => Give the player
            // ----------------------------------
            if (amount > 0)
            {
                decimal remaining = amount;

                // Go through each denomination in descending order
                foreach (var (item, currencyUnit) in currencyUnitItemDictionary)
                {
                    if (remaining <= 0)
                        break;

                    // Calculate how many bills of this type we can create
                    uint billsToAdd = (uint)(remaining / currencyUnit.UnitValue);
                    if (billsToAdd == 0)
                        continue;

                    // "Prepare" these bills to be added
                    // (Presumably creates an InventoryItemModel object with the correct quantity)
                    InventoryItemModel itemPrepare = _inventoryService.PrepareItem(wallet.Id, item, billsToAdd);

                    // Track in transaction
                    transaction.BillsAdded.Add(itemPrepare);

                    // Deduct the amount we just allocated
                    remaining -= (billsToAdd * currencyUnit.UnitValue);
                }
            }
            // -------------------------------------------------
            // CASE 2: Negative => Take from the player (pay out)
            // -------------------------------------------------
            else if (amount < 0)
            {
                decimal amountNeeded = Math.Abs(amount);

                // Start from the largest bill in the wallet
                foreach (var (invItem, currencyUnit) in currencyUnitsInvItemDictionary)
                {
                    if (amountNeeded <= 0)
                        break;

                    decimal billValue = currencyUnit.UnitValue;
                    uint countInInventory = invItem.Amount; // how many bills of this type we have

                    // If even a single bill is bigger than what's needed, remove 1 and give change
                    if (billValue > amountNeeded && countInInventory > 0)
                    {
                        // Prepare to remove exactly 1 from this item
                        InventoryItemModel removeOne = _inventoryService.PrepareItem(wallet.Id, invItem.GetItem(_dataMemoryService), 1);
                        transaction.BillsRemoved.Add(removeOne);

                        // We covered the needed amount, so figure out the difference
                        decimal difference = billValue - amountNeeded;
                        amountNeeded = 0; // fully covered

                        // Give the difference back in smaller bills
                        foreach (var (smallerItem, smallerCurrencyUnit) in currencyUnitItemDictionary
                                 .Where(x => x.Value.UnitValue < billValue) // strictly smaller denominations
                                 .OrderByDescending(x => x.Value.UnitValue))
                        {
                            uint billsToAdd = (uint)(difference / smallerCurrencyUnit.UnitValue);

                            // Prepare 1 smaller bill
                            InventoryItemModel changeBill = _inventoryService.PrepareItem(wallet.Id, smallerItem, billsToAdd);
                                transaction.BillsAdded.Add(changeBill);

                                difference -= (billsToAdd * smallerCurrencyUnit.UnitValue);

                            if (difference <= 0) break;
                        }
                        // If difference > 0 here, we can't fully represent that leftover. 
                        // Decide how to handle partial or rounding if your game demands it.
                    }
                    else
                    {
                        // If the total value of these bills is <= amountNeeded, remove them all
                        decimal totalValue = billValue * countInInventory;
                        if (totalValue <= amountNeeded)
                        {
                            // Prepare to remove all of them
                            InventoryItemModel removeAll = _inventoryService.PrepareItem(wallet.Id, invItem.GetItem(_dataMemoryService), countInInventory);
                            transaction.BillsRemoved.Add(removeAll);

                            amountNeeded -= totalValue;
                        }
                        else
                        {
                            // Only remove as many as needed
                            uint billsToRemove = (uint)(amountNeeded / billValue);
                            if (billsToRemove == 0)
                                continue; // if still 0, we skip

                            InventoryItemModel removePartial = _inventoryService.PrepareItem(wallet.Id, invItem.GetItem(_dataMemoryService), billsToRemove);
                            transaction.BillsRemoved.Add(removePartial);

                            amountNeeded -= (billsToRemove * billValue);
                        }
                    }
                }

                // If there's still money owed, we couldn't pay it fully (nor exchange properly).
                if (amountNeeded > 0)
                {
                    return new BillsTransaction()
                    {
                        NotEnoughFunds = true,
                        AmountRemaining = amountNeeded,
                    };
                }
            }

            // -----------------------------------------------
            // Apply final changes to the wallet via service
            // -----------------------------------------------
            foreach (InventoryItemModel billsToAdd in transaction.BillsAdded)
            {
                uint availableWeight = wallet.GetAvailableWeight(_dataMemoryService);
                uint itemsWeight = billsToAdd.GetTotalWeight(_dataMemoryService);
                uint newWeight = availableWeight - itemsWeight;
                if (newWeight < wallet.MaxWeight)
                {
                    // Actually add them into the wallet
                    _inventoryService.AddItem(billsToAdd);
                } else
                {
                    return new BillsTransaction()
                    {
                        NotEnoughWeight = true
                    };
                }
            }

            foreach (InventoryItemModel billsToRemove in transaction.BillsRemoved)
            {
                // Actually remove them from the wallet
                _inventoryService.RemoveItem(billsToRemove);
            }

            var eventArgs = new OnPlayerCashUpdateEventArgs
            {
                Player = player,
                CurrencyId = currencyId,
            };
            await _serverEventAggregator.PublishAsync(eventArgs);

            return transaction;
        }

        public async Task<BillsTransaction> PrepareBillTransactionBetweenPlayers(
            Player sourcePlayer,
            Player destPlayer,
            ulong currencyId,
            decimal amount)
        {
            var transaction = new BillsTransaction();

            // Validate both characters
            if (!sourcePlayer.IsPlayerPlayingAsCharacter() || !destPlayer.IsPlayerPlayingAsCharacter())
            {
                sourcePlayer.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "Either you or the recipient is not logged into their character.");
                return transaction;
            }

            // Characters
            Character sourceCharacter = sourcePlayer.GetPlayerCurrentlyPlayingAsCharacter();
            Character destCharacter = destPlayer.GetPlayerCurrentlyPlayingAsCharacter();

            // Get wallets
            var sourceWallet = _characterService.GetCharacterWallet(sourceCharacter, _dataMemoryService);
            var destWallet = _characterService.GetCharacterWallet(destCharacter, _dataMemoryService);
            if (sourceWallet == null || destWallet == null)
            {
                sourcePlayer.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "Either you or the recipient do not appear to have a wallet or inventory!");
                return new BillsTransaction { TransferNotPossible = true };
            }

            // Validate amount
            if (amount <= 0)
            {
                sourcePlayer.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "Transfer amount must be positive!");
                return new BillsTransaction { TransferNotPossible = true };
            }

            // Get exact bills from source
            var sourceBills = GetExactBillCombination(sourceWallet, currencyId, amount);
            if (sourceBills == null)
            {
                sourcePlayer.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "You do not have enough money to do this transfer!");
                return new BillsTransaction { TransferNotPossible = true };
            }

            // Check destination weight
            var totalWeight = sourceBills.Sum(b => b.GetTotalWeight(_dataMemoryService));
            if (destWallet.GetAvailableWeight(_dataMemoryService) < totalWeight)
            {
                sourcePlayer.SendPlayerInfoMessage(PlayerInfoMessageType.ERROR, "The recipient does not have enough space in their inventory!");
                return new BillsTransaction { NotEnoughWeight = true };
            }

            // Perform transfer
            try
            {
                // Remove from source
                foreach (var bill in sourceBills)
                {
                    transaction.BillsRemoved.Add(bill);
                }

                // Add to destination
                foreach (var bill in sourceBills.Select(b =>
                    _inventoryService.PrepareItem(destWallet.Id, b.GetItem(_dataMemoryService), b.Amount)))
                {
                    transaction.BillsAdded.Add(bill);
                }

                // Publish events
                /*await _serverEventAggregator.PublishAsync(new OnPlayerCashUpdateEventArgs
                {
                    Player = sourcePlayer,
                    CurrencyId = currencyId
                });
                await _serverEventAggregator.PublishAsync(new OnPlayerCashUpdateEventArgs
                {
                    Player = destPlayer,
                    CurrencyId = currencyId
                });*/

                return transaction;
            }
            catch
            {
                // Rollback logic would go here in production
                return transaction;
            }
        }

        private List<InventoryItemModel> GetExactBillCombination(
            InventoryModel wallet,
            ulong currencyId,
            decimal amount)
        {
            var bills = wallet.GetInventoryItems()
                .Where(i => i.GetItem(_dataMemoryService).IsItemCurrency() && i.GetItem(_dataMemoryService).GetItemCurrencyUnit(_dataMemoryService).CurrencyId == currencyId)
                .OrderByDescending(i => i.GetItem(_dataMemoryService).GetItemCurrencyUnit(_dataMemoryService)?.UnitValue)
                .ToList();

            return FindExactCombination(bills, amount, new List<InventoryItemModel>());
        }

        private List<InventoryItemModel> FindExactCombination(
            List<InventoryItemModel> availableBills,
            decimal remaining,
            List<InventoryItemModel> currentSelection)
        {
            if (remaining == 0) return currentSelection;
            if (!availableBills.Any()) return null;

            var currentBill = availableBills.First();
            var billValue = currentBill.GetItem(_dataMemoryService).GetItemCurrencyUnit(_dataMemoryService).UnitValue;
            var maxTake = Math.Min((uint)(remaining / billValue), currentBill.Amount);

            for (var take = maxTake; take >= 0; take--)
            {
                if (take == 0) // Skip this bill
                {
                    List<InventoryItemModel> newAvailableBills = availableBills.Skip(1).ToList();

                    var result = FindExactCombination(
                        newAvailableBills,
                        remaining,
                        currentSelection
                    );
                    if (result != null) return result;
                    if (!newAvailableBills.Any()) return null;
                }
                else // Try taking this bill
                {
                    List<InventoryItemModel> newAvailableBills = availableBills.Skip(1).ToList();

                    var newRemaining = remaining - (take * billValue);
                    var newSelection = new List<InventoryItemModel>(currentSelection)
                    {
                        _inventoryService.PrepareItem(
                            currentBill.InventoryId,
                            currentBill.GetItem(_dataMemoryService),
                            take
                        )
                    };

                    var result = FindExactCombination(
                        newAvailableBills,
                        newRemaining,
                        newSelection
                    );

                    if (result != null) return result;
                    if (!newAvailableBills.Any()) return null;
                }
            }

            return null;
        }

        public Dictionary<InventoryItemModel, CurrencyUnitModel> GetCharacterInventoryCurrencyUnitsAsDictionary(Character character)
        {
            Dictionary<InventoryItemModel, CurrencyUnitModel> currencyUnits = new Dictionary<InventoryItemModel, CurrencyUnitModel>();

            List<InventoryItemModel> currencyUnitInventoryItems = GetCharacterInventoryCurrencyUnits(character);
            foreach (InventoryItemModel item in currencyUnitInventoryItems)
            {
                CurrencyUnitModel currencyUnit = item.GetItem(_dataMemoryService).GetItemCurrencyUnit(_dataMemoryService);

                if (currencyUnit != null)
                {
                    currencyUnits.Add(item, currencyUnit);
                }
            }
            return currencyUnits;
        }

        public List<InventoryItemModel> GetCharacterInventoryCurrencyUnits(Character character)
        {
            return _characterService.GetCharacterWallet(character, _dataMemoryService).GetInventoryItems().Where(i => i.GetItem(_dataMemoryService).IsItemCurrency()).ToList();
        }

        public string GetCurrencyFormat(ulong currencyId, decimal amount)
        {
            switch (currencyId)
            {
                case 2: // Caeroyna
                    return amount.ToString("C", CultureInfo.CreateSpecificCulture("en-US")).Replace("$", "CRU ");
            }
            return amount.ToString("C", CultureInfo.CreateSpecificCulture("en-US"));
        }

        public decimal GetCharacterCurrency(Character character, ulong currencyId)
        {
            using (DataContext context = new DataContext())
            {
                // Get all currency units for the specified currencyId
                var currencyUnits = _dataMemoryService.GetCurrencyUnits()
                    .Where(cu => cu.CurrencyId == currencyId)
                    .ToDictionary(cu => cu.Id, cu => cu.UnitValue);

                // Get the character's wallet inventory
                var walletInventory = _characterService.GetCharacterWallet(character, _dataMemoryService);
                if (walletInventory == null)
                    return 0;

                // Recursive function to calculate currency for an inventory and its sub-inventories
                decimal CalculateInventoryCurrency(InventoryModel inventory)
                {
                    // Get all items in the current inventory
                    var inventoryItems = context.InventoryItems
                        .Where(ii => ii.InventoryId == inventory.Id)
                        .ToList();

                    // Calculate the total currency for this inventory
                    decimal totalCurrency = inventoryItems
                        .Where(ii =>
                        {
                            var parsedValue = ItemAdditionalData.Parse(ii.GetItem(_dataMemoryService).UseValue)
                                                .GetString("CURRENCY_UNIT");
                            return ulong.TryParse(parsedValue, out var currencyUnitId)
                                   && currencyUnits.ContainsKey(currencyUnitId);
                        })
                        .Sum(ii =>
                        {
                            var parsedValue = ItemAdditionalData.Parse(ii.GetItem(_dataMemoryService).UseValue)
                                                .GetString("CURRENCY_UNIT");
                            var currencyUnitId = ulong.Parse(parsedValue); // Safe since TryParse was successful earlier
                            return currencyUnits[currencyUnitId] * ii.Amount;
                        });

                    // Check for sub-inventories within the current inventory
                    foreach (var subItem in inventoryItems.Where(ii => ii.GetItem(_dataMemoryService).IsItemInventory()))
                    {
                        var subInventoryId = ItemAdditionalData.Parse(subItem.AdditionalData)
                                                .GetString("INVENTORY");

                        if (ulong.TryParse(subInventoryId, out var subInventoryIdParsed))
                        {
                            var subInventory = context.Inventories
                                                .SingleOrDefault(inv => inv.Id == subInventoryIdParsed);
                            if (subInventory != null)
                            {
                                // Add the currency from the sub-inventory
                                totalCurrency += CalculateInventoryCurrency(subInventory);
                            }
                        }
                    }

                    return totalCurrency;
                }

                // Start calculation from the wallet inventory
                return CalculateInventoryCurrency(walletInventory);
            }
        }
    }
}
