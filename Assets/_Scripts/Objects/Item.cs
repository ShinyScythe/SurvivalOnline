using PurrNet;
using UnityEngine;

public class Item : AInteractable
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
    // TODO: Figure out why OnHover doesn't get called here.
    public override void OnHover()
    {
        base.OnHover();
    }

    public override void OnStopHover()
    {
        base.OnStopHover();
    }
}
