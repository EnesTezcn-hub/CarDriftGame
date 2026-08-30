using UnityEngine;

// "Behaviour'un istedigini Rigidbody uzerinde uygula."
// Mouse, drift durumu ya da hangi araba secildigi gibi seyleri bilmez;
// yalnizca kendisine verilen degerleri uygular.
public class CarPhysics : MonoBehaviour
{
    public Rigidbody rb;

    public float DebugLateralVelocity { get; private set; }
    public float DebugLateralForce { get; private set; }

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
