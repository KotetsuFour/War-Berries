using UnityEngine;
using UnityEngine.InputSystem;
public class WMStructure : WMEntity
{
    public override bool receiveOrder(Collider cursor, Transform display)
    {
        //The player pressed the select button do whatever that means for this specific team
        //and update the display accordingly.
        //If no further action can be done, return false. Otherwise, return true.
        return false;
    }
    public override bool retractOrder(Collider cursor, Transform display)
    {
        //The player pressed the back button do whatever that means for this specific team
        //and update the display accordingly.
        //If no further action can be done, return false. Otherwise, return true.
        return false;
    }
    public override bool up(Collider cursor, Transform display, InputAction.CallbackContext ctx)
    {
        //The player has given a directional input for a menu. If there is no menu currently
        //active, return false to notify the player object to move on the map instead
        return false;
    }
    public override bool down(Collider cursor, Transform display, InputAction.CallbackContext ctx)
    {
        //The player has given a directional input for a menu. If there is no menu currently
        //active, return false to notify the player object to move on the map instead
        return false;
    }
    public override bool right(Collider cursor, Transform display, InputAction.CallbackContext ctx)
    {
        //The player has given a directional input for a menu. If there is no menu currently
        //active, return false to notify the player object to move on the map instead
        return false;
    }
    public override bool left(Collider cursor, Transform display, InputAction.CallbackContext ctx)
    {
        //The player has given a directional input for a menu. If there is no menu currently
        //active, return false to notify the player object to move on the map instead
        return false;
    }

}
