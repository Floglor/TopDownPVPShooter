using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class TransformState : INetworkSerializable
    {
        public int Tick;
        public Vector3 Position;
        public float Rotation;
        //public float AngularVelocity;
        public Vector2 Velocity;
        public bool HasStartedMoving;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                FastBufferReader reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Tick);
                reader.ReadValueSafe(out Position);
                reader.ReadValueSafe(out Rotation);
                reader.ReadValueSafe(out HasStartedMoving);
               // reader.ReadValueSafe(out AngularVelocity);
                reader.ReadValueSafe(out Velocity);

            }
            else
            {
                FastBufferWriter writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Tick);
                writer.WriteValueSafe(Position);
                writer.WriteValueSafe(Rotation);
                writer.WriteValueSafe(HasStartedMoving);
              //  writer.WriteValueSafe(AngularVelocity);
                writer.WriteValueSafe(Velocity);

            }
        }
    }
}