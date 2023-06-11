using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkMovementController : NetworkBehaviour
    {
        [SerializeField] private BaseSquareController _square;

        private int _tick = 0;
        private float _tickRate = 1f / 60f;
        private float _tickDeltaTime = 0f;

        private const int BUFFER_SIZE = 1024;
        private InputState[] _inputStates = new InputState[BUFFER_SIZE];
        private TransformState[] _transformStates = new TransformState[BUFFER_SIZE];

        public NetworkVariable<TransformState> ServerTransformState = new NetworkVariable<TransformState>();
        private TransformState _previousTransformState;

        private void OnEnable()
        {
            ServerTransformState.OnValueChanged += OnServerStateChanged;
        }

        private void OnServerStateChanged(TransformState previousvalue, TransformState newvalue)
        {
            _previousTransformState = previousvalue;
        }

        public void ProcessLocalPlayerMovement(Vector2 movementInput, Vector3 lookInput)
        {
            _tickDeltaTime += Time.deltaTime;
            if (_tickDeltaTime > _tickRate)
            {
                int bufferIndex = _tick % BUFFER_SIZE;

                if (!IsServer)
                {
                    MoveServerRpc(_tick, movementInput, lookInput);
                    _square.MoveClient(movementInput.x, movementInput.y);
                    _square.LookTowards(lookInput);

                }
                else
                {
                    _square.MoveClient(movementInput.x, movementInput.y);
                    _square.LookTowards(lookInput);

                    TransformState state = new TransformState()
                    {
                        Tick = _tick,
                        Position = _square.transform.position,
                        Rotation = _square.transform.rotation,
                        HasStartedMoving = true
                    };

                    _previousTransformState = ServerTransformState.Value;
                    ServerTransformState.Value = state;

                }

                InputState inputState = new InputState()
                {
                    Tick = _tick,
                    movementInput = movementInput,
                    lookInput = lookInput
                };
                
                TransformState transformState = new TransformState()
                {
                    Tick = _tick,
                    Position = _square.transform.position,
                    Rotation = _square.transform.rotation,
                    HasStartedMoving = true
                };

                _inputStates[bufferIndex] = inputState;
                _transformStates[bufferIndex] = transformState;

                _tickDeltaTime -= _tickRate;
                _tick++;
            }
        }

        public void ProcessSimulatedPlayerMovement()
        {
            _tickDeltaTime += Time.deltaTime;
            if (_tickDeltaTime > _tickRate)
            {
                if (ServerTransformState.Value.HasStartedMoving)
                {
                    _square.transform.position = ServerTransformState.Value.Position;
                    _square.transform.rotation = ServerTransformState.Value.Rotation;
                }
                
                _tickDeltaTime -= _tickRate;
                _tick++;
            }
        }


        [ServerRpc]
        private void MoveServerRpc(int tick, Vector2 movementInput, Vector3 lookInput)
        {
            _square.MoveClient(movementInput.x, movementInput.y);
            _square.LookTowards(lookInput);

            TransformState state = new TransformState()
            {
                Tick = tick,
                Position = _square.transform.position,
                Rotation = _square.transform.rotation,
                HasStartedMoving = true
            };

            _previousTransformState = ServerTransformState.Value;
            ServerTransformState.Value = state;

        }
    }
}