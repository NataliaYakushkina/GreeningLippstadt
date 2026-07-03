using UnityEngine;
using UnityEngine.UI;

public class TreeButtonController : MonoBehaviour
{
    // Reference to tree placer
    public TreePlacer treePlacer;
    
    // Start is called before the first frame update
    void Start()
    {
        // Find TreePlacer if not assigned
        if (treePlacer == null)
        {
            treePlacer = FindObjectOfType<TreePlacer>();
            if (treePlacer == null)
            {
                Debug.LogError("TreePlacer not found in scene!");
            }
        }
        
        // Set up all child buttons
        SetupButtons();
    }
    
    void SetupButtons()
    {
        // Find all buttons in children
        Button[] buttons = GetComponentsInChildren<Button>();
        
        // For each button, add a listener
        for (int i = 0; i < buttons.Length; i++)
        {
            // Store the index for the lambda
            int treeIndex = i;
            
            // Clear existing listeners
            buttons[i].onClick.RemoveAllListeners();
            
            // Add listener for tree placement
            buttons[i].onClick.AddListener(() => PlaceTree(treeIndex));
        }
    }
    
    // Place the tree with the specified index
    public void PlaceTree(int treeIndex)
    {
        if (treePlacer != null)
        {
            // First select the tree prefab
            Debug.Log($"Selecting tree with index: {treeIndex}");
            treePlacer.SelectTreePrefab(treeIndex);
            
            // Then place it at the current position
            Debug.Log("Placing tree at current position");
            treePlacer.PlaceTreeAtCurrentPlanePosition();
        }
        else
        {
            Debug.LogError("TreePlacer reference is null!");
        }
    }
}