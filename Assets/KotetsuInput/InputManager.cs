using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Player playerPrefab;
    [SerializeField] private int maxPlayers;
    [SerializeField] private PlayerTestEntity testEntity;
    [SerializeField] private WMCursor wmCursor;
    [SerializeField] private bool testing;
    private List<Player> players;
    private Dictionary<int, Player> playerMap;
    private KotetsuInputs controls;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        controls = new KotetsuInputs();
        players = new List<Player>();
        playerMap = new Dictionary<int, Player>();
        controls.Player.PLUS.performed += whatever => plus(whatever);
    }

    private void plus(InputAction.CallbackContext ctx)
    {
        InputDevice device = ctx.control.device;
        if (players.Count >= maxPlayers || playerMap.ContainsKey(device.deviceId))
        {
            return;
        }
        PlayerInput input = PlayerInput.Instantiate(
            playerPrefab.gameObject,
            controlScheme: device is Joystick ? $"NSWPlayer{players.Count + 1}"
            : device is SwitchProControllerHID ? "Switch"
            : null,
            pairWithDevice: ctx.control.device
            );
        Player play = input.GetComponent<Player>();
        play.setPlayerData(players.Count, device);
        players.Add(play);
        playerMap.Add(device.deviceId, play);
        if (testing)
        {
            play.setCharacter(wmCursor, Vector3.zero, Quaternion.identity, true);
        }
    }
    public void setMaxPlayers(int max)
    {
        maxPlayers = max;
    }
    private void OnEnable()
    {
        controls.Enable();
    }
    private void OnDisable()
    {
        controls.Disable();
    }
    /*
    public void playerJoined(PlayerInput input)
    {
        if (testing)
        {
            input.GetComponent<Player>().setCharacter(testEntity, Vector3.zero, Quaternion.identity, true);
        }
    }
    public void playerLeft(PlayerInput input)
    {
    }
    */
}
