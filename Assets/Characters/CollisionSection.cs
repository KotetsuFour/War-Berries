using UnityEngine;

[RequireComponent(typeof(CustomPhysics))]
public class CollisionSection : BFDamageable
{
    private MonoBehaviour parent;
    private CharacterData data;
    [SerializeField] private int partIdx;
    public void setParentBody(MonoBehaviour parent, CharacterData data)
    {
        this.parent = parent;
        this.data = data;
    }
    public MonoBehaviour getParentBody()
    {
        return parent;
    }
    public override void takeDamage(Vector3 damageForce, CharacterData origin)
    {
        //TODO
        float damage = damageForce.magnitude;
        Debug.Log($"The raw damage from {origin} was {damage}.");

        //TODO apply damage

        float hp = data.bodyPartsCurrentHP[partIdx];
        detachFromBody(damageForce);
    }
    private void detachFromBody(Vector3 detachmentForce)
    {
        transform.SetParent(null);
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.AddForceAtPosition(detachmentForce, rb.centerOfMass, ForceMode.Impulse);
        }
    }
}
