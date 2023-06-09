using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    [SerializeField] private KeyBindings _bindings;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this);
    }

    public KeyCode GetKeyForAction(KeyBindAction action)
    {
        foreach (KeyBindings.KeyBindingCheck keyBinding in _bindings.KeyBindingChecks)
        {
            if (keyBinding.KeyBindAction == action)
            {
                return keyBinding.KeyCode;
            }
        }
        return KeyCode.None;
    }

    public bool GetKeyDown(KeyBindAction key)
    {
        foreach (KeyBindings.KeyBindingCheck bindingsKeyBindingCheck in _bindings.KeyBindingChecks)
        {
            if (bindingsKeyBindingCheck.KeyBindAction == key)
            {
                return Input.GetKeyDown(bindingsKeyBindingCheck.KeyCode);
            }
        }
        return false;
    }
    
    public bool GetKeyUp(KeyBindAction key)
    {
        foreach (KeyBindings.KeyBindingCheck bindingsKeyBindingCheck in _bindings.KeyBindingChecks)
        {
            if (bindingsKeyBindingCheck.KeyBindAction == key)
            {
                return Input.GetKeyUp(bindingsKeyBindingCheck.KeyCode);
            }
        }
        return false;
    }

    public bool GetKey(KeyBindAction key)
    {
        foreach (KeyBindings.KeyBindingCheck bindingsKeyBindingCheck in _bindings.KeyBindingChecks)
        {
            if (bindingsKeyBindingCheck.KeyBindAction == key)
            {
                return Input.GetKey(bindingsKeyBindingCheck.KeyCode);
            }
        }
        return false;
    }
}   