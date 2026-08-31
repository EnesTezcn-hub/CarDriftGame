using UnityEngine;

// Sistemleri birbirine baglayan orkestrator.
// Kendisi hicbir sistemin detayini ustlenmez: input okumaz, fizik uygulamaz.
[RequireComponent(typeof(CarInput), typeof(CarBehaviour), typeof(CarPhysics))]
[RequireComponent(typeof(DriftEvaluator))]
public class CarController : MonoBehaviour
{
    public CarInput carInput;
    public CarBehaviour carBehaviour;
    public CarPhysics carPhysics;
    public DriftEvaluator driftEvaluator;

    void Awake()
    {
        if (carInput == null)
            carInput = GetComponent<CarInput>();

        if (carBehaviour == null)
            carBehaviour = GetComponent<CarBehaviour>();

        if (carPhysics == null)
            carPhysics = GetComponent<CarPhysics>();

        if (driftEvaluator == null)
            driftEvaluator = GetComponent<DriftEvaluator>();
    }

    void FixedUpdate()
    {
        // 1. Once mevcut fiziksel durumu olc.
        carPhysics.UpdateVehicleState();

        // 2. Intent + fiziksel durum -> DriftAmount. (Henuz Behaviour'a baglanmadi;
        //    Asama 5: sadece olcum. Araba davranisi degismemeli.)
        driftEvaluator.Evaluate(carInput, carPhysics);

        // 3. Behaviour karar verir. (Asama 6: driftAmount artik grip'i etkiliyor.)
        carBehaviour.Evaluate(carInput.ThrottleInput, carInput.SteerInput, driftEvaluator.DriftAmount);

        carPhysics.ApplyMovement(carInput.ThrottleInput, carBehaviour.CurrentAcceleration);
        carPhysics.ApplySteering(carInput.SteerInput, carBehaviour.CurrentSteeringPower);

        carPhysics.ApplyForwardDrag(carBehaviour.CurrentForwardDrag);
        carPhysics.ApplyLateralGrip(carBehaviour.CurrentGrip, carBehaviour.CurrentLateralForceHeight);
        carPhysics.ApplyAngularDrag(carBehaviour.CurrentAngularControl);
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();

        style.fontSize = 24;
        style.normal.textColor = Color.yellow;

        // Drift sisteminin ilk doneminde bu degerler kritik.
        GUI.Label(
            new Rect(250, 20, 500, 40),
            $"Steer Input:       {carInput.SteerInput:F3}",
            style
        );

        GUI.Label(
            new Rect(250, 50, 500, 40),
            $"Forward Velocity:  {carPhysics.ForwardVelocity:F3}",
            style
        );

        GUI.Label(
            new Rect(250, 80, 500, 40),
            $"Lateral Velocity:  {carPhysics.LateralVelocity:F3}",
            style
        );

        GUI.Label(
            new Rect(250, 110, 500, 40),
            $"Speed:             {carPhysics.Speed:F3}",
            style
        );

        GUI.Label(
            new Rect(250, 140, 500, 40),
            $"Yaw Velocity:      {carPhysics.YawVelocity:F3}",
            style
        );

        GUI.Label(
            new Rect(250, 170, 500, 40),
            $"Lateral Force:     {carPhysics.DebugLateralForce:F3}",
            style
        );

        // Asama 5: DriftAmount sadece izleniyor, Behaviour'a bagli degil.
        style.normal.textColor = Color.cyan;

        GUI.Label(
            new Rect(250, 210, 500, 40),
            $"Drift Amount:      {driftEvaluator.DriftAmount:F3}",
            style
        );

        GUI.Label(
            new Rect(250, 240, 600, 40),
            $"  steer {driftEvaluator.DebugSteerFactor:F2}  " +
            $"lateral {driftEvaluator.DebugLateralFactor:F2}  " +
            $"speed {driftEvaluator.DebugSpeedFactor:F2}",
            style
        );

        GUI.Label(
            new Rect(250, 270, 500, 40),
            $"Current Grip:      {carBehaviour.CurrentGrip:F3}",
            style
        );

        GUI.Label(
            new Rect(250, 300, 500, 40),
            $"Current Steering:  {carBehaviour.CurrentSteeringPower:F0}",
            style
        );
    }
}
