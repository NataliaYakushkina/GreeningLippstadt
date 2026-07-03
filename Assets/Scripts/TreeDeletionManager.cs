using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TreeDeletionManager : MonoBehaviour
{
    // UI elements
    public Button deleteButton;
    
    // Camera for raycasting
    private Camera mainCamera;
    
    void Start()
    {
        // Get main camera
        mainCamera = Camera.main;
        
        // Hide delete button initially
        HideDeleteButton();
        
        // Set up button click event
        if (deleteButton)
            deleteButton.onClick.AddListener(DeleteSelectedTree);
    }
    
    void Update()
    {
        // Check for screen touches
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            // Get touch position
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            
            // If UI is clicked, don't raycast (prevents double actions)
            if (IsPointerOverUI(touchPosition))
                return;
            
            // Cast ray from touch position
            Ray ray = mainCamera.ScreenPointToRay(touchPosition);
            RaycastHit hit;
            
            // Check if we hit a tree
            if (Physics.Raycast(ray, out hit))
            {
                // Check if hit object has a TreeSelector component
                TreeSelector selector = hit.collider.GetComponent<TreeSelector>();
                if (selector != null)
                {
                    // Select the tree
                    selector.Select();
                }
                else
                {
                    // Clicked somewhere else, deselect current tree
                    DeselectCurrentTree();
                }
            }
            else
            {
                // Clicked on nothing, deselect current tree
                DeselectCurrentTree();
            }
        }
    }
    
    // Check if the touch is over UI elements
    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        // If using Unity UI, you need to check if touch is over UI elements
        // This is a simplified check - you might need to enhance this
        
        // If delete button is visible and we're clicking in its area
        if (deleteButton && deleteButton.gameObject.activeInHierarchy)
        {
            RectTransform rt = deleteButton.GetComponent<RectTransform>();
            if (rt)
            {
                // Convert screen position to local position in button's rect
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rt, screenPosition, null, out localPoint);
                
                // Check if point is inside rect
                if (rt.rect.Contains(localPoint))
                    return true;
            }
        }
        
        return false;
    }
    
    public void ShowDeleteButton()
    {
        if (deleteButton) deleteButton.gameObject.SetActive(true);
    }
    
    public void HideDeleteButton()
    {
        if (deleteButton) deleteButton.gameObject.SetActive(false);
    }
    
    private void DeselectCurrentTree()
    {
        if (TreeSelector.currentlySelectedTree)
        {
            // Hide selection indicator
            TreeSelector selector = TreeSelector.currentlySelectedTree.GetComponent<TreeSelector>();
            if (selector && selector.selectionIndicator)
                selector.selectionIndicator.SetActive(false);
            
            // Reset static reference
            TreeSelector.currentlySelectedTree = null;
        }
        
        // Hide delete button
        HideDeleteButton();
    }
    
    public void DeleteSelectedTree()
    {
        if (TreeSelector.currentlySelectedTree)
        {
            Destroy(TreeSelector.currentlySelectedTree);
            TreeSelector.currentlySelectedTree = null;
            HideDeleteButton();
        }
    }
}