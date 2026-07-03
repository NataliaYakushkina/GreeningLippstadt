using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendShapeLoop : MonoBehaviour
{
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private Mesh skinnedMesh;
    private int blendShapeCount;
    private int playIndex = 0;
    private int lastIndex = -1; // Track last shape to reduce redundant calls
    private int direction = 1;  // Direction of animation (1 = forward, -1 = backward)

    // Animation settings
    [Range(5f, 60f)]
    public float blenderFPS = 15f; // Reduced to 15 FPS for better performance
    [Range(0.1f, 3.0f)]
    public float playbackSpeed = 1.0f; // Speed multiplier (1.0 = normal speed)
    public bool pingPongAnimation = true; // Whether to reverse direction at the end
    
    // Performance settings
    public bool reduceMemoryUsage = true; // Enable memory optimization
    [Range(1, 4)]
    public int skipShapesForPerformance = 1; // Skip shapes for better performance
    
    // Internal variables
    private float animTimer = 0f;
    private bool hasInitialized = false;

    void Start()
    {
        InitializeBlendShapes();
    }
    
    void InitializeBlendShapes()
    {
        if (hasInitialized) return;
        
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null)
        {
            Debug.LogError("No SkinnedMeshRenderer found on " + gameObject.name);
            enabled = false;
            return;
        }
        
        skinnedMesh = skinnedMeshRenderer.sharedMesh;
        if (skinnedMesh == null)
        {
            Debug.LogError("No mesh found on SkinnedMeshRenderer");
            enabled = false;
            return;
        }
        
        blendShapeCount = skinnedMesh.blendShapeCount;
        if (blendShapeCount <= 0)
        {
            Debug.LogError("No blend shapes found in the mesh");
            enabled = false;
            return;
        }
        
        // Pre-initialize all blend shapes to 0 (avoids garbage collection later)
        if (reduceMemoryUsage)
        {
            for (int i = 0; i < blendShapeCount; i++)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(i, 0f);
            }
        }
        
        // Activate first shape
        if (blendShapeCount > 0)
        {
            skinnedMeshRenderer.SetBlendShapeWeight(0, 100f);
        }
        
        hasInitialized = true;
    }

    void Update()
    {
        // Make sure we're initialized
        if (!hasInitialized)
        {
            InitializeBlendShapes();
            return;
        }
        
        // Calculate time for one frame at Blender's FPS
        float timePerBlenderFrame = 1f / blenderFPS;
        
        // Update timer with Unity's deltaTime
        animTimer += Time.deltaTime * playbackSpeed;
        
        // Check if we've passed the time for a Blender frame
        if (animTimer >= timePerBlenderFrame)
        {
            // How many frames to advance (optionally skip frames for performance)
            int framesToAdvance = Mathf.Max(1, skipShapesForPerformance);
            
            // Turn off the current blend shape (only if needed)
            if (lastIndex != playIndex && lastIndex >= 0 && lastIndex < blendShapeCount)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(lastIndex, 0f);
            }
            
            // Remember the current index
            lastIndex = playIndex;
            
            // Advance to next shape based on direction
            if (pingPongAnimation)
            {
                // Move in current direction with step size
                playIndex += direction * framesToAdvance;
                
                // Check if we need to reverse direction
                if (playIndex >= blendShapeCount)
                {
                    // Reached the end, start moving backward
                    playIndex = blendShapeCount - 2;
                    direction = -1;
                }
                else if (playIndex < 0)
                {
                    // Reached the beginning, start moving forward
                    playIndex = 1;
                    direction = 1;
                }
            }
            else
            {
                // Original looping behavior
                playIndex = (playIndex + framesToAdvance) % blendShapeCount;
            }
            
            // Turn on the new shape
            skinnedMeshRenderer.SetBlendShapeWeight(playIndex, 100f);
            
            // Reset timer, keeping any leftover time
            animTimer %= timePerBlenderFrame;
        }
    }
    
    void OnDisable()
    {
        // Clean up when disabled
        if (hasInitialized && skinnedMeshRenderer != null)
        {
            for (int i = 0; i < blendShapeCount; i++)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(i, 0f);
            }
        }
    }
}