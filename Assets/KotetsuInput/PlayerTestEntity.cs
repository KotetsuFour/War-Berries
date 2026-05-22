using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTestEntity : PlayerEntity
{
    public void setSelectionMethod()
    {
        if (!StaticData.WMPaused)
        {

        }
    }
    public override void detatchPlayer()
    {

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
}
