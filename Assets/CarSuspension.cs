using UnityEngine;
using UnityEngine.InputSystem;

public class CarSuspension : MonoBehaviour
{
    public Rigidbody rb;

    public Transform frontLeft;
    public Transform frontRight;
    public Transform rearLeft;
    public Transform rearRight;

    public float suspensionLength = 0.5f;
    public float springStrength = 20000f;
    public float damperStrength = 2000f;

    private float flCompression;
    private float frCompression;
    private float rlCompression;
    private float rrCompression;

    void FixedUpdate()
    {
        //4 suspension points için ApplySuspension çağır
        ApplySuspension(frontLeft);
        ApplySuspension(frontRight);
        ApplySuspension(rearLeft);
        ApplySuspension(rearRight);
        // SADECE TEST İÇİN

    if (Keyboard.current != null && Keyboard.current.qKey.isPressed)

    {

        rb.AddForceAtPosition(

            -transform.up * 3000f,

            frontLeft.position,

            ForceMode.Force

        );

    }

    }



    void OnGUI()
    {
        GUIStyle style = new GUIStyle();

        style.fontSize = 24;
        style.normal.textColor = Color.white;

        GUI.Label(
            new Rect(20, 20, 400, 40),
            $"FL: {flCompression:F3}",
            style
        );

        GUI.Label(
            new Rect(20, 50, 400, 40),
            $"FR: {frCompression:F3}",
            style
        );

        GUI.Label(
            new Rect(20, 80, 400, 40),
            $"RL: {rlCompression:F3}",
            style
        );

        GUI.Label(
            new Rect(20, 110, 400, 40),
            $"RR: {rrCompression:F3}",
            style
        );
    }

// tek bir point için yapıyor
    void ApplySuspension(Transform suspensionPoint)
    {
        Debug.DrawRay(
            suspensionPoint.position,
            -suspensionPoint.up * suspensionLength,
            Color.red
        );

        
        RaycastHit hit; // RaycastHit, raycast ile çarpışan objenin bilgilerini tutar

        if (Physics.Raycast(
            suspensionPoint.position,
            -suspensionPoint.up, //yönü aşa
            out hit, // hit değişkenine çarpışma bilgilerini atar
            suspensionLength)) // en uzun mesafe rayin uzunluğu
        {
            // Raycast ile çarpışma oldu, yani suspension sıkıştı 
            float compression =
                suspensionLength - hit.distance;

            // Hangi suspension olduğunu bul ve ona göre compression değerini kaydet
            //DEBUG için GUI'de göstermek için
            if (suspensionPoint == frontLeft)
            {
                flCompression = compression;
            }
            else if (suspensionPoint == frontRight)
            {
                frCompression = compression;
            }
            else if (suspensionPoint == rearLeft)
            {
                rlCompression = compression;
            }
            else if (suspensionPoint == rearRight)
            {
                rrCompression = compression;
            }
            
            // F arabayı yukarı iten kuvvet
            float springForce =
                //x.          k.
                compression * springStrength;

            // her noktanın(her tekerliğin) velocity'sini alıyoruz çünkü araba roll yapabiliyor
            Vector3 pointVelocity =
                rb.GetPointVelocity(suspensionPoint.position);

            
            //bu hız suspansiyonun sıkışma hızını gösterir.
            float verticalVelocity =
                Vector3.Dot(
                    pointVelocity,
                    suspensionPoint.up
                );

            //bu hızı kullanarak süspansiyonun sönümleme kuvvetini hesaplıyoruz

            float damperForce =
                verticalVelocity * damperStrength;
            // toplam kuvvet = yay kuvveti - sönümleme kuvveti
            float totalForce =
                springForce - damperForce;

            rb.AddForceAtPosition(
                suspensionPoint.up * totalForce,
                suspensionPoint.position
            );
        }
    }
}