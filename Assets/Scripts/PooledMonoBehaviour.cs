using System;
using UnityEngine;

public class PooledMonobehaviour : MonoBehaviour
{
    [SerializeField] private int initialPoolSize = 50;

    public int InitialPoolSize => initialPoolSize;

    protected virtual void OnDisable()
    {
        if (OnDestroyEvent != null)
            OnDestroyEvent();
    }

    public event Action OnDestroyEvent;

    public T Get<T>(bool enable = true) where T : PooledMonobehaviour
    {
        Pool pool = Pool.GetPool(this);
        T pooledObject = pool.Get<T>();

        if (enable) pooledObject.gameObject.SetActive(true);

        return pooledObject;
    }

    public T Get<T>(Transform parent, bool resetTransform = false) where T : PooledMonobehaviour
    {
        T pooledObject = Get<T>();
        pooledObject.transform.SetParent(parent);

        if (!resetTransform) return pooledObject;

        Transform pooledObjectTransform = pooledObject.transform;
        pooledObjectTransform.localPosition = Vector3.zero;
        pooledObjectTransform.localRotation = Quaternion.identity;

        return pooledObject;
    }

    public T Get<T>(Transform parent, Vector3 relativePosition, Quaternion relativeRotation)
        where T : PooledMonobehaviour
    {
        T pooledObject = Get<T>();
        Transform pooledObjectTransform;

        (pooledObjectTransform = pooledObject.transform).SetParent(parent);

        pooledObjectTransform.localPosition = relativePosition;
        pooledObjectTransform.localRotation = relativeRotation;

        return pooledObject;
    }
}
