using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float despawnTimeout;
    private Vector3 velocity;
    private bool hasGravity;
    private float mass;
    private CharacterData owner;
    [SerializeField] LayerMask allBattlefieldCollidables;

    //Once the projectile hits, these values are stored so that landingEffect can be abstracted
    private Vector3 hitPoint;
    private Collider hitThing;
    public void initializeDirection(Vector3 velocity, bool hasGravity,
        float despawnTimeout, CharacterData owner, float mass)
    {
        transform.rotation = Quaternion.LookRotation(-velocity);
        this.velocity = velocity;
        this.despawnTimeout = despawnTimeout;
        this.hasGravity = hasGravity;
        this.owner = owner;
        this.mass = mass;
    }

    void Update()
    {
        move();
    }

    protected virtual void updateTimeoutTimer(float deltaTime)
    {
        despawnTimeout -= StaticData.deltaTime();
        if (despawnTimeout <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void move()
    {
        float deltaTime = StaticData.deltaTime();
        RaycastHit hit;
        if (Physics.Raycast(transform.position, velocity, out hit,
            velocity.magnitude * deltaTime, allBattlefieldCollidables))
        {
            hitPoint = hit.point;
            hitThing = hit.collider;
            landingEffect();
        }
        else
        {
            transform.Translate(velocity);
            if (hasGravity)
            {
                velocity.y -= StaticData.GRAVITY * StaticData.deltaTime();
                transform.rotation = Quaternion.LookRotation(-velocity);
            }
            updateTimeoutTimer(deltaTime);
        }
    }
    protected virtual void landingEffect()
    {
        BFDamageable thing = hitThing.GetComponent<BFDamageable>();
        if (thing != null)
        {
            thing.takeDamage(velocity * mass, owner);
        }
    }
}
