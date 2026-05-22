using UnityEngine;

public class WeaponObject : MonoBehaviour
{
    private float swingTimer;
    private float projectileTimer;
    private Vector3 projectileDirection;
    private object attackOrigin;
    private float power;
    private bool blocking;
    [SerializeField] private bool gravity;
    [SerializeField] private LayerMask destructible;
    [SerializeField] private LayerMask surface;
    public void startSwing(float swingTimer, object origin, float power)
    {
        this.swingTimer = swingTimer;
        attackOrigin = origin;
        this.power = power;
        GetComponent<Collider>().enabled = true;
    }
    public bool updateAttack()
    {
        if (swingTimer > 0)
        {
            swingTimer -= StaticData.deltaTimeStore;
            if (swingTimer <= 0)
            {
                GetComponent<Collider>().enabled = false;
                return false;
            }
        }
        return true;
    }
    public void launchAsPerishableProjectile(Vector3 direction, object origin, float timer, float power)
    {
        launchAsNonPerishableProjectile(direction, origin, power);
        projectileTimer = timer;
    }
    public void launchAsNonPerishableProjectile(Vector3 direction, object origin, float power)
    {
        projectileDirection = direction;
        attackOrigin = origin;
        this.power = power;
    }
    public void startBlocking()
    {
        blocking = true;
        GetComponent<Collider>().enabled = true;
    }
    public void stopBlocking()
    {
        blocking = false;
        GetComponent<Collider>().enabled = false;
    }
    void OnTriggerEnter(Collider other)
    {
        if (blocking && other.GetComponent<WeaponObject>())
        {
            other.GetComponent<WeaponObject>().stopAttack();
        }
        else if (other.GetComponent<BFDamageable>() != null)
        {
            hitTarget(other.GetComponent<BFDamageable>());
        }
    }
    public void stopAttack()
    {
        if (swingTimer > 0)
        {
            swingTimer = 0;
        }
        else if (projectileTimer > 0)
        {
            Destroy(gameObject);
        }
        else if (projectileDirection != Vector3.zero)
        {
            landOnGround();
        }
    }
    private void hitTarget(BFDamageable target)
    {
        //TODO
    }
    private void landOnGround()
    {
        projectileDirection = Vector3.zero;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + new Vector3(0, 0.5f, 0), Vector3.down,
            out hit, float.MaxValue, surface))
        {
            transform.position = hit.point;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
