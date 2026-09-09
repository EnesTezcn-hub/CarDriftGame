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
        // Sag ust kosede kompakt debug listesi.
        const float w = 300f;
        const float h = 22f;
        float x = Screen.width - w - 10f;
        float y = 10f;

        GUIStyle style = new GUIStyle
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleRight
        };

        void Line(string text, Color color)
        {
            style.normal.textColor = color;
            GUI.Label(new Rect(x, y, w, h), text, style);
            y += h;
        }

        Line($"Throttle Input    {carInput.ThrottleInput:F2}   (lvl {carInput.ThrottleLevel:F2})", Color.yellow);
        Line($"Raw Scroll        {carInput.DebugRawScroll:F3}", Color.yellow);
        Line($"Steer Input       {carInput.SteerInput:F3}", Color.yellow);
        Line($"Forward Velocity  {carPhysics.ForwardVelocity:F3}", Color.yellow);
        Line($"Lateral Velocity  {carPhysics.LateralVelocity:F3}", Color.yellow);
        Line($"Speed             {carPhysics.Speed:F3}", Color.yellow);
        Line($"Yaw Velocity      {carPhysics.YawVelocity:F3}", Color.yellow);
        Line($"Slip Angle        {carPhysics.SlipAngle:F1}", Color.yellow);
        Line($"Lateral Force     {carPhysics.DebugLateralForce:F3}", Color.yellow);

        y += 6f;
        Line($"Drift Amount      {driftEvaluator.DriftAmount:F3}", Color.cyan);
        Line($"steer {driftEvaluator.DebugSteerFactor:F2}  lateral {driftEvaluator.DebugLateralFactor:F2}  speed {driftEvaluator.DebugSpeedFactor:F2}", Color.cyan);

        y += 6f;
        Line($"Current Grip      {carBehaviour.CurrentGrip:F3}", Color.white);
        Line($"Current Steering  {carBehaviour.CurrentSteeringPower:F0}", Color.white);
        Line($"Current AngularCtl {carBehaviour.CurrentAngularControl:F3}", Color.white);
    }
}
