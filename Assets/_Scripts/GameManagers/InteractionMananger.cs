using PurrNet;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public class InteractionMananger : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float interactDistance = 4f;

    private Camera _cam;
    private AInteractable[] _currentHoveredInteractables;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        HandleHovers();

        if (!Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.E))
            return;

        if (!Physics.Raycast(_cam.transform.position, _cam.transform.forward, out RaycastHit hit, interactDistance, interactableLayer))
            return;

        var interactables = hit.collider.GetComponents<AInteractable>();
        foreach (var interactable in interactables)
        {
            if (interactable.CanInteract())
                interactable.Interact();
        }
    }

    private void HandleHovers()
    {
        if (!Physics.Raycast(_cam.transform.position, _cam.transform.forward, out RaycastHit hit, interactDistance, interactableLayer))
        {
            ClearHover();
            return;
        }
        var interactables = hit.collider.GetComponents<AInteractable>();

        if (interactables == null || interactables.Length == 0)
        {
            ClearHover(); 
            return;
        }

        if (_currentHoveredInteractables != null && _currentHoveredInteractables.Length > 0)
        {
            if (!_currentHoveredInteractables[0])
            {
                ClearHover();
                return;
            }
            
            if (hit.collider.gameObject == _currentHoveredInteractables[0].gameObject)
                return;
        }
            
            
            
        
        _currentHoveredInteractables = interactables;

        foreach (var interactable in interactables)
        {
            if (interactable.CanInteract())
                interactable.OnHover();
        }
    }

    private void ClearHover()
    {
        if (_currentHoveredInteractables == null || _currentHoveredInteractables.Length <= 0) 
            return;

        foreach (var interactable in _currentHoveredInteractables)
        {
            if (interactable)
                interactable.OnStopHover();
        }
        _currentHoveredInteractables = null;
    }
}



public abstract class AInteractable : NetworkBehaviour
{
    public abstract void Interact();

    public virtual void OnHover() { }

    public virtual void OnStopHover() { }

    public virtual bool CanInteract()
    {
        return true;
    }
}
