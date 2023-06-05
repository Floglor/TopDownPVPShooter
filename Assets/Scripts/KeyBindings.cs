using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Key Bindings", menuName = "Key Bindings")]
public class KeyBindings : ScriptableObject
{
    [System.Serializable]
    public class KeyBindingCheck
    {
        public KeyBindAction KeyBindAction;
        public KeyCode KeyCode;
    }

    public List<KeyBindingCheck> KeyBindingChecks;
}