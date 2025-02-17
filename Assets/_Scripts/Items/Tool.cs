using UnityEngine;

public class Tool : Item
{
    public override void UseItem()
    {
        base.UseItem();

        Debug.Log("Using tool");
    }
}
