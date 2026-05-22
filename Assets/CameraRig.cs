using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraRig : MonoBehaviour
{
    [SerializeField] private Transform focus;
    [SerializeField] private float defaultDistance;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    private float distance;

    void Awake()
    {
        distance = defaultDistance;
    }

    public void rotate(Vector3 angle, float speed)
    {
        transform.RotateAround(focus.position, angle, speed);
    }

    public void zoom(float amount)
    {
        distance = Mathf.Clamp(distance + amount, minDistance, maxDistance);
    }

    // Update is called once per frame
    void Update()
    {
        if (focus !=  null)
        {
            Quaternion look = Quaternion.LookRotation(focus.position - transform.position); 
            Vector3 dir = -look.eulerAngles.normalized;
            transform.SetPositionAndRotation(focus.position + (dir * distance),
                look);
        }
    }
}
