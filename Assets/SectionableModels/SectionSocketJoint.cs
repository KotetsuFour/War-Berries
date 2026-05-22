using UnityEngine;

public class SectionSocketJoint : SectionHingeJoint
{
    [Range(0f, 1f)] public float rotation2OrY;
    [Range(0f, 1f)] public float rotation3OrZ;

    void Awake()
    {
        rotation1 = 0.5f;
        rotation2OrY = 0.5f;
        rotation3OrZ = 0.5f;
    }

    void Update()
    {
        transform.localRotation = Quaternion.Euler(new Vector3(rotation1 - 0.5f,
            rotation2OrY - 0.5f,
            rotation3OrZ - 0.5f) * FULL_ROTATION);
    }
}
