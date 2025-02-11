using PurrNet;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    private InventoryItem _item;
    public bool IsEmpty => _item == null;
    public InventoryItem Item => _item;

    public void SetItem(InventoryItem item)
    {
        _item = item;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        eventData.pointerDrag.transform.SetParent(transform);
        var inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        inventoryItem.SetAvailable();

        if (!InstanceHandler.TryGetInstance(out InventoryManager inventoryManager))
        {
            Debug.LogError($"Failed to get inventory manager.", this);
            return;
        }

        inventoryManager.ItemMoved(inventoryItem, this);
    }
}
