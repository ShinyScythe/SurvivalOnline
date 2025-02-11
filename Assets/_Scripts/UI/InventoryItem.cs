using PurrNet;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private TMP_Text amountText;

    [Header("References")]
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Transform _originalParent;
    private Canvas _canvas;
    private Image _itemImage;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _itemImage = GetComponent<Image>();
        _canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalParent = transform.parent;
        _canvasGroup.blocksRaycasts = false; // don't block
        _rectTransform.SetParent(_rectTransform.root); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerEnter == null || !eventData.pointerEnter.TryGetComponent(out InventorySlot inventorySlot))
        {
            _canvasGroup.blocksRaycasts = true;
            _rectTransform.SetParent(_originalParent);
            SetAvailable();
        }
        
    }

    public void SetAvailable()
    {
        _canvasGroup.blocksRaycasts = true;
        _rectTransform.anchoredPosition = Vector2.zero;
    }

    public void Init(string itemName, Sprite itemPicture, int amount)
    {
        _itemImage.sprite = itemPicture;
        amountText.text = amount.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        if (!InstanceHandler.TryGetInstance(out InventoryManager inventoryManager))
        {
            Debug.Log($"Failed to get inventory mananger to drop item.");
            return;
        }

        inventoryManager.DropItem(this);
    }
}
