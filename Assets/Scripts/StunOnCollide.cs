using UnityEngine;

public class StunOnCollide : MonoBehaviour
{
    [SerializeField] private float forceMagnitude = 100f;
    [SerializeField] private float rotationAmount = 180f;
    [SerializeField] private float stunDuration = 1f;
    [SerializeField] private float rotationDuration = 1f;
    [SerializeField] private bool _isVertical;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        IStunnable stunnable = collision.gameObject.GetComponent<IStunnable>();

        if (stunnable != null)
        {
            Vector2 forceDirection = (Vector2) collision.transform.position - collision.GetContact(0).point;

            if (_isVertical)
                forceDirection.x = 0;
            else
                forceDirection.y = 0;

            Vector2 forceMagnitudeVector = forceMagnitude * forceDirection;
            
            stunnable.Stun(forceMagnitudeVector, rotationAmount, stunDuration, rotationDuration, false);
        }
    }
}