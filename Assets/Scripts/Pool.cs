using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class Pool : NetworkBehaviour
{
    private static readonly Dictionary<GameObject, Pool> _pools =
        new Dictionary<GameObject, Pool>();

    private readonly List<GameObject> _disabledObjects = new List<GameObject>();

    private readonly Queue<GameObject> _objects = new Queue<GameObject>();

    private GameObject _prefab;

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

    public void SetPrefab(GameObject prefab)
    {
        _prefab = prefab;
    }

    private void AddObjectToAvailable(GameObject pooledObject)
    {
        _disabledObjects.Add(pooledObject);
        _objects.Enqueue(pooledObject);
    }

    public void GrowPool()
    {
        for (int i = 0; i < _prefab.InitialPoolSize; i++)
        {
            GameObject pooledObject = Instantiate(_prefab);
            pooledObject.GetComponent<NetworkObject>().Spawn(true);
            pooledObject.gameObject.name += " " + i;

            pooledObject.OnDestroyEvent += () => AddObjectToAvailable(pooledObject);

            pooledObject.gameObject.SetActive(false);
             
        }
    }

    public T Get<T>() where T : GameObject
    {
        if (_objects.Count == 0) GrowPool();

        GameObject pooledObject = _objects.Dequeue();
        if (pooledObject == null) return null;
        return pooledObject as T;
    }

    private void MakeDisabledObjectsChildren()
    {
        if (_disabledObjects.Count <= 0) return;
        foreach (GameObject pooledObject in _disabledObjects.Where(pooledObject =>
                     pooledObject.gameObject.activeInHierarchy == false))
        {
            pooledObject.transform.SetParent(transform);
            pooledObject.GetComponent<NetworkObject>().TrySetParent(transform);
        }
        

        _disabledObjects.Clear();
    }

    public static Pool GetPool(GameObject prefab)
    {
        if (_pools.ContainsKey(prefab))
            return _pools[prefab];

        Pool pool = UnityEngine.GameObject.Find("Pool-" + prefab.name).GetComponent<Pool>();
        _pools.Add(prefab, pool);
        return pool;
    }
}