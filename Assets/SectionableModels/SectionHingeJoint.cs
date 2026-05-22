using UnityEngine;

public class SectionHingeJoint : SectionJoint
{
    public const int FULL_ROTATION = 360;

    [Range(0f, 1f)] public float rotation1;
    [SerializeField] private Dimenion dimensionUsed;

    void Awake()
    {
        rotation1 = 0.5f;
    }

    void Update()
    {
        if (dimensionUsed == Dimenion.X)
        {
            transform.localRotation = Quaternion.Euler(new Vector3(rotation1 - 0.5f, 0, 0) * FULL_ROTATION);
        }
        else if (dimensionUsed == Dimenion.Y)
        {
            transform.localRotation = Quaternion.Euler(new Vector3(0, rotation1 - 0.5f, 0) * FULL_ROTATION);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(new Vector3(0, 0, rotation1 - 0.5f) * FULL_ROTATION);
        }
    }

    public enum Dimenion
    {
        X, Y, Z
    }
}
