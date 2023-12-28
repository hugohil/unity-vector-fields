using UnityEngine;

public class ClothFieldInteraction : MonoBehaviour {
    public VectorFieldGrid vectorField;
    public float multiplier = 1.0f;

    void FixedUpdate() {
        Vector3 fieldForce = vectorField.GetClosestForce(transform.position);
        GetComponent<Cloth>().externalAcceleration = fieldForce * multiplier;
    }
}
