using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerEntity : MonoBehaviour
{
    public InputDevice inputDevice;
    public PlayerInput playerInput;
    public virtual void attachPlayer(PlayerInput playerInput, InputDevice inputDevice)
    {
        this.playerInput = playerInput;
        this.inputDevice = inputDevice;
    }
    public abstract void detatchPlayer();
    public abstract void aButton(InputAction.CallbackContext ctx);
    public abstract void bButton(InputAction.CallbackContext ctx);
    public abstract void xButton(InputAction.CallbackContext ctx);
    public abstract void yButton(InputAction.CallbackContext ctx);
    public abstract void upButton(InputAction.CallbackContext ctx);
    public abstract void downButton(InputAction.CallbackContext ctx);
    public abstract void rightButton(InputAction.CallbackContext ctx);
    public abstract void leftButton(InputAction.CallbackContext ctx);
    public abstract void home(InputAction.CallbackContext ctx);
    public abstract void plus(InputAction.CallbackContext ctx);
    public abstract void minus(InputAction.CallbackContext ctx);
    public abstract void extra(InputAction.CallbackContext ctx);
    public abstract void r1(InputAction.CallbackContext ctx);
    public abstract void r2(InputAction.CallbackContext ctx);
    public abstract void l1(InputAction.CallbackContext ctx);
    public abstract void l2(InputAction.CallbackContext ctx);
    public abstract void lPress(InputAction.CallbackContext ctx);
    public abstract void rPress(InputAction.CallbackContext ctx);
    public abstract void rightMove(InputAction.CallbackContext ctx);
    public abstract void leftMove(InputAction.CallbackContext ctx);
    public abstract void mouseLeft(InputAction.CallbackContext ctx);
    public abstract void mouseRight(InputAction.CallbackContext ctx);
    public abstract void mouseMiddle(InputAction.CallbackContext ctx);
    public abstract void mouseScroll(InputAction.CallbackContext ctx);
}
