using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Pool : NetworkBehaviour
{
    private static readonly Dictionary<PooledNetworkMonoBehavior, Pool> _pools =
        new Dictionary<PooledNetworkMonoBehavior, Pool>();

    private readonly List<PooledNetworkMonoBehavior> _disabledObjects = new List<PooledNetworkMonoBehavior>();

    private readonly Queue<PooledNetworkMonoBehavior> _objects = new Queue<PooledNetworkMonoBehavior>();

    private PooledNetworkMonoBehavior _prefab;

    private void Update()
    {
        MakeDisabledObjectsChildren();
    }

    private void OnDestroy()
    {
        _pools.Clear();
        _disabledObjects.Clear();
        _objects.Clear();
    }

    public void SetPrefab(PooledNetworkMonoBehavior prefab)
    {
        _prefab = prefab;
    }

    private void AddObjectToAvailable(PooledNetworkMonoBehavior pooledObject)
    {
        _disabledObjects.Add(pooledObject);
        _objects.Enqueue(pooledObject);
    }

    public void GrowPool()
    {
        for (int i = 0; i < _prefab.InitialPoolSize; i++)
        {
            PooledNetworkMonoBehavior pooledObject = Instantiate(_prefab);
            pooledObject.GetComponent<NetworkObject>().Spawn(true);
            pooledObject.gameObject.name += " " + i;

            pooledObject.OnDestroyEvent += () => AddObjectToAvailable(pooledObject);

            pooledObject.gameObject.SetActive(false);
             
        }
    }

    public T Get<T>() where T : PooledNetworkMonoBehavior
    {
        if (_objects.Count == 0) GrowPool();

        PooledNetworkMonoBehavior pooledObject = _objects.Dequeue();
        if (pooledObject == null) return null;
        return pooledObject as T;
    }

    private void MakeDisabledObjectsChildren()
    {
        if (_disabledObjects.Count <= 0) return;
        foreach (PooledNetworkMonoBehavior pooledObject in _disabledObjects.Where(pooledObject =>
                     pooledObject.gameObject.activeInHierarchy == false))
        {
            pooledObject.transform.SetParent(transform);
            pooledObject.GetComponent<NetworkObject>().TrySetParent(transform);
        }
        

        _disabledObjects.Clear();
    }

    public static Pool GetPool(PooledNetworkMonoBehavior prefab)
    {
        if (_pools.ContainsKey(prefab))
            return _pools[prefab];

        Pool pool = GameObject.Find("Pool-" + prefab.name).GetComponent<Pool>();
        _pools.Add(prefab, pool);
        return pool;
    }
}