using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorFieldEffector : MonoBehaviour
{
    public VectorFieldGrid field;
    public bool isAttracting = false;
    [Range(0.1f, 10.0f)]
    public float multiplier = 1.0f;
    [Range(0.1f, 1.0f)]
    public float dampingRot = 0.1f;

    void FixedUpdate()
    {
        Vector3 closestForce = field.GetClosestForce(transform.position);

        if (isAttracting || field.isOutside(transform.position)) {
            closestForce = closestForce - transform.position;
        }

        closestForce *= multiplier;

        GetComponent<Rigidbody>().AddForce(closestForce);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(closestForce), dampingRot);
    }
}
