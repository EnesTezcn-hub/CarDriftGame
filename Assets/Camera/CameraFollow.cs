using UnityEngine;

// Kamerayi arabaya takmadan (parent yapmadan) takip ettirir.
// Baslangictaki kamera-araba farkini kilitler -> "oyun basladigindaki goruntu" korunur.
// CarInput / CarPhysics / CarBehaviour zincirine dokunmaz.
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    // Bos birakilirsa sahnedeki CarController otomatik bulunur.
    public Transform target;

    [Header("Follow")]
    // 0 = aninda yapisir, buyudukce daha yumusak/gecikmeli.
    public float positionSmoothTime = 0.15f;

    [Header("Rotation")]
    // Kapali: baslangic acisi sabit kalir (stabil, arcade).
    // Acik: kamera yumusakca arabaya bakar.
    public bool lookAtTarget = false;
    public float lookSmoothSpeed = 5f;

    Vector3 offset;
    Vector3 followVelocity;

    void Start()
    {
        if (target == null)
        {
            CarController car = FindObjectOfType<CarController>();
            if (car != null)
                target = car.transform;
        }

        if (target == null)
        {
            Debug.LogWarning("CameraFollow: target yok, takip devre disi.");
            enabled = false;
            return;
        }

        // Sahnedeki mevcut kamera-araba farki = referans goruntu.
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        // Araba o kare hareketini bitirdikten SONRA kamerayi yerlestir.
        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            positionSmoothTime
        );

        if (lookAtTarget)
        {
            Vector3 toTarget = target.position - transform.position;
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(toTarget);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    desiredRotation,
                    lookSmoothSpeed * Time.deltaTime
                );
            }
        }
    }
}
