using PurrNet;
using UnityEngine;
using UnityEngine.UI;

public class ActionSlot : MonoBehaviour
{
    [SerializeField] private Image slotImage;
    [SerializeField] private KeyCode actionKey = KeyCode.Alpha1;
    [SerializeField] private Color activeColor;

    private Color _originalColor;

    private void Awake()
    {
        _originalColor = slotImage.color;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(actionKey))
        {
            return;
        }

        InstanceHandler.GetInstance<InventoryManager>().SetActionSlotActive(this);
    }

    public void ToggleActive(bool toggle)
    {
        slotImage.color = toggle ? activeColor : _originalColor;
    }
}
