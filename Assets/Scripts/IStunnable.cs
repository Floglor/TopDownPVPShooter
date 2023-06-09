using UnityEngine;

public interface IStunnable
{
    public void Stun(Vector2 force, float rotation, float duration, float rotationDuration, bool fromPlayer);
}