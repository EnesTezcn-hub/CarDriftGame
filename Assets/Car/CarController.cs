using UnityEngine;

// Sistemleri birbirine baglayan orkestrator.
// Kendisi hicbir sistemin detayini ustlenmez: input okumaz, fizik uygulamaz.
[RequireComponent(typeof(CarInput), typeof(CarBehaviour), typeof(CarPhysics))]
public class CarController : MonoBehaviour
{
    public CarInput carInput;
    public CarBehaviour carBehaviour;
    public CarPhysics carPhysics;

    void Awake()
    {
        if (carInput == null)
            carInput = GetComponent<CarInput>();

        if (carBehaviour == null)
            carBehaviour = GetComponent<CarBehaviour>();

        if (carPhysics == null)
            carPhysics = GetComponent<CarPhysics>();
    }

    void FixedUpdate()
    {
        carBehaviour.Evaluate(carInput.ThrottleInput, carInput.SteerInput);

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

        GUI.Label(
            new Rect(250, 20, 500, 40),
            $"Lateral Velocity: {carPhysics.DebugLateralVelocity:F3}",
            style
        );

        GUI.Label(
            new Rect(250, 50, 500, 40),
            $"Lateral Force: {carPhysics.DebugLateralForce:F3}",
            style
        );

        GUI.Label(
            new Rect(250, 80, 500, 40),
            $"Steer Input: {carInput.SteerInput:F3}",
            style
        );
    }
}
