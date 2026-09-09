using UnityEngine;
using UnityEngine.InputSystem;

// "Oyuncu ne yapmak istiyor?" — yalnizca veri uretir, Rigidbody'e dokunmaz.
public class CarInput : MonoBehaviour
{
    [Header("Mouse Drag Steering")]
    // Bu kadar piksel drag = tam steerInput (-1 ya da +1)
    public float maxDragDistance = 200f;

    [Header("Scroll Throttle")]
    // Baslangic gaz seviyesi (0..1).
    public float defaultThrottleLevel = 0f;
    // Scroll ham degeri bu carpanla ThrottleLevel'a eklenir (trackpad'e gore ayarla).
    public float scrollSensitivity = 0.05f;
    // Ok tuslariyla ayarlarken her basista degisim.
    public float keyboardThrottleStep = 0.1f;

    public float ThrottleInput { get; private set; }
    public float SteerInput { get; private set; }

    // Scroll / ok tuslari ile ayarlanan gaz seviyesi (0..1). Dogrudan uygulanir.
    public float ThrottleLevel { get; private set; }

    // Debug: son okunan ham scroll degeri (trackpad sorununu teshis icin).
    public float DebugRawScroll { get; private set; }

    Vector2 dragStartPosition;
    bool isDragging;

    void Awake()
    {
        ThrottleLevel = Mathf.Clamp01(defaultThrottleLevel);
    }

    void Update()
    {
        ReadThrottleInput();
        ReadSteerInput();
    }

    void ReadThrottleInput()
    {
        // Trackpad iki parmak swipe / mouse tekerlegi -> gaz seviyesini ayarla.
        // Ham degeri hassasiyetle olcekleyip biriktiriyoruz (Mac'te kucuk, mouse'ta buyuk gelir).
        float scrollY = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
        DebugRawScroll = scrollY;

        if (Mathf.Abs(scrollY) > 0.0001f)
        {
            ThrottleLevel = Mathf.Clamp01(
                ThrottleLevel + scrollY * scrollSensitivity
            );
        }

        // Klavye: yukari/asagi ok ile gaz seviyesini ayarla.
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            ThrottleLevel = Mathf.Clamp01(ThrottleLevel + keyboardThrottleStep);

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            ThrottleLevel = Mathf.Clamp01(ThrottleLevel - keyboardThrottleStep);

        // Gaz dogrudan uygulanir; coast icin seviyeyi 0'a cek.
        float throttle = ThrottleLevel;

        // Klavye override (dev / geri vites).
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
