using System;
using UnityEngine;
// "Bu nasil bir araba?" — aracin fiziksel karakterini tutar.
// CarPhysics kodu degismeden farkli araba karakterleri buradan tanimlanabilir.
[Serializable]
public class CarConfig
{
    [Header("Movement")]
    public float acceleration = 8000f;

    [Header("Steering")]
    public float turnSpeed = 100f;

    [Header("Lateral Grip")]
    public float lateralGrip = 8f;
    public float lateralForceHeight = 0.5f;

    [Header("Drift Tuning")]
    // DriftAmount = 1 iken hedeflenen grip. Drift'te araba daha cok yana kaysin diye
    // baseline lateralGrip'ten dusuk. (Asama 6)
    public float driftGrip = 4f;

    // DriftAmount = 1 iken hedeflenen steering power. Drift'te araba daha keskin
    // donebilsin diye baseline turnSpeed'ten yuksek. KUCUK artisla basla (spin riski).
    // (Asama 10)
    public float driftTurnSpeed = 3300f;

    // DriftAmount = 1 iken hedeflenen angular control. Baseline angularDrag'ten dusuk:
    // drift sirasinda yaw hareketine daha cok izin verilir, araba aciyi tutar,
    // donusten sonra hemen toparlamaz. (Asama 11)
    public float driftAngularControl = 2f;

    [Header("Drag")]
    public float forwardDrag = 2f;
    public float angularDrag = 5f;
}
