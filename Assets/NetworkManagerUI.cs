using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button _serverButton;
    [SerializeField] private TMP_InputField _tmpInputFieldHost;
    [SerializeField] private TMP_InputField _tmpInputFieldClient;

    [SerializeField] private Button _clientButton;
    [SerializeField] private Button _hostButton;


    private void Awake()
    {
      // _serverButton.onClick.AddListener((() => { NetworkManager.Singleton.StartServer(); }));
      // _clientButton.onClick.AddListener((() =>
      // {
      //     NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
      //         _tmpInputFieldClient.text.IsNullOrWhiteSpace ? "127.0.0.1" : _tmpInputFieldClient.text,
      //         (ushort) 7777
      //     );
      //     NetworkManager.Singleton.StartClient();
      //     this.gameObject.SetActive(false);
      // }));
      // _hostButton.onClick.AddListener((() =>
      // {
      //     NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
      //         _tmpInputFieldHost.text.IsNullOrWhiteSpace() ? "127.0.0.1" : _tmpInputFieldHost.text,
      //         (ushort) 7777
      //     );NetworkManager.Singleton.StartHost();
      //     this.gameObject.SetActive(false);

      // }));
    }
}