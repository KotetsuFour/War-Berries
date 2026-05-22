using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider))]
public class WMCursor : PlayerEntity
{
    private Transform display;
    private Collider coll;
    private Tile currentTile;
    private WMTeam currentTeam;
    private WMStructure currentStructure;
    [SerializeField] private float cursorMoveSpeed;
    [SerializeField] private float cursorRotateSpeed;
    [SerializeField] private float cursorZoomSpeed;
    [SerializeField] private CameraRig cam;
    [SerializeField] private float boxCastRange;
    private Affiliation affiliation;

    private WMEntity selectedEntity;
    void Awake()
    {
        coll = GetComponent<Collider>();
        //TODO delete the next line, as it is just for testing
        affiliation = StaticData.affiliations[0];
    }
    public override void detatchPlayer()
    {

    }
    public override void aButton(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            StaticData.pauseWM(this);
            if (selectedEntity == null)
            {
                if (currentTeam != null && currentTeam.getTeam().getAffiliation().answersTo(affiliation))
                {
                    selectTeam();
                }
            }
            else
            {
                if (!selectedEntity.receiveOrder(coll, display))
                {
                    deselectTeam();
                }
            }
        }
    }

    public override void bButton(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (selectedEntity != null)
            {
                if (!selectedEntity.retractOrder(coll, display))
                {
                    deselectTeam();
                }
            }
            else
            {
                StaticData.unpauseWM(this);
            }
        }
    }

    public override void downButton(InputAction.CallbackContext ctx)
    {
        if (selectedEntity == null || selectedEntity.up(coll, display, ctx))
        {
            Vector3 dir = (cam.transform.rotation * Quaternion.Euler(0, 180, 0)).eulerAngles;
            dir.y = 0;
            transform.position += dir.normalized * Time.deltaTime * cursorMoveSpeed;
        }
    }

    public override void extra(InputAction.CallbackContext ctx)
    {

    }

    public override void home(InputAction.CallbackContext ctx)
    {

    }

    public override void l1(InputAction.CallbackContext ctx)
    {
        cam.zoom(-cursorZoomSpeed * Time.deltaTime);
    }

    public override void l2(InputAction.CallbackContext ctx)
    {

    }

    public override void leftButton(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && (selectedEntity == null || !selectedEntity.up(coll, display, ctx)))
        {
            Vector3 dir = (cam.transform.rotation * Quaternion.Euler(0, -90, 0)).eulerAngles;
            dir.y = 0;
            transform.position += dir.normalized * Time.deltaTime * cursorMoveSpeed;
        }
    }

    public override void leftMove(InputAction.CallbackContext ctx)
    {
        if (selectedEntity == null || selectedEntity.up(coll, display, ctx))
        {
            Vector2 inpt = ctx.ReadValue<Vector2>();
            Vector3 dir = new Vector3(inpt.x, 0, inpt.y);
            dir = (Quaternion.Euler(dir) * cam.transform.rotation).eulerAngles;
            dir.y = 0;
            transform.position += dir.normalized * Time.deltaTime * cursorMoveSpeed;
        }
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
        ctx.ReadValue<float>();
        cam.zoom(cursorZoomSpeed * Time.deltaTime);
    }

    public override void plus(InputAction.CallbackContext ctx)
    {

    }

    public override void r1(InputAction.CallbackContext ctx)
    {
        cam.zoom(cursorZoomSpeed * Time.deltaTime);
    }

    public override void r2(InputAction.CallbackContext ctx)
    {

    }

    public override void rightButton(InputAction.CallbackContext ctx)
    {
        if (selectedEntity == null || selectedEntity.up(coll, display, ctx))
        {
            Vector3 dir = (cam.transform.rotation * Quaternion.Euler(0, 90, 0)).eulerAngles;
            dir.y = 0;
            transform.position += dir.normalized * Time.deltaTime * cursorMoveSpeed;
        }
    }

    public override void rightMove(InputAction.CallbackContext ctx)
    {
        Vector2 inpt = ctx.ReadValue<Vector2>();
        Vector3 dir = new Vector3(0, inpt.x, inpt.y);
        cam.rotate(dir, Time.deltaTime * cursorMoveSpeed);
    }

    public override void rPress(InputAction.CallbackContext ctx)
    {

    }

    public override void upButton(InputAction.CallbackContext ctx)
    {
        if (selectedEntity == null || selectedEntity.up(coll, display, ctx))
        {
            Vector3 dir = cam.transform.rotation.eulerAngles;
            dir.y = 0;
            transform.position += dir.normalized * Time.deltaTime * cursorMoveSpeed;
        }
    }

    public override void xButton(InputAction.CallbackContext ctx)
    {
        //TODO Generate general menu
    }

    public override void yButton(InputAction.CallbackContext ctx)
    {

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit[] hitData =
        Physics.BoxCastAll(coll.bounds.center, coll.bounds.extents / 2, Vector3.down,
            Quaternion.identity, boxCastRange, coll.includeLayers);
        bool hasTeam = false;
        bool hasStructure = false;
        foreach (RaycastHit hit in hitData)
        {
            if (hit.collider.GetComponent<Tile>() != null)
            {
                Tile checkTile = hit.collider.GetComponent<Tile>();
                if (currentTile == null
                    || (transform.position - checkTile.transform.position).magnitude
                    < (transform.position - currentTile.transform.position).magnitude)
                {
                    currentTile = checkTile;
                }
            }
            if (hit.collider.GetComponent<WMTeam>() != null)
            {
                WMTeam checkTeam = hit.collider.GetComponent<WMTeam>();
                if (currentTeam == null
                    || (transform.position - checkTeam.transform.position).magnitude
                    < (transform.position - currentTeam.transform.position).magnitude)
                {
                    currentTeam = checkTeam;
                    hasTeam = true;
                }
            }
            if (hit.collider.GetComponent<WMStructure>() != null)
            {
                WMStructure checkStructure = hit.collider.GetComponent<WMStructure>();
                if (currentStructure == null
                    || (transform.position - checkStructure.transform.position).magnitude
                    < (transform.position - currentStructure.transform.position).magnitude)
                {
                    currentStructure = checkStructure;
                    hasStructure = true;
                }
            }
            if (currentTile != null)
            {
                //TODO set tile display
            }
            if (hasTeam)
            {
                //TODO set team display
            }
            else
            {
                currentTeam = null;
            }
            if (hasStructure)
            {
                //TODO set structure display
            }
            else
            {
                currentStructure = null;
            }
        }
    }

    public void selectTeam()
    {
        if (currentTeam.isClaimed())
        {
            //TODO play action fail sound
        }
        else
        {
            currentTeam.claim(true);
            selectedEntity = currentTeam;
        }
    }
    public void deselectTeam()
    {
        currentTeam.claim(false);
        selectedEntity = null;
    }
}
