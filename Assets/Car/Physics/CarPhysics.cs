using UnityEngine;

// "Behaviour'un istedigini Rigidbody uzerinde uygula."
// Mouse, drift durumu ya da hangi araba secildigi gibi seyleri bilmez;
// yalnizca kendisine verilen degerleri uygular.
public class CarPhysics : MonoBehaviour
{
    public Rigidbody rb;

    public float DebugLateralVelocity { get; private set; }
    public float DebugLateralForce { get; private set; }

    // Vehicle state — "Rigidbody su anda fiziksel olarak ne yapiyor?"
    // DriftEvaluator bu degerleri okur; kendi Rigidbody hesabini yapmaz.
    public float ForwardVelocity { get; private set; }
    public float LateralVelocity { get; private set; }
    public float Speed { get; private set; }
    public float YawVelocity { get; private set; }

    // Arabanin baktigi yon ile gercekten hareket ettigi yon arasindaki fark (derece).
    // ~0-3 duz surus, 5-10 hafif kayma, 10-20 drift, 20+ guclu drift / spin riski.
    public float SlipAngle { get; private set; }

    // Rigidbody verilerini okuyup arac durumunu gunceller.
    // CarController.FixedUpdate en basinda cagrilmali.
    public void UpdateVehicleState()
    {
        Vector3 localVelocity =
            transform.InverseTransformDirection(rb.linearVelocity);

        ForwardVelocity = localVelocity.z;
        LateralVelocity = localVelocity.x;
        Speed = rb.linearVelocity.magnitude;

        Vector3 localAngularVelocity =
            transform.InverseTransformDirection(rb.angularVelocity);

        YawVelocity = localAngularVelocity.y;

        // Isaretli: negatif = sola kayiyor, pozitif = saga kayiyor.
        // Cok yavasken slip angle anlamsiz (yon gurultusu) -> 0 kabul et.
        SlipAngle = Speed < 0.5f
            ? 0f
            : Mathf.Atan2(LateralVelocity, Mathf.Abs(ForwardVelocity)) * Mathf.Rad2Deg;
    }

    public void ApplyMovement(float throttleInput, float acceleration)
    {
        rb.AddForce(
            transform.forward * throttleInput * acceleration,
            ForceMode.Force
        );
    }

    public void ApplySteering(float steerInput, float steeringPower)
    {
        rb.AddTorque(
            transform.up * steerInput * steeringPower,
            ForceMode.Force
        );
    }

    public void ApplyLateralGrip(float grip, float forceHeight)
    {
        Vector3 localVelocity =
            transform.InverseTransformDirection(rb.linearVelocity);

        float lateralVelocity = localVelocity.x;

        Vector3 lateralForce =
            -transform.right *
            lateralVelocity *
            grip;

        DebugLateralVelocity = lateralVelocity;
        DebugLateralForce = lateralForce.magnitude;

        Vector3 forcePoint =
            rb.worldCenterOfMass +
            transform.up * forceHeight;

        rb.AddForceAtPosition(
            lateralForce,
            forcePoint,
            ForceMode.Acceleration
        );
    }

    public void ApplyForwardDrag(float dragCoefficient)
    {
        Vector3 localVelocity =
            transform.InverseTransformDirection(rb.linearVelocity);

        float forwardVelocity = localVelocity.z;

        Vector3 dragForce =
            -transform.forward *
            forwardVelocity *
            dragCoefficient;

        rb.AddForce(
            dragForce,
            ForceMode.Acceleration
        );
    }

    public void ApplyAngularDrag(float angularControl)
    {
        Vector3 localAngularVelocity =
            transform.InverseTransformDirection(rb.angularVelocity);

        float yawVelocity = localAngularVelocity.y;

        rb.AddTorque(
            -transform.up * yawVelocity * angularControl,
            ForceMode.Acceleration
        );
    }
}
