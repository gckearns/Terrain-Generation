using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityBody : MonoBehaviour
{

    public bool useGravity;

    public GravityAttractor attractor;
    public Rigidbody rBody;

    Vector3 gravityUp
    {
        get
        {
            return (rBody.position - attractor.transform.position).normalized;
        }
    }

    void Start()
    {
        rBody.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (useGravity)
        {
            rBody.AddForce(gravityUp * attractor.gravity);

            rBody.MoveRotation(Quaternion.FromToRotation(rBody.transform.up, gravityUp) * rBody.rotation);
        }
    }
}
