using UnityEngine;
using UnityEngine.InputSystem;
public abstract class WMEntity : MonoBehaviour
{
    private bool claimed;

    public void claim(bool status)
    {
        claimed = status;
    }
    public bool isClaimed()
    {
        return claimed;
    }
    public abstract bool receiveOrder(Collider cursor, Transform display);
    public abstract bool retractOrder(Collider cursor, Transform display);
    public abstract bool up(Collider cursor, Transform display, InputAction.CallbackContext ctx);
    public abstract bool down(Collider cursor, Transform display, InputAction.CallbackContext ctx);
    public abstract bool right(Collider cursor, Transform display, InputAction.CallbackContext ctx);
    public abstract bool left(Collider cursor, Transform display, InputAction.CallbackContext ctx);
}
