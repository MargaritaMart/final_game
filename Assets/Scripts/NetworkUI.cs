using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TMP_InputField ipInputField;

    private void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);

        if (string.IsNullOrEmpty(ipInputField.text))
        {
            ipInputField.text = "127.0.0.1";
        }
    }

    private void StartHost()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = "0.0.0.0"; // Escuchar en todas las interfaces
        transport.ConnectionData.Port = 7777;

        NetworkManager.Singleton.StartHost();
        lobbyPanel.SetActive(false);
        Debug.Log($"Started as Host on 0.0.0.0:7777");
    }

    private void StartClient()
    {
        // Obtener IP del Input Field
        string ipAddress = ipInputField.text;

        // Validar que no esté vacío
        if (string.IsNullOrEmpty(ipAddress))
        {
            Debug.LogError("IP Address is empty! Using default: 127.0.0.1");
            ipAddress = "127.0.0.1";
        }

        // Configurar Unity Transport con la IP ingresada
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ipAddress;
        transport.ConnectionData.Port = 7777;

        // Iniciar como cliente
        NetworkManager.Singleton.StartClient();
        lobbyPanel.SetActive(false);

        Debug.Log($"Started as Client - Connecting to: {ipAddress}:7777");
    }

}