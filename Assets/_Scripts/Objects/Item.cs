using PurrNet;
using UnityEngine;

public abstract class Item : AInteractable
{
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemPicture;
    [SerializeField] private Rigidbody rigidbody;

    // gets private items publicly but cannot change them
    public string ItemName => itemName;
    public Sprite ItemPicture => itemPicture;

    protected override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
    {
        base.OnOwnerChanged(oldOwner, newOwner, asServer);

        if (PlayerInventory.localInventory.IsHoldingItem(this))
        {
            rigidbody.isKinematic = true;
            return;
        }
        rigidbody.isKinematic = !isOwner;
    }

    [ContextMenu("Test Pickup")]
    public void Pickup()
    {
        if (!InstanceHandler.TryGetInstance(out InventoryManager inventoryManager))
        {
            Debug.LogError($"Failed to get inventory manager.", this);
            return;
        }

        if (PlayerInventory.localInventory.IsHoldingItem(this))
        {
            Debug.Log("TEST: Attempted to take item in hand.");
            return;
        }
        // TODO: Parse out equipment in FOV, currently takes item out of hand and duplicates

        inventoryManager.AddItem(this);

        Destroy(gameObject);
    }

    public void SetKinematic(bool toggle)
    {
        rigidbody.isKinematic = toggle;
    }

    public override void Interact()
    {
        Pickup();
        print($"Picked up: {ItemName}");
    }
   
    public virtual void UseItem()
    {

    }

    public virtual void ConsumeItem()
    {

    }

    public override void OnHover()
    {
        base.OnHover();
    }

    public override void OnStopHover()
    {
        base.OnStopHover();
    }
}
