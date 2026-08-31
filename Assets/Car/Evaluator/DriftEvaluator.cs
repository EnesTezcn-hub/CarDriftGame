using UnityEngine;

// "Oyuncunun istegi + aracin fiziksel durumu birlikte dusunuldugunde
//  araba su anda ne kadar drift durumunda?"
//
// Sorumlulugu SADECE olcmek/yorumlamak: CarInput ve CarPhysics verilerini
// okur, tek bir sureklidegermek olan DriftAmount uretir.
//
// ONEMLI: Burada hicbir rb.AddForce / rb.AddTorque cagrisi OLMAZ.
// Fizik uygulamaz, Behaviour'a da dokunmaz.
public class DriftEvaluator : MonoBehaviour
{
    [Header("Speed Factor (m/s)")]
    // Bu hizin altinda drift olarak degerlendirilmez.
    public float minDriftSpeed = 3f;
    // Bu hizda ve ustunde speed faktoru tam (1).
    public float fullDriftSpeed = 10f;

    [Header("Lateral Factor (m/s)")]
    // Bu yanal hizda lateral faktor tam (1). Aliminda: baseline'da kayma
    // olmadigi icin bu deger ancak grip dustukten (Asama 6) sonra anlam kazanir.
    public float lateralVelocityForFullDrift = 6f;

    // 0.0 = drift yok, 1.0 = maksimum drift
    public float DriftAmount { get; private set; }

    // Debug icin ara faktorler
    public float DebugSteerFactor { get; private set; }
    public float DebugLateralFactor { get; private set; }
    public float DebugSpeedFactor { get; private set; }

    // Asama 4: olabildigince basit formul. Slip angle YOK.
    // Nihai formul degil; sadece veri akisini test etmek icin.
    public void Evaluate(CarInput input, CarPhysics physics)
    {
        // 1. Steer intensity — oyuncu ne kadar sert yon degistirmek istiyor
        float steerFactor = Mathf.Abs(input.SteerInput);

        // 2. Lateral intensity — araba gercekten ne kadar yana hareket ediyor
        float lateralFactor = Mathf.Clamp01(
            Mathf.Abs(physics.LateralVelocity) / lateralVelocityForFullDrift
        );

        // 3. Speed factor — cok yavassa drift degil
        float speedFactor = Mathf.Clamp01(
            (physics.Speed - minDriftSpeed) / (fullDriftSpeed - minDriftSpeed)
        );

        DebugSteerFactor = steerFactor;
        DebugLateralFactor = lateralFactor;
        DebugSpeedFactor = speedFactor;

        DriftAmount = Mathf.Clamp01(steerFactor * lateralFactor * speedFactor);
    }
}
