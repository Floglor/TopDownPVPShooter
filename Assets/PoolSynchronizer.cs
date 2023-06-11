using Extensions;
using Unity.Netcode;
using UnityEngine;

public class PoolSynchronizer : NetworkBehaviour
{
    [SerializeField] private UnityEngine.GameObject _poolPrefab;
    
    private void Start()
    {
        if (!IsServer && !IsHost || !IsOwner)
            return;
        
        IHavePooledObject pooledInterface = gameObject.GetInterface<IHavePooledObject>();
        
        if (pooledInterface != null)
            CreatePool(pooledInterface.GetPooledPrefab());
        else
        {
            Debug.LogError("PoolSynchronizer couldn't find pooled prefab");
        }
    }
    
    
    public Pool CreatePool(GameObject prefab)
    {
        //if server then spawn and if not just lmao search for the name I guess
       // if (!IsServer && !IsHost)
       //     return null;


        // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
        Pool pool = Instantiate(_poolPrefab, Vector3.zero, Quaternion.identity).GetComponent<Pool>();
        pool.gameObject.name = "Pool-" + prefab.name;
        pool.SetPrefab(prefab);
        pool.GrowPool();

        NetworkObject networkObject = pool.GetComponent<NetworkObject>();
        networkObject.name = "Pool-" + prefab.name;
        networkObject.SpawnWithOwnership(0);

        return pool;
    }
}