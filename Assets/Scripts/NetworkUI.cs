using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] Button hostButton;
    [SerializeField] Button clientButton;
    [SerializeField] TMP_InputField ipInputField;
    [SerializeField] GameObject lobbyPanel;

    void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
        
        // Callback para detectar desconexiones
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    void StartHost()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        
        // Escuchar en todas las interfaces de red
        transport.SetConnectionData("0.0.0.0", 7777);
        
        NetworkManager.Singleton.StartHost();
        lobbyPanel.SetActive(false);
        
        Debug.Log($"[HOST] Started on 0.0.0.0:7777");
    }

    void StartClient()
    {
        string ipAddress = ipInputField.text.Trim();
        
        // Validación básica
        if (string.IsNullOrEmpty(ipAddress))
        {
            Debug.LogError("[CLIENT] IP is empty!");
            return;
        }
        
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ipAddress, 7777);
        
        NetworkManager.Singleton.StartClient();
        lobbyPanel.SetActive(false);
        
        Debug.Log($"[CLIENT] Connecting to {ipAddress}:7777");
    }
    
    void OnClientDisconnect(ulong clientId)
    {
        if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            Debug.LogError($"[CLIENT] Disconnected! ClientId: {clientId}");
            // Reactivar lobby para reintentar
            lobbyPanel.SetActive(true);
        }
    }
    
    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }
}