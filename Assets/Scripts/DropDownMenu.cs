using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro Dropdown

public class TreeDropdownMenu : MonoBehaviour
{
    [Header("References")]
    public TreePlacer treePlacer;
    public TMP_Dropdown dropdown; // Use Dropdown if using legacy UI
    
    [Header("Settings")]
    public bool autoPlaceOnSelection = true; // Place tree immediately when selected
    
    void Start()
    {
        // Find TreePlacer if not assigned
        if (treePlacer == null)
        {
            treePlacer = FindObjectOfType<TreePlacer>();
            if (treePlacer == null)
            {
                Debug.LogError("TreePlacer not found!");
                return;
            }
        }
        
        // Find dropdown if not assigned
        if (dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
            if (dropdown == null)
            {
                Debug.LogError("Dropdown component not found!");
                return;
            }
        }
        
        // Populate dropdown with tree names
        //PopulateDropdown();
        
        // Add listener for dropdown selection
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }
    
    void PopulateDropdown()
    {
        // Clear existing options
        dropdown.ClearOptions();
        
        // Create list of tree names
        List<string> treeNames = new List<string>();
        
        if (treePlacer.treePrefabs != null)
        {
            for (int i = 0; i < treePlacer.treePrefabs.Length; i++)
            {
                GameObject treePrefab = treePlacer.treePrefabs[i];
                if (treePrefab != null)
                {
                    treeNames.Add(treePrefab.name);
                }
                else
                {
                    treeNames.Add($"Tree {i + 1}");
                }
            }
        }
        
        // Add options to dropdown
        dropdown.AddOptions(treeNames);
        
        Debug.Log($"Dropdown populated with {treeNames.Count} tree options");
    }
    
    void OnDropdownValueChanged(int index)
    {
        Debug.Log($"Dropdown selection changed to index: {index}");
        
        if (treePlacer == null)
        {
            Debug.LogError("TreePlacer is null!");
            return;
        }
        
        if (autoPlaceOnSelection)
        {
            // Select and place the tree immediately
            treePlacer.SelectTreePrefab(index);
            treePlacer.PlaceTreeAtCurrentPlanePosition();
        }
        else
        {
            // Just select the tree, don't place it yet
            treePlacer.SelectTreePrefab(index);
        }
    }
    
    // Public method to place the currently selected tree
    public void PlaceSelectedTree()
    {
        if (treePlacer != null)
        {
            treePlacer.PlaceTreeAtCurrentPlanePosition();
        }
    }
}
