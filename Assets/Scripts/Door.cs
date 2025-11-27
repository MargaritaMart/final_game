using Unity.Netcode;
using UnityEngine;

public class Door : NetworkBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D doorCollider;
    [SerializeField] private Color closedColor = new Color(0.4f, 0.3f, 0.2f); // Marrón
    [SerializeField] private Color openColor = new Color(0.4f, 0.3f, 0.2f, 0.3f); // Transparente

    [Header("State")]
    public NetworkVariable<bool> IsOpen = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        // Cachear componentes
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (doorCollider == null)
            doorCollider = GetComponent<BoxCollider2D>();
    }

    public override void OnNetworkSpawn()
    {
        // Suscribirse a cambios de estado
        IsOpen.OnValueChanged += OnDoorStateChanged;

        // Aplicar estado inicial
        UpdateDoorState(IsOpen.Value);
    }

    public override void OnNetworkDespawn()
    {
        // Cleanup
        IsOpen.OnValueChanged -= OnDoorStateChanged;
    }

    // CALLBACKS

    private void OnDoorStateChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"[Door] State changed: {(previousValue ? "Open" : "Closed")} → {(newValue ? "Open" : "Closed")}");
        UpdateDoorState(newValue);
    }

    private void UpdateDoorState(bool open)
    {
        if (open)
        {
            // Puerta abierta
            spriteRenderer.color = openColor;  // Más transparente
            doorCollider.enabled = false;       // No bloquea paso
        }
        else
        {
            // Puerta cerrada
            spriteRenderer.color = closedColor; // Sólida
            doorCollider.enabled = true;        // Bloquea paso
        }
    }

    // PUBLIC METHODS (para GameButton)

    /// <summary>
    /// Abre la puerta
    /// </summary>
    public void Open()
    {
        if (!IsServer) return;
        IsOpen.Value = true;
    }

    /// <summary>
    /// Cierra la puerta
    /// </summary>
    public void Close()
    {
        if (!IsServer) return;
        IsOpen.Value = false;
    }

    // Testing opcional
    [ContextMenu("Toggle Door (Server Only)")]
    private void DebugToggle()
    {
        if (IsServer)
            IsOpen.Value = !IsOpen.Value;
    }
}