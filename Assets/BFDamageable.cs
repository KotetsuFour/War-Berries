using UnityEngine;
[RequireComponent(typeof(Collider))]
public abstract class BFDamageable : MonoBehaviour
{
    public abstract void takeDamage(Vector3 damageForce, CharacterData origin);
}
