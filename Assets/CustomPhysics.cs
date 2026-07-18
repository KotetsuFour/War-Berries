using UnityEngine;

/// <summary>
/// A custom physics component that manually simulates gravity and velocity,
/// moving the GameObject via transform.position, using Physics.BoxCast
/// (sized to this object's existing Collider bounds) to check for
/// obstructions before committing each frame's movement.
///
/// Time is driven by StaticData.deltaTime() (assumed to already exist
/// elsewhere in the project) instead of Time.deltaTime, scaled by a
/// per-object timeDilation multiplier.
///
/// Assumes a Collider is attached to this GameObject. It does not need to be
/// a BoxCollider specifically -- Collider.bounds gives an axis-aligned box
/// that works as a conservative approximation for Capsule, Sphere, Mesh,
/// etc. colliders too.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CustomPhysics : MonoBehaviour
{
    [Header("Time")]
    [SerializeField] private float timeDilation = 1f;

    [Header("Gravity")]
    [SerializeField] private bool affectedByGravity = true;

    [Header("Mass")]
    [SerializeField] private float mass = 1f; // used to convert AddForce impulses into velocity changes, and to weight mass-based collisions

    /// <summary>This object's mass, readable by other CustomPhysics instances for collision resolution.</summary>
    public float Mass => mass;

    /// <summary>This object's current velocity, readable by other CustomPhysics instances for collision resolution.</summary>
    public Vector3 Velocity => velocity;

    [Header("Friction")]
    [SerializeField] private float groundFriction = 5f; // horizontal speed lost per second while grounded (Coulomb/linear friction)

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask = ~0; // "Everything" by default
    [SerializeField] private float skinWidth = 0.01f; // shrinks the cast box slightly to avoid false-positive self collisions
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
    [SerializeField, Range(0f, 1f)] private float collisionBounciness = 0.5f; // when colliding with another CustomPhysics: how much of the blocked movement carries through as a push (0 = fully absorbed, like hitting something impassable; 1 = fully redistributed by mass, no energy lost)

    private Collider ownCollider;

    // Persistent velocity (units/second). Gravity, AddVelocity, and AddForce
    // all accumulate into this, and it carries over frame to frame.
    private Vector3 velocity;

    // One-frame movement buffer set by Move(). Unlike velocity, this does NOT
    // persist -- it's consumed and cleared every Update, similar to
    // CharacterController.Move(). Assumed to be set at most once per frame
    // from a single call site, so it's overwritten rather than accumulated.
    private Vector3 pendingMove;

    // Whether this object is currently resting on something below it, as of
    // the last Update's collision resolution. Ground friction only applies
    // while true.
    private bool grounded;

    // Floor for closing speed used in momentum-weighted collision splitting.
    // Without this, an object at rest (closing speed 0) would have zero
    // effective weight and offer no resistance at all; flooring at 1 makes
    // resting objects fall back to plain mass-based weighting instead.
    private const float MinimumClosingSpeedForMomentumWeighting = 1f;

    // Tracks the mass-based collisions this object took part in this frame
    // (as either the initiator or the one being pushed), so that if both
    // objects in a pair detect the same collision, only one of them actually
    // computes and applies the push. Fixed at 2 slots since each object casts
    // at most twice per frame (once per axis), so it can have at most 2
    // distinct collision partners in a single frame -- no dynamic allocation
    // needed.
    private const int MaxCollisionRecordsPerFrame = 2;
    private readonly int[] collisionRecordFrames = { -1, -1 };
    private readonly CustomPhysics[] collisionRecordPartners = new CustomPhysics[MaxCollisionRecordsPerFrame];
    private int nextCollisionRecordSlot;

    private void Awake()
    {
        ownCollider = GetComponent<Collider>();
    }

    /// <summary>
    /// Performs a BoxCast sized to this object's Collider bounds, shrunk by
    /// skinWidth, using the object's current position and rotation. Shared
    /// by ResolveMovement (axis-aligned casts) and the diagonal corner-clip
    /// check in Update.
    /// </summary>
    private bool TryBoxCast(Vector3 direction, float distance, out RaycastHit hit)
    {
        Bounds bounds = ownCollider.bounds;
        Vector3 halfExtents = Vector3.Max(bounds.extents - Vector3.one * skinWidth, Vector3.zero);

        return Physics.BoxCast(
            bounds.center,
            halfExtents,
            direction,
            out hit,
            transform.rotation,
            distance,
            collisionMask,
            triggerInteraction
        );
    }

    /// <summary>
    /// Fallback for the specific case where the vertical BoxCast reports no
    /// hit even though this object is already resting on (or very slightly
    /// embedded in) something below it. BoxCast -- like SphereCast and
    /// CapsuleCast -- silently excludes anything its swept volume already
    /// overlaps at the start of the cast, so an object resting exactly at a
    /// surface can intermittently fail to detect it via BoxCast alone.
    /// Raycast doesn't have this exclusion (no swept volume to be embedded
    /// in), so a short downward ray from the collider's center catches this
    /// case cheaply. Only checked for the vertical axis, since that's the
    /// one direction that keeps re-approaching the ground every frame under
    /// gravity even while otherwise at rest.
    /// </summary>
    private bool IsRestingOnGroundFallback()
    {
        Bounds bounds = ownCollider.bounds;
        float rayDistance = bounds.extents.y + skinWidth;

        if (!Physics.Raycast(bounds.center, Vector3.down, out RaycastHit hit, rayDistance, collisionMask, triggerInteraction))
        {
            return false;
        }

        CustomPhysics otherPhysics = hit.collider.GetComponent<CustomPhysics>();

        // Same "an inactive CustomPhysics is ignored entirely" rule as ResolveMovement.
        return otherPhysics == null || otherPhysics.enabled;
    }

    /// <summary>
    /// Sets the time dilation multiplier applied on top of StaticData.deltaTime().
    /// </summary>
    public void SetTimeDilation(float newTimeDilation)
    {
        timeDilation = newTimeDilation;
    }

    /// <summary>
    /// Incorporates the given movement into CustomPhysics's movement for this
    /// frame only. This is added on top of velocity-driven movement (gravity,
    /// AddVelocity, AddForce) but is not itself persisted -- call this every
    /// frame you want the effect to continue (e.g. from player input). Assumes
    /// this is called at most once per frame from a single source; each call
    /// overwrites the previous frame's value rather than stacking with it.
    /// </summary>
    public void Move(Vector3 movement)
    {
        pendingMove = movement;
    }

    /// <summary>
    /// Adds the given vector directly to this object's persistent velocity.
    /// Unlike Move(), this change carries over into future frames until
    /// altered again (e.g. by gravity, collisions, or further calls).
    /// </summary>
    public void AddVelocity(Vector3 velocityToAdd)
    {
        velocity += velocityToAdd;
    }

    /// <summary>
    /// Simulates the application of an explosive/impulsive force on the
    /// object, converting it into an instantaneous velocity change based on
    /// this object's mass (velocity += force / mass), similar to
    /// Rigidbody.AddForce with ForceMode.Impulse.
    /// </summary>
    public void AddForce(Vector3 force)
    {
        float safeMass = Mathf.Max(mass, Mathf.Epsilon);
        velocity += force / safeMass;
    }

    /// <summary>
    /// This object's dilated delta time: StaticData.deltaTime() * timeDilation.
    /// </summary>
    private float GetDeltaTime()
    {
        return StaticData.deltaTime() * timeDilation;
    }

    /// <summary>
    /// Integrates gravity and friction into velocity, then resolves this
    /// frame's movement one axis at a time: horizontal (x/z) and vertical (y)
    /// are BoxCast and clamped/pushed independently, so e.g. a fall can land
    /// (vertical resolved) in the same frame a horizontal collision brings it
    /// to a stop (horizontal resolved) -- one axis being blocked doesn't
    /// truncate the other, unlike casting along the single combined vector.
    /// Each axis's cast is skipped entirely when that axis has no movement,
    /// which is the common case for e.g. a purely-falling object.
    ///
    /// When both axes have movement this frame, a third cast along the
    /// original diagonal vector catches corners that neither axis-aligned
    /// sweep would touch on its own (see ClampToDiagonal) before the combined
    /// movement is applied.
    /// </summary>
    private void Update()
    {
        float dt = GetDeltaTime();

        if (affectedByGravity)
        {
            velocity.y += StaticData.GRAVITY * dt;
        }

        if (grounded)
        {
            ApplyGroundFriction(dt);
        }

        Vector3 frameMovement = velocity * dt + pendingMove;
        pendingMove = Vector3.zero;

        if (frameMovement == Vector3.zero)
        {
            return;
        }

        grounded = false;

        Vector3 horizontalMovement = new Vector3(frameMovement.x, 0f, frameMovement.z);
        Vector3 verticalMovement = new Vector3(0f, frameMovement.y, 0f);

        Vector3 resolvedMovement = Vector3.zero;

        if (horizontalMovement != Vector3.zero)
        {
            resolvedMovement += ResolveMovement(horizontalMovement);
        }

        if (verticalMovement != Vector3.zero)
        {
            resolvedMovement += ResolveMovement(verticalMovement);
        }

        if (horizontalMovement != Vector3.zero && verticalMovement != Vector3.zero)
        {
            resolvedMovement = ClampToDiagonal(resolvedMovement, frameMovement);
        }

        transform.position += resolvedMovement;
    }

    /// <summary>
    /// Applies constant-deceleration (linear/Coulomb) friction to the
    /// horizontal component of velocity, leaving vertical velocity untouched.
    /// Speed is reduced by a fixed amount per second until it reaches zero --
    /// it does not reverse direction or overshoot into negative speed.
    /// </summary>
    private void ApplyGroundFriction(float dt)
    {
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        float speed = horizontalVelocity.magnitude;

        if (speed <= 0f)
        {
            return;
        }

        float newSpeed = Mathf.Max(speed - groundFriction * dt, 0f);
        Vector3 newHorizontalVelocity = horizontalVelocity.normalized * newSpeed;

        velocity.x = newHorizontalVelocity.x;
        velocity.z = newHorizontalVelocity.z;
    }

    /// <summary>
    /// Casts once along the original (pre-axis-split) diagonal movement
    /// vector, purely to catch corner-clipping that the separate horizontal
    /// and vertical casts can miss -- an obstacle sitting on the diagonal
    /// path that isn't touched by either axis-aligned sweep from the
    /// starting position. Unlike ResolveMovement, this never pushes another
    /// CustomPhysics object and never records a collision (the 2-slot guard
    /// is untouched by this method) -- it only scales the already-resolved
    /// movement down if it would overshoot what the diagonal cast allows.
    /// An inactive CustomPhysics on the other end is ignored, same as in
    /// ResolveMovement.
    /// </summary>
    private Vector3 ClampToDiagonal(Vector3 resolvedMovement, Vector3 diagonalMovement)
    {
        float distance = diagonalMovement.magnitude;
        Vector3 direction = diagonalMovement / distance;

        if (!TryBoxCast(direction, distance, out RaycastHit hit))
        {
            return resolvedMovement;
        }

        CustomPhysics otherPhysics = hit.collider.GetComponent<CustomPhysics>();

        if (otherPhysics != null && !otherPhysics.enabled)
        {
            // Inactive CustomPhysics -- ignore this hit entirely.
            return resolvedMovement;
        }

        float allowedDistance = Mathf.Max(hit.distance - skinWidth, 0f);
        float projectedDistance = Vector3.Dot(resolvedMovement, direction);

        if (projectedDistance <= allowedDistance)
        {
            // Not overshooting the diagonal cast's hit point -- nothing to clamp.
            return resolvedMovement;
        }

        float scale = allowedDistance / projectedDistance;
        return resolvedMovement * scale;
    }

    /// <summary>
    /// BoxCasts along the desired movement direction (expected to be a
    /// single-axis vector -- horizontal or vertical -- as called from
    /// Update) using this object's Collider bounds. If something is hit, the
    /// movement is clamped to just short of the hit point; if this was the
    /// vertical axis and the object was falling, vertical velocity is reset
    /// and grounded is set (i.e. it "landed").
    ///
    /// If the thing hit does NOT have a CustomPhysics component, it's treated
    /// as impassable (movement simply clamps at the hit point). If it has an
    /// inactive CustomPhysics component, the collision is ignored entirely
    /// (full desired movement is allowed, as if nothing were hit). Otherwise,
    /// the portion of movement that would have been blocked is split between
    /// the two objects, weighted by momentum (mass * closing speed, not mass
    /// alone) and scaled by collisionBounciness (some of the blocked movement
    /// is simply lost rather than becoming a push, simulating a
    /// partially-inelastic impact). This only nudges positions for this one
    /// frame -- neither object's stored velocity is changed by the collision.
    /// If the same pair of objects resolves against each other more than
    /// once in a frame (e.g. once per axis, or from each object's own
    /// Update), only the first resolution actually computes and applies the
    /// split; later ones just clamp to the hit point.
    /// </summary>
    private Vector3 ResolveMovement(Vector3 desiredMovement)
    {
        float distance = desiredMovement.magnitude;
        Vector3 direction = desiredMovement / distance;

        bool hitSomething = TryBoxCast(direction, distance, out RaycastHit hit);

        if (!hitSomething)
        {
            if (affectedByGravity && desiredMovement.y < 0f && IsRestingOnGroundFallback())
            {
                // The vertical BoxCast found nothing, but a downward Raycast
                // confirms we're already touching/embedded in something --
                // this is the sweep-exclusion gap (BoxCast silently ignores
                // anything it starts already overlapping). Treat this as
                // already landed rather than falling through.
                velocity.y = 0f;
                grounded = true;
                return Vector3.zero;
            }

            return desiredMovement;
        }

        float allowedDistance = Mathf.Max(hit.distance - skinWidth, 0f);

        if (affectedByGravity && desiredMovement.y < 0f)
        {
            velocity.y = 0f;
            grounded = true;
        }

        CustomPhysics otherPhysics = hit.collider.GetComponent<CustomPhysics>();

        if (otherPhysics == null)
        {
            // No CustomPhysics on the other object -- treat it as impassable.
            return direction * allowedDistance;
        }

        if (!otherPhysics.enabled)
        {
            // Inactive CustomPhysics (e.g. an inactive child collider) --
            // ignore this collision entirely, as if nothing were hit.
            return desiredMovement;
        }

        int currentFrame = Time.frameCount;

        if (HasAlreadyResolvedWith(otherPhysics, currentFrame))
        {
            // This pair was already resolved (from either side) earlier this
            // frame -- don't compute and apply the push a second time. Just
            // clamp to the hit point as normal.
            return direction * allowedDistance;
        }

        float blockedDistance = Mathf.Max(distance - allowedDistance, 0f);
        float redistributable = blockedDistance * collisionBounciness;

        // Momentum-weighted split: weight by mass * closing speed rather than
        // mass alone, so a fast, light object can still shove a heavy, slow
        // one further than pure mass would allow. Closing speed is floored
        // at 1 (not 0) so that an object at rest still falls back to plain
        // mass-based weighting instead of appearing "weightless" -- a heavy
        // stationary object should still resist being pushed.
        float selfClosingSpeed = Mathf.Max(Vector3.Dot(velocity, direction), MinimumClosingSpeedForMomentumWeighting);
        float otherClosingSpeed = Mathf.Max(Vector3.Dot(otherPhysics.Velocity, -direction), MinimumClosingSpeedForMomentumWeighting);

        float selfWeight = mass * selfClosingSpeed;
        float otherWeight = otherPhysics.Mass * otherClosingSpeed;

        float invWeightSelf = 1f / Mathf.Max(selfWeight, Mathf.Epsilon);
        float invWeightOther = 1f / Mathf.Max(otherWeight, Mathf.Epsilon);
        float invWeightSum = invWeightSelf + invWeightOther;

        float selfShare = redistributable * (invWeightSelf / invWeightSum);
        float otherShare = redistributable * (invWeightOther / invWeightSum);

        // Record that this pair has been resolved this frame, on both sides,
        // so neither object re-applies the same collision again this frame.
        RecordCollisionResolution(otherPhysics, currentFrame);
        otherPhysics.RecordCollisionResolution(this, currentFrame);

        // Push the other object away along the same direction we were moving.
        otherPhysics.transform.position += direction * otherShare;

        return direction * (allowedDistance + selfShare);
    }

    /// <summary>
    /// Checks whether this object has already resolved a mass-based
    /// collision against the given partner during the given frame.
    /// </summary>
    private bool HasAlreadyResolvedWith(CustomPhysics partner, int frame)
    {
        for (int i = 0; i < MaxCollisionRecordsPerFrame; i++)
        {
            if (collisionRecordFrames[i] == frame && collisionRecordPartners[i] == partner)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Records that this object resolved a mass-based collision against the
    /// given partner during the given frame, overwriting the oldest record.
    /// </summary>
    private void RecordCollisionResolution(CustomPhysics partner, int frame)
    {
        collisionRecordFrames[nextCollisionRecordSlot] = frame;
        collisionRecordPartners[nextCollisionRecordSlot] = partner;
        nextCollisionRecordSlot = (nextCollisionRecordSlot + 1) % MaxCollisionRecordsPerFrame;
    }
}