using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Utility;

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
        private const float LERP_TIME = 0.01f;
        private PositionLerper _positionLerper;


        public NetworkVariable<TransformState> ServerTransformState = new NetworkVariable<TransformState>();
        public NetworkVariable<InputState> ServerInputState = new NetworkVariable<InputState>();
        
        private List<Vector3> positionErrors = new List<Vector3>();

        
        private void OnEnable()
        {
            _square = FindObjectOfType<PlayerTestSquare>();
            ServerInputState.OnValueChanged += OnServerInputStateChanged;
            ServerTransformState.OnValueChanged += OnServerTransformStateChanged;
            StartCoroutine(LogCounter());
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _positionLerper = new PositionLerper(transform.position, LERP_TIME);
        }


        private void OnServerTransformStateChanged(TransformState previousvalue, TransformState newvalue)
        {
            if (!IsOwner)
            {
                if (!IsClient) return;

                _square.Rb.position = newvalue.Position;
                //_square.Rb.position = _positionLerper.LerpPosition(_square.Rb.position, newvalue.Position);
                _square.Rb.rotation = newvalue.Rotation;
                _square.Rb.velocity = newvalue.Velocity;
                _square.Rb.angularVelocity = newvalue.AngularVelocity;

                return;
            }

            int bufferSlot = newvalue.Tick % 1024;

            Vector3 positionError = _transformStateBuffer[bufferSlot].Position - newvalue.Position;
            float rotationError = _transformStateBuffer[bufferSlot].Rotation - newvalue.Rotation;

            if (positionError.sqrMagnitude > 0.0000001)
            {
                positionErrors.Add(positionError);
                RewindMovement(newvalue);
            }
        }

        private int counter = 0;
        private void RewindMovement(TransformState newvalue)
        {
            counter++;
            int bufferSlot;
            Rigidbody2D rb = _square.GetComponent<Rigidbody2D>();
           //rb.position = newvalue.Position;
           rb.position = _positionLerper.LerpPosition(rb.position, newvalue.Position);
            //rb.rotation = newvalue.Rotation;
            rb.velocity = newvalue.Velocity;
            rb.angularVelocity = newvalue.AngularVelocity;

            int rewindTick = newvalue.Tick;
            while (rewindTick < _currentTick)
            {
                bufferSlot = rewindTick % 1024;
                //_inputStatesBuffer[bufferSlot] = _currentInput;
                _transformStateBuffer[bufferSlot].Position = _square.Rb.position;
                _transformStateBuffer[bufferSlot].Rotation = _square.Rb.rotation;
                
                //player_client.MovePlaye(player_rigidbody, this.client_input_buffer[buffer_slot]);
                _square.MoveClient(_inputStatesBuffer[bufferSlot].MovementInput.x, _inputStatesBuffer[bufferSlot].MovementInput.y);

                Physics2D.Simulate(Time.deltaTime);
                ++rewindTick;
            }
        }

        private int countTimer = 10;
        private IEnumerator LogCounter()
        {
            while (true)
            {
                Debug.Log($"RewindMoment count per {countTimer} seconds: {counter}");

               //foreach (Vector3 positionError in positionErrors)
               //{
               //    Debug.Log(positionError);
               //}

                positionErrors.Clear();
                counter = 0;
                yield return new WaitForSeconds(countTimer);
            }
        }

        private void OnServerInputStateChanged(InputState previousvalue, InputState newvalue)
        {
            if (!IsServer) return;

            _square.MoveClient(newvalue.MovementInput.x, newvalue.MovementInput.y);
            
            if (!IsOwner)
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