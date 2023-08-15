using System;
using Unity.Netcode;
using UnityEngine;

public class BasicShootController : NetworkBehaviour
{
    [SerializeField] private UnityEngine.GameObject  _basicProjectile;
    [SerializeField] private float _spawnCooldown = 0.5f;
    

    private float _lastSpawnTime = -Mathf.Infinity;

    

    public void Shoot()
    {
        //if (IsServer || IsHost)
        //{
            if (!(Time.time - _lastSpawnTime >= _spawnCooldown)) return;

            NetworkObject projectileObject =
                NetworkObjectPool.Singleton.GetNetworkObject(_basicProjectile, transform.position, Quaternion.identity);

            //SpawnServerRpc(projectileObject);
            
            projectileObject.transform.position = transform.position;
            Projectile projectile = projectileObject.GetComponent<Projectile>();

            projectile.SetVector(transform.right.normalized);

            _lastSpawnTime = Time.time;
       // }
    }
    
    [ServerRpc]
    public void SpawnProjectileServerRpc(Vector3 position, Quaternion rotation, Vector3 right)
    {
        NetworkObject projectileObject =
            NetworkObjectPool.Singleton.GetNetworkObject(_basicProjectile, position, rotation);

        projectileObject.transform.position = transform.position;
        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Owner = this;
        projectile.SetVector(right.normalized);
        
        projectileObject.Spawn();
    }

    public void ShootRpc()
    {
        if (!IsOwner) return;
        
        if (!(Time.time - _lastSpawnTime >= _spawnCooldown)) return;

        SpawnProjectileServerRpc(transform.position, Quaternion.identity, transform.right);
        

        _lastSpawnTime = Time.time;
    }
    
}

public interface IHavePooledObject
{
    public GameObject GetPooledPrefab();
}
