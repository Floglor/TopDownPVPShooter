using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkMovementControllerPhysics : NetworkBehaviour
    {
        private const int BUFFER_SIZE = 1024;
        private int _currentTick;
        private PlayerTestSquare _square;

        private TransformState[] _transformStateBuffer = new TransformState[BUFFER_SIZE];
        private InputState[] _inputStatesBuffer = new InputState[BUFFER_SIZE];

        private InputState _currentInput;

        public NetworkVariable<TransformState> ServerTransformState = new NetworkVariable<TransformState>();
        public NetworkVariable<InputState> ServerInputState = new NetworkVariable<InputState>();

        private void OnEnable()
        {
            _square = FindObjectOfType<PlayerTestSquare>();
            ServerInputState.OnValueChanged += OnServerInputStateChanged;
            ServerTransformState.OnValueChanged += OnServerTransformStateChanged;
        }

        public void StopMovement()
        {
            
        }

        private void OnServerTransformStateChanged(TransformState previousvalue, TransformState newvalue)
        {
            if (!IsOwner)
            {
                if (!IsClient) return;

                _square.Rb.position = newvalue.Position;
                _square.Rb.rotation = newvalue.Rotation;
                _square.Rb.velocity = newvalue.Velocity;
                _square.Rb.angularVelocity = newvalue.AngularVelocity;

                return;
            }

            int bufferSlot = newvalue.Tick % 1024;

            Vector3 positionError = _transformStateBuffer[bufferSlot].Position;

            if (positionError.sqrMagnitude > 0.0000001)
            {
                RewindMovement(newvalue);
            }
        }

        private void RewindMovement(TransformState newvalue)
        {
            int bufferSlot;
            Rigidbody2D rb = _square.GetComponent<Rigidbody2D>();
            rb.position = newvalue.Position;
            rb.rotation = newvalue.Rotation;
            rb.velocity = newvalue.Velocity;

            int rewindTick = newvalue.Tick;
            while (rewindTick < _currentTick)
            {
                bufferSlot = rewindTick % 1024;
                _inputStatesBuffer[bufferSlot] = _currentInput;
                _transformStateBuffer[bufferSlot].Position = _square.Rb.position;
                if (_transformStateBuffer == null)
                {
                    Debug.LogError("_transformStateBuffer is null");
                }

                if (_transformStateBuffer[bufferSlot] == null)
                {
                    Debug.LogError($"_transformStateBuffer[{bufferSlot}] is null");
                }

                _transformStateBuffer[bufferSlot].Rotation = _square.Rb.rotation;
                _square.MoveClient(_currentInput.MovementInput.x, _currentInput.MovementInput.y);

                Physics2D.Simulate(Time.deltaTime);
                ++rewindTick;
            }
        }

        private void OnServerInputStateChanged(InputState previousvalue, InputState newvalue)
        {
            if (!IsServer) return;

            _square.MoveClient(newvalue.MovementInput.x, newvalue.MovementInput.y);
            _square.LookTowards(newvalue.LookInput);

            Physics2D.Simulate(Time.fixedDeltaTime);

            TransformState transformState = new TransformState()
            {
                Position = _square.Rb.position,
                Rotation = _square.Rb.rotation,
                Velocity = _square.Rb.velocity,
                AngularVelocity = _square.Rb.angularVelocity,
                Tick = _currentTick++
            };

            ServerTransformState.Value = transformState;
        }


        private void Start()
        {
            for (int i = 0; i < _transformStateBuffer.Length; i++)
            {
                _transformStateBuffer[i] = new TransformState();
            }
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;

            InputState inputState = new InputState()
            {
                Tick = _currentTick,
                MovementInput = _square.GetMovementInput(),
                LookInput = _square.GetLookInput()
            };

            _currentInput = inputState;

            if (!IsServer)
            {
                SendInputToServerServerRpc(inputState);

                int bufferSlot = _currentTick % BUFFER_SIZE;
                _inputStatesBuffer[bufferSlot] = inputState;
                _transformStateBuffer[bufferSlot].Position = _square.Rb.position;
                _transformStateBuffer[bufferSlot].Rotation = _square.Rb.rotation;

                _square.MoveClient(inputState.MovementInput.x, inputState.MovementInput.y);
            }
            else
            {
                _square.MoveClient(inputState.MovementInput.x, inputState.MovementInput.y);
                _square.LookTowards(inputState.LookInput);

                TransformState state = new TransformState()
                {
                    Tick = _currentTick,
                    Position = _square.transform.position,
                    Rotation = _square.Rb.rotation,
                    AngularVelocity = _square.Rb.angularVelocity,
                    Velocity = _square.Rb.velocity,
                    HasStartedMoving = true
                };

                ServerTransformState.Value = state;
            }

            Physics2D.Simulate(Time.fixedDeltaTime);
            ++_currentTick;
        }

        [ServerRpc]
        private void SendInputToServerServerRpc(InputState inputState)
        {
            ServerInputState.Value = inputState;
        }
    }
}