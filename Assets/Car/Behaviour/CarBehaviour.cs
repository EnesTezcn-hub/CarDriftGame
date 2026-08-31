using UnityEngine;

// "Oyuncunun istegi ve aracin mevcut durumuna gore araba nasil davranmali?"
// Bu asamada henuz drift hesaplamiyor; config degerlerini oldugu gibi disari verir.
// Fizik uygulamaz: rb.AddForce / rb.AddTorque burada olmamali.
public class CarBehaviour : MonoBehaviour
{
    public CarConfig config = new CarConfig();

    public float CurrentAcceleration { get; private set; }
    public float CurrentSteeringPower { get; private set; }
    public float CurrentGrip { get; private set; }
    public float CurrentLateralForceHeight { get; private set; }
    public float CurrentForwardDrag { get; private set; }
    public float CurrentAngularControl { get; private set; }

    public void Evaluate(float throttleInput, float steerInput, float driftAmount)
    {
        CurrentAcceleration = config.acceleration;
        CurrentSteeringPower = config.turnSpeed;

        // Asama 6: ILK ve SIMDILIK TEK drift parametresi.
        // driftAmount 0 -> lateralGrip, driftAmount 1 -> driftGrip.
        CurrentGrip = Mathf.Lerp(config.lateralGrip, config.driftGrip, driftAmount);

        CurrentLateralForceHeight = config.lateralForceHeight;
        CurrentForwardDrag = config.forwardDrag;
        CurrentAngularControl = config.angularDrag;
    }
}
