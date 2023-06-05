using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pool : MonoBehaviour
{
    private static readonly Dictionary<PooledMonobehaviour, Pool> _pools =
        new Dictionary<PooledMonobehaviour, Pool>();

    private readonly List<PooledMonobehaviour> _disabledObjects = new List<PooledMonobehaviour>();

    private readonly Queue<PooledMonobehaviour> _objects = new Queue<PooledMonobehaviour>();

    private PooledMonobehaviour _prefab;

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


    private void AddObjectToAvailable(PooledMonobehaviour pooledObject)
    {
        _disabledObjects.Add(pooledObject);
        _objects.Enqueue(pooledObject);
    }

    private void GrowPool()
    {
        for (int i = 0; i < _prefab.InitialPoolSize; i++)
        {
            PooledMonobehaviour pooledObject = Instantiate(_prefab);
            pooledObject.gameObject.name += " " + i;

            pooledObject.OnDestroyEvent += () => AddObjectToAvailable(pooledObject);

            pooledObject.gameObject.SetActive(false);
        }
    }

    public T Get<T>() where T : PooledMonobehaviour
    {
        if (_objects.Count == 0) GrowPool();

        PooledMonobehaviour pooledObject = _objects.Dequeue();
        if (pooledObject == null) return null;
        return pooledObject as T;
    }

    private void MakeDisabledObjectsChildren()
    {
        if (_disabledObjects.Count <= 0) return;
        foreach (PooledMonobehaviour pooledObject in _disabledObjects.Where(pooledObject =>
                     pooledObject.gameObject.activeInHierarchy == false))
            pooledObject.transform.SetParent(transform);

        _disabledObjects.Clear();
    }

    public static Pool GetPool(PooledMonobehaviour prefab)
    {
        if (_pools.ContainsKey(prefab))
            return _pools[prefab];

        // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
        Pool pool = new GameObject("Pool-" + prefab.name).AddComponent<Pool>();
        pool._prefab = prefab;

        pool.GrowPool();
        _pools.Add(prefab, pool);
        return pool;
    }
}