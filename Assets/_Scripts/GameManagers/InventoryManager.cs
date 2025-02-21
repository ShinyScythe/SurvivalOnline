using NUnit.Framework;
using PurrNet;
using PurrNet.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private List<Item> allItems = new();

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private InventoryItem itemPrefab;
    [SerializeField] private List<InventorySlot> slots = new();
    [SerializeField] private List<ActionSlot> actionSlots = new();

    [PurrReadOnly, SerializeField] private InventoryItemData[] _inventoryData;
    private ActionSlot _activeActionSlot;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
        _inventoryData = new InventoryItemData[slots.Count];

        // hides inventory
        ToggleInventory(false);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<InventoryManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            bool isOpen = canvasGroup.alpha > 0;
            ToggleInventory(!isOpen); 
        }
    }

    private void ToggleInventory(bool toggle)
    {
        canvasGroup.alpha = toggle ? 1 : 0;
        canvasGroup.blocksRaycasts = toggle;

        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = toggle;
    }

    public void AddItem(Item item)
    {
        if (!TryStackItem(item))
            AddNewItem(item);
    }

    private bool TryStackItem(Item item)
    {        
        for (int i = 0; i < _inventoryData.Length; i++)
        {
            var data = _inventoryData[i];
            if (String.IsNullOrEmpty(data.itemName)) // check if empty
                continue;

            if (data.itemName !=  item.ItemName) // check if mismatch
                continue;

            // increments item amount and initializes struct again
            data.amount++;
            data.inventoryItem.Init(item.ItemName, item.ItemPicture, data.amount);

            // assigns the data into array
            _inventoryData[i] = data;

            return true; // stackable
        }
        return false; // not stackable
    }

    private void AddNewItem(Item item)
    {
        for (var i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (!slot.IsEmpty)
            {
                continue;
            }

            var inventoryItem = Instantiate(itemPrefab, slot.transform);
            inventoryItem.Init(item.ItemName, item.ItemPicture, 1);

            var itemData = new InventoryItemData()
            {
                itemPicture = item.ItemPicture,
                inventoryItem = inventoryItem,
                itemName = item.ItemName,
                amount = 1
            };

            _inventoryData[i] = itemData;
            slot.SetItem(inventoryItem);
            break;
        }
    }

    public void ItemMoved(InventoryItem item, InventorySlot newSlot)
    {
        var newSlotIndex = slots.IndexOf(newSlot);
        var oldSlotIndex = Array.FindIndex(_inventoryData, x => x.inventoryItem == item);
        if (oldSlotIndex == -1)
        {
            Debug.LogError($"Couldn't find item {item.name} in inventory data.", this);
            return;
        }
        // store the old data
        var oldData = _inventoryData[oldSlotIndex];
        // wipe the old data
        _inventoryData[oldSlotIndex] = default;
        // store the new data
        _inventoryData[newSlotIndex] = oldData;
    }

    public void DropItem(InventoryItem inventoryItem)
    {
        for (int i = 0; i < _inventoryData.Length; i++)
        {
            var data = _inventoryData[i];
            if (data.inventoryItem != inventoryItem)
                continue;

            var itemToSpawn = GetItemByName(data.itemName);
            if (itemToSpawn == null)
            {
                Debug.LogError($"Item to spawn with name {data.itemName} not found.");
                return;
            }

            PlayerInventory.localInventory.UnequipItem(itemToSpawn);

            // set position in front of local player
            Vector3 spawnPosition = PlayerController.localPlayerController.transform.position + PlayerController.localPlayerController.transform.forward + Vector3.up;
            var item = Instantiate(itemToSpawn, spawnPosition, Quaternion.identity);

            if (DeductItem(inventoryItem) <= 0)
                PlayerInventory.localInventory.UnequipItem(itemToSpawn);
            
            break;
        }
    }

    private int DeductItem(InventoryItem inventoryItem)
    {
        for (int i = 0; i < _inventoryData.Length; i++)
        {
            var data = _inventoryData[i];
            if (data.inventoryItem != inventoryItem)
                continue;

            data.amount--;

            if (data.amount <= 0)
            {
                _inventoryData[i] = default;
                slots[i].SetItem(null);
                Destroy(inventoryItem.gameObject);
                return 0;
            }
            else
            {
                data.inventoryItem.Init(data.itemName, data.itemPicture, data.amount);
                _inventoryData[i] = data;
                return data.amount;
            }
        }
        return 0;
    }
    private Item GetItemByName(string itemName)
    {
        return allItems.Find(x => x.ItemName == itemName);
    }

    private Item GetItemByActionSlot(ActionSlot actionSlot)
    {
        var inventorySlot = actionSlot.GetComponent<InventorySlot>();
        // iterate backwards since actions slots at end of slots list
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i] == inventorySlot)
                return GetItemByName(_inventoryData[i].itemName);
        }
        return null;
    }

    public void SetActionSlotActive(ActionSlot actionSlot)
    {
        if (_activeActionSlot == actionSlot) { return; }

        if (_activeActionSlot != null) 
        {
            PlayerInventory.localInventory.UnequipItem(GetItemByActionSlot(_activeActionSlot));
            _activeActionSlot.ToggleActive(false);
        }

        actionSlot.ToggleActive(true);
        _activeActionSlot = actionSlot;

        PlayerInventory.localInventory.EquipItem(GetItemByActionSlot(actionSlot));
    }

    [SerializeField]
    public struct InventoryItemData
    {
        public InventoryItem inventoryItem;
        public Sprite itemPicture;
        public String itemName;
        public int amount;
    }
}


