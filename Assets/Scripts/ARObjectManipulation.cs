using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ARObjectManipulation : MonoBehaviour
{
    [Header("Scale Settings")]
    public bool enablePinchToScale = true;
    public float minScale = 0.5f;
    public float maxScale = 3.0f;
    public float scaleSpeed = 0.006f;
    
    [Header("Rotation Settings")]
    public bool enableTwistToRotate = true;
    public bool enableDragToRotate = true;
    public float rotationSpeed = 0.3f;
    
    [Header("Smooth Movement")]
    public bool smoothTransform = true;
    public float smoothSpeed = 10f;
    
    // Private variables
    private Vector3 initialScale;
    private float lastPinchDistance = 0f;
    private float lastTwistAngle = 0f;
    private Vector2 lastTouchPosition;
    private bool isManipulating = false;
    
    // Target values for smooth transitions
    private Vector3 targetScale;
    private Quaternion targetRotation;
    
    void Start()
    {
        // Store initial scale
        initialScale = transform.localScale;
        targetScale = initialScale;
        targetRotation = transform.rotation;
        
        // Enable Enhanced Touch
        EnhancedTouchSupport.Enable();
    }
    
    void Update()
    {
        int touchCount = Touch.activeTouches.Count;
        
        if (touchCount == 0)
        {
            isManipulating = false;
            return;
        }
        
        // Check if touching this object
        if (!IsTouchingThisObject())
        {
            return;
        }
        
        if (touchCount == 1 && enableDragToRotate)
        {
            // Single finger - rotate around Y axis
            HandleSingleFingerRotation();
        }
        else if (touchCount >= 2)
        {
            // Two fingers - pinch to scale and twist to rotate
            if (enablePinchToScale)
            {
                HandlePinchToScale();
            }
            
            if (enableTwistToRotate)
            {
                HandleTwistToRotate();
            }
        }
        
        // Apply smooth transitions
        if (smoothTransform)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * smoothSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
    }
    
    bool IsTouchingThisObject()
    {
        // Check if any active touch is over this object
        foreach (Touch touch in Touch.activeTouches)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.screenPosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject || 
                    hit.collider.transform.IsChildOf(transform))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    void HandlePinchToScale()
    {
        if (Touch.activeTouches.Count < 2) return;
        
        Touch touch1 = Touch.activeTouches[0];
        Touch touch2 = Touch.activeTouches[1];
        
        // Calculate distance between two fingers
        float currentPinchDistance = Vector2.Distance(touch1.screenPosition, touch2.screenPosition);
        
        // Initialize on first pinch
        if (!isManipulating || lastPinchDistance == 0f)
        {
            lastPinchDistance = currentPinchDistance;
            isManipulating = true;
            return;
        }
        
        // Calculate scale change
        float pinchDelta = currentPinchDistance - lastPinchDistance;
        float scaleFactor = 1f + (pinchDelta * scaleSpeed);
        
        // Apply scale
        Vector3 newScale = targetScale * scaleFactor;
        
        // Clamp scale
        float currentMaxScale = Mathf.Max(newScale.x, newScale.y, newScale.z);
        float currentMinScale = Mathf.Min(newScale.x, newScale.y, newScale.z);
        
        if (currentMaxScale <= maxScale && currentMinScale >= minScale)
        {
            targetScale = newScale;
        }
        
        if (!smoothTransform)
        {
            transform.localScale = targetScale;
        }
        
        lastPinchDistance = currentPinchDistance;
    }
    
    void HandleTwistToRotate()
    {
        if (Touch.activeTouches.Count < 2) return;
        
        Touch touch1 = Touch.activeTouches[0];
        Touch touch2 = Touch.activeTouches[1];
        
        // Calculate angle between two fingers
        Vector2 direction = touch2.screenPosition - touch1.screenPosition;
        float currentTwistAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Initialize on first twist
        if (!isManipulating || lastTwistAngle == 0f)
        {
            lastTwistAngle = currentTwistAngle;
            isManipulating = true;
            return;
        }
        
        // Calculate rotation change
        float angleDelta = Mathf.DeltaAngle(lastTwistAngle, currentTwistAngle);
        
        // Apply rotation around Y-axis (vertical)
        targetRotation *= Quaternion.Euler(0, -angleDelta * rotationSpeed, 0);
        
        if (!smoothTransform)
        {
            transform.rotation = targetRotation;
        }
        
        lastTwistAngle = currentTwistAngle;
    }
    
    void HandleSingleFingerRotation()
    {
        Touch touch = Touch.activeTouches[0];
        
        // Initialize on first touch
        if (!isManipulating)
        {
            lastTouchPosition = touch.screenPosition;
            isManipulating = true;
            return;
        }
        
        // Calculate drag delta
        Vector2 touchDelta = touch.screenPosition - lastTouchPosition;
        
        // Apply rotation based on horizontal drag
        float rotationAmount = touchDelta.x * rotationSpeed;
        targetRotation *= Quaternion.Euler(0, rotationAmount, 0);
        
        if (!smoothTransform)
        {
            transform.rotation = targetRotation;
        }
        
        lastTouchPosition = touch.screenPosition;
    }
    
    // Reset to original scale and rotation
    public void ResetTransform()
    {
        targetScale = initialScale;
        targetRotation = Quaternion.identity;
        
        if (!smoothTransform)
        {
            transform.localScale = initialScale;
            transform.rotation = Quaternion.identity;
        }
    }
    
    void OnDisable()
    {
        isManipulating = false;
    }
}
