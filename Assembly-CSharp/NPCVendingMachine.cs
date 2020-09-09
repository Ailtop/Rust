using Oxide.Core;
using UnityEngine;

public class NPCVendingMachine : VendingMachine
{
	public NPCVendingOrder vendingOrders;

	public byte GetBPState(bool sellItemAsBP, bool currencyItemAsBP)
	{
		byte result = 0;
		if (sellItemAsBP)
		{
			result = 1;
		}
		if (currencyItemAsBP)
		{
			result = 2;
		}
		if (sellItemAsBP && currencyItemAsBP)
		{
			result = 3;
		}
		return result;
	}

	public override void TakeCurrencyItem(Item takenCurrencyItem)
	{
		if (Interface.CallHook("OnTakeCurrencyItem", this, takenCurrencyItem) == null)
		{
			takenCurrencyItem.MoveToContainer(base.inventory);
			takenCurrencyItem.RemoveFromContainer();
			takenCurrencyItem.Remove();
		}
	}

	public override void GiveSoldItem(Item soldItem, BasePlayer buyer)
	{
		if (Interface.CallHook("OnNpcGiveSoldItem", this, soldItem, buyer) == null)
		{
			Item item = ItemManager.Create(soldItem.info, soldItem.amount, 0uL);
			if (soldItem.blueprintTarget != 0)
			{
				item.blueprintTarget = soldItem.blueprintTarget;
			}
			base.GiveSoldItem(soldItem, buyer);
			transactionActive = true;
			if (!item.MoveToContainer(base.inventory))
			{
				Debug.LogWarning("NPCVending machine unable to refill item :" + soldItem.info.shortname + " buyer :" + buyer.displayName + " - Contact Developers");
				item.Remove();
			}
			transactionActive = false;
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Invoke(InstallFromVendingOrders, 1f);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		skinID = 861142659uL;
		SendNetworkUpdate();
		Invoke(InstallFromVendingOrders, 1f);
	}

	public virtual void InstallFromVendingOrders()
	{
		if (vendingOrders == null)
		{
			Debug.LogError("No vending orders!");
			return;
		}
		ClearSellOrders();
		base.inventory.Clear();
		ItemManager.DoRemoves();
		NPCVendingOrder.Entry[] orders = vendingOrders.orders;
		for (int i = 0; i < orders.Length; i++)
		{
			NPCVendingOrder.Entry entry = orders[i];
			AddItemForSale(entry.sellItem.itemid, entry.sellItemAmount, entry.currencyItem.itemid, entry.currencyAmount, GetBPState(entry.sellItemAsBP, entry.currencyAsBP));
		}
	}

	public override void InstallDefaultSellOrders()
	{
		base.InstallDefaultSellOrders();
	}

	public void ClearSellOrders()
	{
		sellOrders.sellOrders.Clear();
	}

	public void AddItemForSale(int itemID, int amountToSell, int currencyID, int currencyPerTransaction, byte bpState)
	{
		AddSellOrder(itemID, amountToSell, currencyID, currencyPerTransaction, bpState);
		transactionActive = true;
		int num = 10;
		if (bpState == 1 || bpState == 3)
		{
			for (int i = 0; i < num; i++)
			{
				Item item = ItemManager.CreateByItemID(blueprintBaseDef.itemid, 1, 0uL);
				item.blueprintTarget = itemID;
				base.inventory.Insert(item);
			}
		}
		else
		{
			base.inventory.AddItem(ItemManager.FindItemDefinition(itemID), amountToSell * num, 0uL);
		}
		transactionActive = false;
		RefreshSellOrderStockLevel();
	}

	public void RefreshStock()
	{
	}

	public override bool CanPlayerAdmin(BasePlayer player)
	{
		object obj = Interface.CallHook("CanAdministerVending", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return false;
	}
}
