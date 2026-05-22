using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    private int playerId;
    private PlayerEntity controlledCharacter;
    private InputDevice input;
    public void setPlayerData(int playerId, InputDevice input)
    {
        this.playerId = playerId;
        this.input = input;
        gameObject.name = $"Player{playerId}({input.GetType()})";
    }
    public void setCharacter(PlayerEntity entityPrefab,
        Vector3 startPos, Quaternion startRot,
        bool destroyOld)
    {
        if (destroyOld)
        {
            Destroy(controlledCharacter);
        }
        controlledCharacter = Instantiate(entityPrefab, startPos, startRot);
        controlledCharacter.attachPlayer(GetComponent<PlayerInput>(), input);
    }
    public int getId()
    {
        return playerId;
    }

    public void aButton(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.aButton(ctx);
        }
    }
    public void bButton(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.bButton(ctx);
        }
    }
    public void xButton(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.xButton(ctx);
        }
    }
    public void yButton(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.yButton(ctx);
        }
    }

    public void upButton(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.upButton(ctx);
        }
    }
    public void downButton(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.downButton(ctx);
        }
    }
    public void rightButton(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.rightButton(ctx);
        }
    }
    public void leftButton(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.leftButton(ctx);
        }
    }
    public void home(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.home(ctx);
        }
    }
    public void plus(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.plus(ctx);
        }
    }
    public void minus(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.minus(ctx);
        }
    }
    public void extra(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.extra(ctx);
        }
    }

    public void r1(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.r1(ctx);
        }
    }
    public void r2(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.r2(ctx);
        }
    }
    public void l1(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.aButton(ctx);
        }
    }
    public void l2(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.l1(ctx);
        }
    }

    public void lPress(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.lPress(ctx);
        }
    }
    public void rPress(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.rPress(ctx);
        }
    }
    public void rightMove(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.rightMove(ctx);
        }
    }
    public void leftMove(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.leftMove(ctx);
        }
    }
    public void mouseLeft(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.mouseLeft(ctx);
        }
    }
    public void mouseRight(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.mouseRight(ctx);
        }
    }
    public void mouseMiddle(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.mouseMiddle(ctx);
        }
    }
    public void mouseScroll(InputAction.CallbackContext ctx)
    {
        if (controlledCharacter != null)
        {
            controlledCharacter.mouseScroll(ctx);
        }
    }
    public void lostConnection(PlayerInput input)
    {
        Debug.Log("Controller lost connection");
    }
    public void regainedConnection(PlayerInput input)
    {
        Debug.Log("Controller regained connection");
    }
    public void changedConnection(PlayerInput input)
    {
        Debug.Log("Controller changed");
    }
}
