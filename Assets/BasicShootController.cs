using UnityEngine;

public class BasicShootController : MonoBehaviour, IHavePooledObject
{
    [SerializeField] private PooledNetworkMonoBehavior  _basicProjectile;
    [SerializeField] private float _spawnCooldown = 0.5f;

    private float _lastSpawnTime = -Mathf.Infinity;
    

    public void Shoot()
    {
        if (!(Time.time - _lastSpawnTime >= _spawnCooldown)) return;
        
        PooledNetworkMonoBehavior  projectileObject = _basicProjectile.Get<PooledNetworkMonoBehavior>();
        projectileObject.transform.position = transform.position;
        Projectile projectile = projectileObject.GetComponent<Projectile>();

        projectile.SetVector(transform.right.normalized);
        
        _lastSpawnTime = Time.time;
    }

    public PooledNetworkMonoBehavior GetPooledPrefab()
    {
        return _basicProjectile;
    }
}

public interface IHavePooledObject
{
    public PooledNetworkMonoBehavior GetPooledPrefab();
}
