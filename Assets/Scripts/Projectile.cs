using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float forceMagnitude = 100f;
    [SerializeField] private float rotationAmount = 180f;
    [SerializeField] private float stunDuration = 1f;
    [SerializeField] private float rotationDuration = 1f;

    private BasicShootController _owner;

    public BasicShootController Owner
    {
        get => _owner;
        set => _owner = value;
    }

    private bool _isDespawned;


    private Vector3 _direction;

    private void OnEnable()
    {
        _isDespawned = false;
    }

    public void SetVector(Vector3 vector)
    {
        _direction = vector.normalized;

        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void FixedUpdate()
    {
        if (IsServer || IsHost)
            Move();
    }

    private void Move()
    {
        transform.position += transform.right * speed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsServer || !IsSpawned) return;
        
        if (col.GetComponent<Collider2D>().CompareTag("Wall"))
        {
            DespawnServerRpc();
            return;
        }

        if (col.GetComponent<Collider2D>().GetComponent<PlayerTestSquare>().BasicShootController == Owner) return;
        
        StunEnemy(col);
        DespawnServerRpc();

    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!IsServer) return;
        
        if (col.collider.GetComponent<PlayerTestSquare>().BasicShootController != Owner)
        {
            StunEnemy(col);
            DespawnServerRpc();
        }
        else if (col.collider.CompareTag("Wall"))
        {
            DespawnServerRpc();
        }
    }

    [ServerRpc]
    private void DespawnServerRpc()
    {
        if (!IsServer) return;

        if (_isDespawned) return;
        //gameObject.SetActive(false);
        NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
        networkObject.Despawn();
        _isDespawned = true;
        Owner = null;
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
    
    private void StunEnemy(Collider2D col)
    {
        IStunnable stunnable = col.GetComponent<IStunnable>();
        Vector2 forceMagnitudeVector = forceMagnitude * _direction;

        if (stunnable != null)
        {
            stunnable.Stun(forceMagnitudeVector, rotationAmount, stunDuration, rotationDuration, true);
        }
    }
}