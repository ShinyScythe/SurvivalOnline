using UnityEngine;

public class Consumable : Item
{
    public override void ConsumeItem()
    {
        base.ConsumeItem();

        Debug.Log("Consuming item");
    }
}
