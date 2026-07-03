
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class SwipeMenu : MonoBehaviour
{
    public GameObject scrollbar;
    public TreePlacer treePlacer; // Reference to TreePlacer script
    
    [Header("Selected Tree Highlight")]
    //public Color selectedColor = Color.green;  // Optional: highlight color for selected tree
    
    float scroll_pos = 0;
    float[] pos;
    private int currentSelectedIndex = -1;  // Currently selected tree index

    void Start()
    {
        // Enable enhanced touch support
        EnhancedTouchSupport.Enable(); 
        
        // Set up button click listeners for all tree items
        SetupTreeButtons();
    }
    
    // Add button components and click handlers to all tree items
    private void SetupTreeButtons()
    {
        if (treePlacer == null) 
        {
            Debug.LogError("TreePlacer reference is missing!");
            return;
        }
        
        // Add button components to all child objects if needed
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Button button = child.GetComponent<Button>();
            
            // If no button component exists, add one
            if (button == null)
            {
                button = child.gameObject.AddComponent<Button>();
                
                // Set ColorBlock to preserve image color when highlighted
                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = Color.white;
                colors.pressedColor = new Color(190f/255f, 190f/255f, 190f/255f);
                colors.selectedColor = Color.white;
                button.colors = colors;
            }
            
            // Remove any existing listeners and add new one
            button.onClick.RemoveAllListeners();
            
            // Capture the index for the lambda expression
            int index = i;
            button.onClick.AddListener(() => OnTreeButtonClicked(index));
        }
    }
    
    // Handle tree button clicks
    private void OnTreeButtonClicked(int index)
    {
        Debug.Log($"Tree button {index} clicked");

        if (treePlacer != null)
        {
            /*
            // Select and immediately place this tree
            treePlacer.SelectAndPlaceTree(index);
            */

            // First ensure this tree is selected (in case user didn't scroll to it)
            treePlacer.SelectTreePrefab(index);
        
            // Then place the selected tree
            treePlacer.PlaceTreeAtCurrentPlanePosition();
        }
    }
    
    void OnDestroy()
    {
        // Disable enhanced touch support when destroyed
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        pos = new float[transform.childCount];
        float distance = 1f / (pos.Length - 1f);

        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }

        // Check if mouse button is pressed using new Input System
        bool isPressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool touchActive = Touchscreen.current != null && UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0;

        if (isPressed || touchActive)
        {
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
        }
        else
        {
            for (int i = 0; i < pos.Length; i++)
            {
                if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
                {
                    // Snap to position
                    scrollbar.GetComponent<Scrollbar>().value =
                    Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
                }
            }
        }
        for (int i = 0; i < pos.Length; i++) 
        {
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2)) 
            {
                // If this is a new selection, update the TreePlacer
                if (currentSelectedIndex != i && treePlacer != null)
                {
                    currentSelectedIndex = i;
                    treePlacer.SelectTreePrefab(i);
                }
                
                // выбранный элемент "масштабируется" к (1.2,1.2)
                transform.GetChild(i).localScale =
                    Vector3.Lerp(transform.GetChild(i).localScale, new Vector3(1.2f, 1.2f, 1f), 0.05f * Time.deltaTime * 60f);

                // Highlight the selected tree (optional)
                /*
                Image img = transform.GetChild(i).GetComponent<Image>();
                if (img != null)
                {
                    img.color = Color.Lerp(img.color, selectedColor, 0.05f * Time.deltaTime * 30f);
                }
                */
                // все остальные элементы
                for (int a = 0; a < pos.Length; a++) 
                {
                    if (a != i) 
                    {
                        // уменьшаем их масштаб к (0.7, 0.7)
                        transform.GetChild(a).localScale =
                            Vector3.Lerp(transform.GetChild(a).localScale, new Vector3(0.7f, 0.7f, 1f), 0.05f * Time.deltaTime * 60f);
                            
                        // Reset color of unselected items (optional)
                        Image img2 = transform.GetChild(a).GetComponent<Image>();
                        if (img2 != null)
                        {
                            img2.color = Color.Lerp(img2.color, Color.white, 0.05f * Time.deltaTime * 30f);
                        }
                    }
                }
            }
        }
    }
}
