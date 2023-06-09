using UnityEngine;

public class Projectile : PooledNetworkMonoBehavior
{
    [SerializeField] private float speed = 10f; 
    [SerializeField] private float forceMagnitude = 100f;
    [SerializeField] private float rotationAmount = 180f;
    [SerializeField] private float stunDuration = 1f;
    [SerializeField] private float rotationDuration = 1f;
        
    private Vector3 _direction;

    public void SetVector(Vector3 vector)
    {
        _direction = vector.normalized;
        
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void FixedUpdate()
    {
        transform.position += transform.right * speed * Time.fixedDeltaTime;

    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Enemy"))
        {
            StunEnemy(col);
        }
        else if (col.collider.CompareTag("Wall"))
        {
            gameObject.SetActive(false);
        }
    }

    private void StunEnemy(Collision2D col)
    {
        IStunnable stunnable = col.collider.GetComponent<IStunnable>();
        Vector2 forceMagnitudeVector = forceMagnitude * _direction;

        if (stunnable != null)
        {
            stunnable.Stun(forceMagnitudeVector, rotationAmount, stunDuration, rotationDuration, true);
        }
    }
}