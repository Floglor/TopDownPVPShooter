using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolReferenceList : MonoBehaviour
{
    //Workaround for despawning pooled objects from the component. They need a prefab of itself.
    public List<GameObject> _pooledPrefabs;
}
