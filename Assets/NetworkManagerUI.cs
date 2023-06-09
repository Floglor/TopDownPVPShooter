using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button _serverButton;
    [SerializeField] private Button _clientButton;
    [SerializeField] private Button _hostButton;


    private void Awake()
    {
        _serverButton.onClick.AddListener((() =>
        {
            NetworkManager.Singleton.StartServer();
        }));
        _clientButton.onClick.AddListener((() =>
        {
            NetworkManager.Singleton.StartClient();
        }));
        _hostButton.onClick.AddListener((() =>
        {
            NetworkManager.Singleton.StartHost();
        }));
    }
}
