using UnityEngine;
using UnityEngine.UI;

public class TreeSelector : MonoBehaviour
{
    // Visual indicator (optional)
    public GameObject selectionIndicator;
    
    // Static reference to currently selected tree
    public static GameObject currentlySelectedTree;
    
    // Reference to the deletion manager
    public TreeDeletionManager deletionManager;
    
    // For AR raycasting
    private Camera mainCamera;
    
    void Start()
    {
        // Hide selection indicator if it exists
        if (selectionIndicator) selectionIndicator.SetActive(false);
        
        // Find deletion manager in scene
        deletionManager = FindObjectOfType<TreeDeletionManager>();
        if (deletionManager == null)
            Debug.LogWarning("TreeDeletionManager not found in scene!");
            
        // Get main camera for raycasting
        mainCamera = Camera.main;
    }
    
    // This will be called by the TreeDeletionManager when a tap is detected on this tree
    public void Select()
    {
        // Deselect previous tree if different
        if (currentlySelectedTree && currentlySelectedTree != gameObject)
        {
            TreeSelector prevSelector = currentlySelectedTree.GetComponent<TreeSelector>();
            if (prevSelector && prevSelector.selectionIndicator)
                prevSelector.selectionIndicator.SetActive(false);
        }
        
        // Select this tree
        currentlySelectedTree = gameObject;
        
        // Show selection indicator
        if (selectionIndicator) selectionIndicator.SetActive(true);
        
        // Show delete button
        if (deletionManager) deletionManager.ShowDeleteButton();
    }
    
    // For visual feedback on selection (optional)
    public void Highlight()
    {
        // You could change material color or add a highlight effect
        // For example:
        Renderer renderer = GetComponent<Renderer>();
        if (renderer)
        {
            // Store original color and make it slightly brighter
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.Lerp(originalColor, Color.white, 0.2f);
        }
    }
    
    public void Unhighlight()
    {
        // Reset any visual changes
        Renderer renderer = GetComponent<Renderer>();
        if (renderer)
        {
            // Return to original color
            renderer.material.color = renderer.material.color;
        }
    }
}