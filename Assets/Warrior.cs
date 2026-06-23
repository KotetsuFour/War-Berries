using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CustomNavAgent))]
public class Warrior : PlayerEntity
{
    public static float STANDARD_WARRIOR_BODY_RADIUS = 0.5f;

    private CharacterData data;
    private GameObject model;
    private CustomNavAgent agent;
    private bool isBeingControlled;
    private CharacterController cc;
    private Collider generalCollider;

    private Warrior commandingOfficer;
    private CollisionSection target;

    private float ordersCooldown;
    private float targetingCooldown;
    private float weaponCooldown;
    private float hesitation;
    private static float ORDERS_COOLDOWN_MIN = 5;
    private static float TARGETING_COOLDOWN_MIN = 2;
    private static float HESITATION_MIN = 1;
    private static float ORDERS_COOLDOWN_INTERVAL = 0.5f; //For every 1% Leadership below the max
    private static float TARGETING_COOLDOWN_INTERVAL = 0.05f; //For every 1% Luck below the max
    private static float HESITATION_INTERVAL = 0.03f; //For every 1% Morale below the max

    private int dominantHandInventoryIdx;
    private int otherHandInventoryIdx;
    private int ammunitionIdx;
    private Item dominantHandItem;
    private Item otherHandItem;

    private Transform dominantHandTransform;
    private Transform otherHandTransform;

    private Task task;
    private TargetMode targetMode;

    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<CustomNavAgent>();
        model = StaticData.findDeepChild(transform, "model").gameObject;
        if (data != null)
        {
            Material[] materials = StaticData.findDeepChild(model.transform, "Mesh").GetComponent<SkinnedMeshRenderer>().materials;
            StaticData.paintHairSkinEye(materials, data.hair, data.skin, data.eye);
        }
        cc = GetComponent<CharacterController>();
        agent.setCharacterController(cc);
        //TODO set mass
        generalCollider = GetComponent<Collider>();
        if (data.leftHanded)
        {
            dominantHandTransform = StaticData.findDeepChild(transform, "LeftHandHoldPoint");
            otherHandTransform = StaticData.findDeepChild(transform, "RightHandHoldPoint");
        }
        else
        {
            dominantHandTransform = StaticData.findDeepChild(transform, "RightHandHoldPoint");
            otherHandTransform = StaticData.findDeepChild(transform, "LeftHandHoldPoint");
        }
    }
    public void setData(CharacterData data)
    {
        this.data = data;

        LinkedList<Transform> children = new LinkedList<Transform>();
        children.AddLast(transform);
        while (children.Count != 0)
        {
            Transform current = children.Last.Value;
            for (int q = 0; q < current.childCount; q++)
            {
                children.AddLast(current.GetChild(q));
            }
            children.RemoveFirst();
            CollisionSection section = current.GetComponent<CollisionSection>();
            if (section != null)
            {
                section.setParentBody(this, data);
                Rigidbody rb = section.GetComponent<Rigidbody>();
                rb.isKinematic = false;
            }
        }
    }
    public CharacterData getData()
    {
        return data;
    }
    public override void attachPlayer(PlayerInput playerInput, InputDevice inputDevice)
    {
        base.attachPlayer(playerInput, inputDevice);
        isBeingControlled = true;
        agent.setActive(false);
    }
    public override void detatchPlayer()
    {
        isBeingControlled = false;
        agent.setActive(true);
    }
    public bool isControlledByPlayer()
    {
        return isBeingControlled;
    }
    public override void aButton(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            Debug.Log($"Player{playerInput.playerIndex} Pressed the A Button! Just them, no one else.");
        }
    }

    public override void bButton(InputAction.CallbackContext ctx)
    {

    }

    public override void downButton(InputAction.CallbackContext ctx)
    {

    }

    public override void extra(InputAction.CallbackContext ctx)
    {

    }

    public override void home(InputAction.CallbackContext ctx)
    {

    }

    public override void l1(InputAction.CallbackContext ctx)
    {

    }

    public override void l2(InputAction.CallbackContext ctx)
    {

    }

    public override void leftButton(InputAction.CallbackContext ctx)
    {

    }

    public override void leftMove(InputAction.CallbackContext ctx)
    {
        Vector2 inpt = ctx.ReadValue<Vector2>();
        Vector3 dir = new Vector3(inpt.x, 0, inpt.y);
        dir = (Quaternion.Euler(dir) * transform.rotation).eulerAngles;
        dir.y = 0;
        move(dir);
    }

    public override void lPress(InputAction.CallbackContext ctx)
    {

    }

    public override void minus(InputAction.CallbackContext ctx)
    {

    }

    public override void mouseLeft(InputAction.CallbackContext ctx)
    {

    }

    public override void mouseMiddle(InputAction.CallbackContext ctx)
    {

    }

    public override void mouseRight(InputAction.CallbackContext ctx)
    {

    }

    public override void mouseScroll(InputAction.CallbackContext ctx)
    {

    }

    public override void plus(InputAction.CallbackContext ctx)
    {

    }

    public override void r1(InputAction.CallbackContext ctx)
    {

    }

    public override void r2(InputAction.CallbackContext ctx)
    {

    }

    public override void rightButton(InputAction.CallbackContext ctx)
    {

    }

    public override void rightMove(InputAction.CallbackContext ctx)
    {
        //TODO handle differently if manning a vehicle or not in control
        Vector2 inpt = ctx.ReadValue<Vector2>();
        transform.Rotate(Vector3.up * inpt.x * StaticData.deltaTimeStore * data.getRotationSpeed());
        Transform waist = StaticData.findDeepChild(transform, "WaistJoint");
        Vector3 waistRot = waist.localRotation.eulerAngles;
        waistRot.x = Mathf.Clamp(waistRot.x + (inpt.y * StaticData.deltaTimeStore * data.getRotationSpeed()),
            -80, 80);
        waist.rotation = Quaternion.Euler(waistRot);
    }

    public override void rPress(InputAction.CallbackContext ctx)
    {

    }

    public override void upButton(InputAction.CallbackContext ctx)
    {

    }

    public override void xButton(InputAction.CallbackContext ctx)
    {

    }

    public override void yButton(InputAction.CallbackContext ctx)
    {

    }

    private void move(Vector3 direction)
    {
        //TODO handle differently if manning a vehicle or not in control
        transform.position += direction.normalized * Time.deltaTime * data.getMoveSpeed();
    }
    public void setTask(Task task)
    {
        this.task = task;
    }

    public void updateAI(int mapExtent)
    {
        if (!agent.isActive())
        {
            return;
        }
        //TODO this is a test. Do the actual algorithm
        if (agent.reachedDestination())
        {
            agent.setDestination(new Vector3(Random.Range(0, mapExtent) - (mapExtent / 2),
                0, Random.Range(0, mapExtent) - (mapExtent / 2)),
                generalCollider);
            agent.setActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hesitation > 0)
        {
            hesitation -= StaticData.deltaTime();
        }
        if (weaponCooldown > 0)
        {
            weaponCooldown -= StaticData.deltaTime();
        }
        if (isBeingControlled)
        {
            //TODO player updates as needed
        }
        else
        {
            if (data.isLeader()) //The you're a squad leader
            {
                //Frequency of ordering (including navigation) (based on Leadership stat)
                if (ordersCooldown <= 0)
                {
                    if (commandingOfficer == null) //Then you are the commander in this battle
                    {
                        //TODO overall commands. Communicate with the Battlefield itself to
                        //centralize info and tell all affiliated soldiers present what to do
                        //based on your own team's WMOrders
                    }
                    //Then, normal squad leader stuff.
                    //Sneak, march, or run
                    //Hold fire, return fire, or fire at will
                    //What targets to prioritize
                    //Formation and special tasks
                    //Healing, rest and support
                    data.team.assignWarriorRoles();
                    //And cooldown
                    ordersCooldown = ORDERS_COOLDOWN_MIN
                        + ((CharacterData.MAX_LEADERSHIP - data.leadership) * ORDERS_COOLDOWN_INTERVAL);
                }
                else
                {
                    ordersCooldown -= StaticData.deltaTime();
                }
            } //Else, you're a non-leader team member, so you only do the standard actions

            //Frequency of finding/changing targets (based on Luck stat)
            if (targetingCooldown <= 0)
            {
                //Search for a target by going through the list of nearby targets associated
                //with your task, and seeing if they're viable
                if (task == Task.NO_PREFERENCE)
                {
                    //Nothing. For runtime purposes. If you should do SOMETHING, the task should be
                    //set to OFFENSE, and if that fails, you'll just automatically try HEALING next
                }
                else if (task == Task.OFFENSE)
                {
                    setOffensiveTargetAndWeapon();
                }
                else if (task == Task.SUPPORT)
                {
                    setSupportTargetAndItem();
                }
                else if (task == Task.DEMOLITION)
                {
                    //TODO there's probably a kind of productive rest that can be applied here
                }
                //Then both targeting cooldown (based on Luck)
                //and hesitation (based on Morale)
                startHesitationAndTargetCooldown();

                //If no immediate threat exists, or you're primarily support anyway,
                //check for nearby allies that need help and equip a support item or nothing instead
                if (task == Task.OFFENSE && target == null)
                {
                    setSupportTargetAndItem();
                }

                //The original thought was, if target is still null, just look in a random direction
                //for an enemy. Except, actually, if you couldn't see anyone before, while using
                //the Battlefield object to search, then you won't see anyone anyway, so nevermind
            }
            else
            {
                targetingCooldown -= StaticData.deltaTime();
            }

            //Frequency of firing (based on Weapon)
            if (target != null)
            {
                if (dominantHandItem != null)
                {
                    tryUsingItem(dominantHandTransform, dominantHandItem, dominantHandInventoryIdx, true);
                }
                if (otherHandItem != null)
                {
                    tryUsingItem(otherHandTransform, otherHandItem, otherHandInventoryIdx, false);
                }
            }
        }
    }

    private void tryUsingItem(Transform hand, Item handItem, int handIdx, bool isDominantHand)
    {
        if (handItem is Weapon)
        {
            //Then you're for sure trying to fire at someone or something, because otherwise
            //you would just have idx -1 equipped, making primeHandItem null
            Weapon wep = (Weapon)handItem;
            if (hesitation <= 0 && weaponCooldown <= 0)
            {
                //Then you can fire, having updated the hesitation
                //and weaponCooldown timers outside the if(isBeingControlled) statement
                if (wep is RangeWeapon)
                {
                    //Shoot from ammunitionIdx
                    if (data.inventory[ammunitionIdx][2] > 0)
                    {
                        //Shoot
                        Item item = StaticData.getItemFromIndex(data.inventory[ammunitionIdx]);
                        shootProjectile((RangeWeapon)wep, item, hand);
                        if (!data.removeItemAtIdx(ammunitionIdx, 1))
                        {
                            //The item has been removed from the inventory
                            //Update hand indexes as needed, then set ammunitionIdx to -1
                            if (dominantHandInventoryIdx > ammunitionIdx)
                            {
                                dominantHandInventoryIdx--;
                            }
                            if (otherHandInventoryIdx > ammunitionIdx)
                            {
                                otherHandInventoryIdx--;
                            }
                            ammunitionIdx = -1;
                        }
                    }
                }
                else if (wep is MeleeWeapon)
                {
                    //TODO swing the weapon
                }
                else if (wep is ThrowWeapon)
                {
                    //Throw the weapon
                    if (!data.removeItemAtIdx(handIdx, 1))
                    {
                        //The item has been removed from the inventory
                        //Update hand indexes as needed, then set ammunitionIdx to -1
                        if (dominantHandInventoryIdx >= handIdx)
                        {
                            dominantHandInventoryIdx--;
                        }
                        if (otherHandInventoryIdx >= handIdx)
                        {
                            otherHandInventoryIdx--;
                        }
                        if (isDominantHand)
                        {
                            dominantHandInventoryIdx = -1;
                        }
                        else
                        {
                            otherHandInventoryIdx = -1;
                        }
                    }
                }
                weaponCooldown = ((Weapon)handItem).cooldown;
            }
        }
    }

    private void shootProjectile(RangeWeapon wep, Item projectileItem, Transform hand)
    {
        Transform launchPoint = StaticData.findDeepChild(hand, "LaunchPoint");
        Vector3 targetPos = target.transform.position;
        float distanceFromTarget = (targetPos - launchPoint.position).magnitude;
        float acc = data.dexterity * wep.maxRecommendedRange / distanceFromTarget;
        //Essentially what's happening is, we're getting the area of the circle which is guaranteed
        //to hit the target (PI * r^2). Then we divide that by the accuracy percentage,
        //which gives us the full margin of error circle (/ acc). Then, we find the radius of THAT circle
        //by dividing by PI and then taking the square root. So simplified, our radius for the margin
        //of error circle is:
        float errRad = Mathf.Sqrt(Mathf.Pow(STANDARD_WARRIOR_BODY_RADIUS, 2) / acc);
        //Now, the place we will aim for is anywhere between 0 and errRad away from the target's center,
        //in any direction
        errRad = Random.Range(0, errRad);
        float randomAngle = Random.Range(0, 360);
        //Now, we can calculate the effective point we'll hit
        Vector3 derivativeTarget = targetPos + (launchPoint.up * errRad * Mathf.Sin(randomAngle))
            + launchPoint.right * errRad * Mathf.Cos(randomAngle);
        //And therefore, the trajectory of the projectile
        Vector3 lineToDerivedTarget = (derivativeTarget - launchPoint.position).normalized;
        //And fire
        Projectile proj = Instantiate(AssetDictionary.getProjectilePrefab(projectileItem.itemName));
        proj.initializeDirection(wep.launchSpeed * lineToDerivedTarget,
            projectileItem.weight > 0, RangeWeapon.STANDARD_DESPAWN_TIMEOUT,
            data, projectileItem.weight);
    }

    private void startHesitationAndTargetCooldown()
    {
        targetingCooldown = TARGETING_COOLDOWN_MIN
            + ((CharacterData.MAX_LUCK - data.luck) * TARGETING_COOLDOWN_INTERVAL);
        hesitation = HESITATION_MIN
            + ((CharacterData.MAX_MORALE - data.morale) * HESITATION_INTERVAL);
    }

    private void setOffensiveTargetAndWeapon()
    {
        //TODO select the target
        //TODO Then decide what item to use AFTER, NOT DURING
        //If you have no weapon, equip nothing (-1)
        //Or maybe ask for one. Maybe not. You have to prompt people to share in MoW:AS2
    }
    private void setSupportTargetAndItem()
    {
        //TODO select the target
        //TODO Then decide what item to use AFTER, NOT DURING
        //If you have no relevant item, equip nothing (-1)
        //Or maybe ask for one. Or maybe not. In MoW:AS2, you have to prompt people to share items
    }

    /**
     * A nearby attack may trigger this function, and this Warrior would then tell
     * the rest of their team about the threat
     */
    public void noticeImminentThreat()
    {
        //TODO
    }
    public enum TargetMode
    {
        HOLD_FIRE, RETURN_FIRE, FIRE_AT_WILL
    }
    public enum Task
    {
        NO_PREFERENCE, OFFENSE, SUPPORT, DEMOLITION, WATCH
    }
}
