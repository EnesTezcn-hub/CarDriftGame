using UnityEngine;
using UnityEngine.InputSystem;

// "Oyuncu ne yapmak istiyor?" — yalnizca veri uretir, Rigidbody'e dokunmaz.
public class CarInput : MonoBehaviour
{
    [Header("Mouse Drag Steering")]
    // Bu kadar piksel drag = tam steerInput (-1 ya da +1)
    public float maxDragDistance = 200f;

    public float ThrottleInput { get; private set; }
    public float SteerInput { get; private set; }

    Vector2 dragStartPosition;
    bool isDragging;

    void Update()
    {
        ReadThrottleInput();
        ReadSteerInput();
    }

    void ReadThrottleInput()
    {
        float throttle = 0f;

        if (Keyboard.current.wKey.isPressed)
            throttle = 1f;

        if (Keyboard.current.sKey.isPressed)
            throttle = -1f;

        ThrottleInput = throttle;
    }

    void ReadSteerInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            dragStartPosition = Mouse.current.position.ReadValue();
            isDragging = true;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 currentPosition = Mouse.current.position.ReadValue();
            float dragX = currentPosition.x - dragStartPosition.x;

            SteerInput = Mathf.Clamp(dragX / maxDragDistance, -1f, 1f);
            return;
        }

        // Mouse basili degilken klavye ile de yonlendirilebilir
        float steer = 0f;

        if (Keyboard.current.aKey.isPressed)
            steer = -1f;

        if (Keyboard.current.dKey.isPressed)
            steer = 1f;

        SteerInput = steer;
    }
}
