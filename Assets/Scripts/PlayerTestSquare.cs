using Cinemachine;
using Network;
using Unity.Netcode;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerTestSquare : BaseSquareController
{
    private InputManager _inputManager;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private BasicShootController _basicShootController;
    


    public BasicShootController BasicShootController => _basicShootController;

    private float _xInput;
    private float _yInput;

    public Vector2 GetMovementInput()
    {
        return new Vector2(_xInput, _yInput);
    }

    public Vector2 GetLookInput()
    {
        return FindMousePosition();
    }

    private void Start()
    {
        Speed = _speed;

        _inputManager = InputManager.instance;
        
    }

    private void Update()
    {
        Speed = _speed;
        ManageButtonMovement();

        if (IsClient && IsLocalPlayer)
        {
            LookTowardsMousePos(FindMousePosition());
            ManageButtonMovement();
            //_movement.ProcessLocalPlayerMovement(new Vector2(_xInput, _yInput), FindMousePosition());
           // _xInput = 0;
           // _yInput = 0;
        }
        else
        {
            //_movement.ProcessSimulatedPlayerMovement();
        }
      // if (!IsOwner) return;
      // if (IsStunned) return;
      // 
      // LookTowardsMousePos(FindMousePosition());
      // ManageButtonMovement();
    }

    private Vector3 FindMousePosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        return mousePosition;
    }

    private void LookTowardsMousePos(Vector3 mousePosition)
    {
        LookTowards(mousePosition);
    }
    [ServerRpc]
    private void LookTowardsMousePosServerRpc(Vector3 mousePosition)
    {
        //Debug.Log($"Client {OwnerClientId} mousepos: {mousePosition}");
        LookTowards(mousePosition);
    }

    private void ManageButtonMovement()
    {
        if (_inputManager.GetKey(KeyBindAction.Up))
        {
            _yInput = 1;
        }
        else if (_inputManager.GetKey(KeyBindAction.Down))
        {
            _yInput = -1;
        }
        else
        {
            _yInput = 0;
        }

        if (_inputManager.GetKey(KeyBindAction.Left))
        {
            _xInput = -1;
        }
        else if (_inputManager.GetKey(KeyBindAction.Right))
        {
            _xInput = 1;
        }
        else
        {
            _xInput = 0;
        }

        //MoveServerRpc(_xInput, _yInput);
        //MoveClient(_xInput, _yInput);
        SetMovement(_xInput, _yInput);
        
        
        if (_inputManager.GetKeyDown(KeyBindAction.Shoot) && !IsStunned.Value && CanMoveAndShoot.Value)
        {
            _basicShootController.ShootRpc();
        }
    }
}