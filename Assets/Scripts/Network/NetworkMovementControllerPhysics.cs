using System;
using Unity.Netcode;

namespace Network
{
    public class NetworkMovementControllerPhysics : NetworkBehaviour
    {
        private uint _currentTick;

        private TransformState[] _transformStateBuffer = new TransformState[1024];
        private InputState[] _inputStatesBuffer = new InputState[1024];

        private InputState _currentInput;

        private void Start()
        {
            for (int i = 0; i < _transformStateBuffer.Length; i++)
            {
                _transformStateBuffer[i] = new TransformState();
            }
        }

        private void FixedUpdate()
        {
            //     InputState inputState = new InputState(){
            //         
            // }
        }
    }
}