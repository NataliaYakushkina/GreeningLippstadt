using UnityEngine;
using Vuforia;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class TreePlacer : MonoBehaviour
{
    public PlaneFinderBehaviour planeFinder;
    public GameObject[] treePrefabs; // Array of tree prefabs for carousel
    public SwipeMenu swipeMenu; // Reference to the SwipeMenu script

    [Header("Tree Placement")]
    public float placementHeight = 0f; // Height offset for tree placement
    public bool resetAfterPlacement = true; // Whether to reset selection after placing a tree

    private GameObject selectedTreePrefab = null; // Selected tree prefab
    private Camera arCamera;
    private int currentTreeIndex = 0; // Current index in the treePrefabs array

    void Start()
    {
        // Get the AR camera
        arCamera = Camera.main;

        // Enable enhanced touch support
        EnhancedTouchSupport.Enable();

        // Initialize with first tree if available
        if (treePrefabs != null && treePrefabs.Length > 0)
        {
            selectedTreePrefab = treePrefabs[0];
        }
    }

    void OnEnable()
    {
        planeFinder.OnAutomaticHitTest.AddListener(OnAutomaticHitTest);
    }

    void OnDisable()
    {
        planeFinder.OnAutomaticHitTest.RemoveListener(OnAutomaticHitTest);
    }

    void OnDestroy()
    {
        // Disable enhanced touch when destroyed
        EnhancedTouchSupport.Disable();
    }
    
    // This method handles automatic hit tests from the PlaneFinderBehaviour
    public void OnAutomaticHitTest(HitTestResult result)
    {
        // You can put any logic here that should happen when a plane is detected
        // For now, this is just an empty method to satisfy the event listener
    }

    // We don't need the Update method anymore since we'll place trees directly from button clicks
    void Update()
    {
        // Empty - tree placement happens from button clicks, not from screen touches
    }


    // Method for Buttons to select a tree prefab by index AND place it immediately
    public void SelectAndPlaceTree(int index)
    {
        Debug.Log($"SelectAndPlaceTree called with index: {index}");

        if (treePrefabs == null)
        {
            Debug.LogError("treePrefabs array is null!");
            return;
        }

        Debug.Log($"treePrefabs length: {treePrefabs.Length}");

        if (index >= 0 && index < treePrefabs.Length)
        {
            currentTreeIndex = index;
            GameObject prefab = treePrefabs[index];

            if (prefab == null)
            {
                Debug.LogError($"Tree prefab at index {index} is null!");
                return;
            }

            selectedTreePrefab = prefab;
            Debug.Log($"Selected tree: {selectedTreePrefab.name} at index: {index}");

            // Find the currently detected plane position and place the tree there
            if (planeFinder != null)
            {
                // The tree will be placed at the position where the plane was last detected
                PlaceTreeAtCurrentPlanePosition();
            }
            else
            {
                Debug.LogError("PlaneFinder reference is null!");
            }
        }
        else
        {
            Debug.LogError($"Tree index {index} is out of range for array of length {treePrefabs.Length}");
        }
    }

    // Method for SwipeMenu to select a tree prefab by index (without placing)
    public void SelectTreePrefab(int index)
    {
        if (treePrefabs != null && index >= 0 && index < treePrefabs.Length)
        {
            currentTreeIndex = index;
            selectedTreePrefab = treePrefabs[index];
            Debug.Log($"Selected tree: {selectedTreePrefab.name}");
        }
    }
    
    // Method for TreeCarousel to select any tree prefab directly (without placing)
    public void SelectTreePrefab(GameObject treePrefab)
    {
        if (treePrefab != null)
        {
            selectedTreePrefab = treePrefab;
            Debug.Log($"Selected tree: {treePrefab.name}");
            
            // Find the index of this prefab in the array (optional)
            for (int i = 0; i < treePrefabs.Length; i++)
            {
                if (treePrefabs[i] == treePrefab)
                {
                    currentTreeIndex = i;
                    break;
                }
            }
        }
    }
    
    // Place the selected tree at the current plane position
    // This is called when a tree button is clicked in SwipeMenu
    public void PlaceTreeAtCurrentPlanePosition()
    {
        // Make sure we actually have a selected tree
        if (selectedTreePrefab == null)
        {
            Debug.LogWarning("Attempted to place a tree, but no tree is selected.");
            return;
        }
        
        // Debug which tree is being placed
        Debug.Log($"Attempting to place tree: {selectedTreePrefab.name} at index: {currentTreeIndex}");
        
        // Check if the treePrefabs array has the correct setup
        Debug.Log($"TreePrefabs array length: {(treePrefabs != null ? treePrefabs.Length.ToString() : "null")}");
        
        // Place the tree at the position of the DefaultPlaneIndicator
        GameObject planeIndicator = planeFinder.GetComponentInChildren<MeshRenderer>()?.gameObject;
        
        if (planeIndicator != null)
        {
            Debug.Log($"Found plane indicator at position: {planeIndicator.transform.position}");
            PlaceTreeAtPosition(planeIndicator.transform.position, planeIndicator.transform.rotation);
        }
        else
        {
            // If we can't find the plane indicator, use the last automatic hit test
            Debug.LogError("No DefaultPlaneIndicator found. Make sure the PlaneFinderBehaviour has a DefaultPlaneIndicator assigned.");
            
            // Attempt to find the plane finder behavior
            if (planeFinder == null)
                Debug.LogError("PlaneFinder reference is null!");
            else 
                Debug.Log($"PlaneFinder exists but couldn't find a MeshRenderer child for the plane indicator");
        }
    }
    
    // Method to place a tree at a specific position
    public void PlaceTreeAtPosition(Vector3 position, Quaternion rotation)
    {

        if (selectedTreePrefab != null)
        {
            // Apply height offset if needed
            position.y += placementHeight;

            try
            {
                // Instantiate the tree
                GameObject placedTree = Instantiate(selectedTreePrefab, position, rotation);

                if (placedTree != null)
                {
                    Debug.Log($"Successfully placed {selectedTreePrefab.name} at: {position}");

                    // Make sure the instantiated tree is visible and active
                    placedTree.SetActive(true);

                    //!NEW
                    // Add rotation and scaling functionality
                    ARObjectManipulation manipulation = placedTree.AddComponent<ARObjectManipulation>();
                    if (manipulation != null)
                    {
                        // Configure manipulation settings
                        manipulation.minScale = 0.5f;
                        manipulation.maxScale = 3.0f;
                        manipulation.scaleSpeed = 0.004f;
                        manipulation.rotationSpeed = 0.2f;
                        manipulation.enablePinchToScale = true;
                        manipulation.enableTwistToRotate = true;
                        manipulation.enableDragToRotate = true;
                        manipulation.smoothTransform = true;
                        Debug.Log("Added ARObjectManipulation to tree");
                    }
                    //!ENDNEW

                    // Add TreeSelector for deletion
                    TreeSelector selector = placedTree.GetComponent<TreeSelector>();
                    if (selector != null) {
                        // Configure at runtime if needed
                        selector.deletionManager = FindObjectOfType<TreeDeletionManager>();
                    }
                }
                else
                {
                    Debug.LogError($"Failed to instantiate {selectedTreePrefab.name} - Instantiate returned null");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error instantiating tree: {ex.Message}");
            }

            // Reset selection if configured to do so
            if (resetAfterPlacement)
            {
                Debug.Log("Resetting tree selection after placement");
                selectedTreePrefab = null;
            }
        }
        else
        {
            Debug.LogError("Cannot place tree: selectedTreePrefab is null");
        }
    }
    
    // Overload for just position (uses identity rotation)
    public void PlaceTreeAtPosition(Vector3 position)
    {
        PlaceTreeAtPosition(position, Quaternion.identity);
    }
}