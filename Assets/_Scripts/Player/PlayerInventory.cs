using UnityEngine;
using PurrNet;
using System;

public class PlayerInventory : NetworkBehaviour

{
    public static PlayerInventory localInventory;

    [SerializeField] private KeyCode useItemKey, consumeItemKey;

    [SerializeField] private Transform itemPoint;
    private Item _itemInHand;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        if (!isOwner) { return; }

        localInventory = this;
    }

    protected override void OnDespawned()
    {
        base.OnDespawned();
        if (!isOwner) { return; }

        localInventory = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(useItemKey))
        {
            useItem();
        }

        if (Input.GetKeyDown(consumeItemKey))
        {
            ConsumeItem();
        }
    }

    private void ConsumeItem()
    {
        if (!_itemInHand)
            return;

        _itemInHand.ConsumeItem();
    }

    private void useItem()
    {
        if (!_itemInHand)
            return;

        _itemInHand.UseItem();
    }

    public void EquipItem(Item item)
    {
        if (!item) { return; }

        _itemInHand = Instantiate(item, itemPoint.position, itemPoint.rotation, itemPoint);

        _itemInHand.SetKinematic(true);
    }
    public void UnequipItem(Item item)
    {
        if (!item) { return; }

        if (!_itemInHand) { return; }

        if (_itemInHand.ItemName != item.ItemName) { return; }

        Destroy(_itemInHand.gameObject);
        _itemInHand = null;

        Debug.Log($"Attempting to unequip item: {item.ItemName}");
    }

    public bool IsHoldingItem(Item item)
    {
        if (!_itemInHand) { return false; }

        return item == _itemInHand;
    }
}
