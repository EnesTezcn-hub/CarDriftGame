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

    [Header("Drag")]
    public float forwardDrag = 2f;
    public float angularDrag = 5f;
}
