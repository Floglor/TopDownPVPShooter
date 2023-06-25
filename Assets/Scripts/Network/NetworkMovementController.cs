using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkMovementController : NetworkBehaviour
    {
        [SerializeField] private BaseSquareController _square;
        [SerializeField] private Color _gizmoColor;


        private int _tick = 0;
        private float _tickRate = 1f / 60f;
        private float _tickDeltaTime = 0f;

        private const int BUFFER_SIZE = 1024;
        private InputState[] _inputStates = new InputState[BUFFER_SIZE];
        private TransformState[] _transformStates = new TransformState[BUFFER_SIZE];

        private int _lastProcessedTick = 0;

        public NetworkVariable<TransformState> ServerTransformState = new NetworkVariable<TransformState>();
        private TransformState _previousTransformState;
        private string _latestPositionDifference;

        private void OnEnable()
        {
            Physics2D.simulationMode = SimulationMode2D.Script;
            ServerTransformState.OnValueChanged += OnServerStateChanged;
            StartLoggingFunctionCalls();
        }

        private int _functionCalls = 0;

        private void OnServerStateChanged(TransformState previousState, TransformState serverState)
        {
            if (!IsLocalPlayer) return;


            if (_previousTransformState == null)
            {
                _previousTransformState = serverState;
            }

            TransformState calculatedState = _transformStates.First(localState => localState.Tick == serverState.Tick);

            Vector3 positionError = calculatedState.Position - serverState.Position;

            if (positionError.sqrMagnitude > 0.0000001f)
            {
                SnapBackToServerPosition(serverState);
                ReplayInputs(serverState);
            }
        }

        private int numberOfLoggedInputs = 10;

        private void StartLoggingFunctionCalls()
        {
            StartCoroutine(LogFunctionCallsPerSecond());
        }

        private IEnumerator LogFunctionCallsPerSecond()
        {
            while (true)
            {
                yield return new WaitForSeconds(10f);

                float callsPerSecond = _functionCalls / 10f; // Calculate calls per second
                Debug.Log($"Snapback {callsPerSecond:f2} calls per 10 seconds, tick: {_tick}");

                _functionCalls = 0; // Reset the counter
            }
        }


        private void ReplayInputs(TransformState serverState)
        {
            IEnumerable<InputState> inputs = _inputStates.Where(input => input.Tick > serverState.Tick);
            inputs = from inputState in inputs orderby inputState.Tick select inputState;

            foreach (InputState inputState in _inputStates)
            {
                if (inputState == null) return;
                //MoveServerRpc(_tick, inputState.movementInput, inputState.lookInput);
                _square.MoveClientWithTime(inputState.movementInput.x, inputState.movementInput.y, _tickRate);
                _square.LookTowards(inputState.lookInput);

                TransformState transformState = new TransformState()
                {
                    Tick = inputState.Tick,
                    Position = _square.transform.position,
                    Rotation = _square.transform.rotation,
                    AngularVelocity = _square.Rb.angularVelocity,
                    Velocity = _square.Rb.velocity,
                    HasStartedMoving = true
                };

                for (int i = 0; i < _transformStates.Length; i++)
                {
                    if (_transformStates[i].Tick == inputState.Tick)
                    {
                        _transformStates[i] = transformState;
                        break;
                    }
                }

                //Physics2D.Simulate(_tickDeltaTime);
            }
        }

        private void SnapBackToServerPosition(TransformState state)
        {
            _square.transform.position = state.Position;
            _square.transform.rotation = state.Rotation;
            _square.Rb.velocity = state.Velocity;
            _square.Rb.angularVelocity = state.AngularVelocity;

            for (int i = 0; i < _transformStates.Length; i++)
            {
                if (_transformStates[i].Tick != state.Tick) continue;

                _transformStates[i] = state;
                break;
            }

            _functionCalls++;
        }

        public void ProcessLocalPlayerMovement(Vector2 movementInput, Vector3 lookInput)
        {
            _tickDeltaTime += Time.deltaTime;
            if (_tickDeltaTime > _tickRate)
            {
                int bufferIndex = _tick % BUFFER_SIZE;

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
                    AngularVelocity = _square.Rb.angularVelocity,
                    Velocity = _square.Rb.velocity,
                    HasStartedMoving = true
                };

                _inputStates[bufferIndex] = inputState;
                _transformStates[bufferIndex] = transformState;


                if (!IsServer)
                {
                    if (IsOwner)
                    {
                        //Debug.Log($"Client MovementInput: {movementInput}, {lookInput}, tick: {_tick}");
                    }

                    MoveServerRpc(_tick, movementInput, lookInput);
                    //MoveTransformServerRpc(_tick, movementInput, lookInput);
                    _square.MoveClientWithTime(movementInput.x, movementInput.y, _tickRate);
                    _square.LookTowards(lookInput);
                }
                else
                {
                    _square.MoveClientWithTime(movementInput.x, movementInput.y, _tickRate);
                    _square.LookTowards(lookInput);

                    TransformState state = new TransformState()
                    {
                        Tick = _tick,
                        Position = _square.transform.position,
                        Rotation = _square.transform.rotation,
                        AngularVelocity = _square.Rb.angularVelocity,
                        Velocity = _square.Rb.velocity,
                        HasStartedMoving = true
                    };

                    _previousTransformState = ServerTransformState.Value;
                    ServerTransformState.Value = state;
                }


                Physics2D.Simulate(_tickRate);
                _tickDeltaTime -= _tickRate;
                _tick++;
            }
        }

        public void ProcessSimulatedPlayerMovement()
        {
            _tickDeltaTime += Time.deltaTime;
            if (ServerTransformState.Value == null) return;

            if (_tickDeltaTime > _tickRate)
            {
                if (ServerTransformState.Value.HasStartedMoving)
                {
                    _square.transform.position = ServerTransformState.Value.Position;
                    _square.transform.rotation = ServerTransformState.Value.Rotation;
                    _square.Rb.angularVelocity = ServerTransformState.Value.AngularVelocity;
                    _square.Rb.velocity = ServerTransformState.Value.Velocity;
                }

                _tickDeltaTime -= _tickRate;
                _tick++;
                Physics2D.Simulate(_tickRate);
            }
        }


        [ServerRpc]
        private void MoveServerRpc(int tick, Vector2 movementInput, Vector3 lookInput)
        {
            //Debug.Log($"Client on server MovementInput: {movementInput}, {lookInput}, ServerTick: {_tick}, ClientTick: {tick}, tickDifference = {tick - _lastProcessedTick}");

            _lastProcessedTick = tick;

            _square.MoveClientWithTime(movementInput.x, movementInput.y, _tickRate);
            _square.LookTowards(lookInput);

            TransformState state = new TransformState()
            {
                Tick = tick,
                Position = _square.transform.position,
                Rotation = _square.transform.rotation,
                AngularVelocity = _square.Rb.angularVelocity,
                Velocity = _square.Rb.velocity,
                HasStartedMoving = true
            };

            _previousTransformState = ServerTransformState.Value;
            ServerTransformState.Value = state;
        }

        [ServerRpc]
        private void MoveTransformServerRpc(int tick, Vector2 movementInput, Vector3 lookInput)
        {
            _square.MoveClientWithTimeTransform(movementInput.x, movementInput.y, _tickRate);
            _square.LookTowards(lookInput);

            TransformState state = new TransformState()
            {
                Tick = tick,
                Position = _square.transform.position,
                Rotation = _square.transform.rotation,
                AngularVelocity = _square.Rb.angularVelocity,
                Velocity = _square.Rb.velocity,
                HasStartedMoving = true
            };

            _previousTransformState = ServerTransformState.Value;
            ServerTransformState.Value = state;
        }

        private void OnDrawGizmos()
        {
            if (ServerTransformState.Value != null)
            {
                Gizmos.color = _gizmoColor;
                Gizmos.DrawCube(ServerTransformState.Value.Position, new Vector3(1, 1, 0));
            }
        }
    }
}