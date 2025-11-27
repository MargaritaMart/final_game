using Unity.Netcode;
using UnityEngine;

public class GameButton : NetworkBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private Color normalColor = Color.yellow;
    [SerializeField] private Color pressedColor = Color.red;

    [Header("State")]
    // NetworkVariable para sincronizar si está presionado
    public NetworkVariable<bool> IsPressed = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Connected Objects")]
    [SerializeField] private Door connectedDoor;

    private void Awake()
    {
        // Cachear el SpriteRenderer
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        // Suscribirse a cambios de estado
        IsPressed.OnValueChanged += OnPressedChanged;

        // Aplicar color inicial
        UpdateVisuals(IsPressed.Value);
    }

    public override void OnNetworkDespawn()
    {
        // Cleanup: desuscribirse
        IsPressed.OnValueChanged -= OnPressedChanged;
    }

    // CALLBACKS

    private void OnPressedChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"[Button] State changed: {previousValue} → {newValue}");
        UpdateVisuals(newValue);
    }

    private void UpdateVisuals(bool pressed)
    {
        // Cambiar color según estado y sprites para el futuro
        spriteRenderer.color = pressed ? pressedColor : normalColor;
        spriteRenderer.sprite = pressed ? pressedSprite : normalSprite;
    }

    // COLLISION DETECTION
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Solo el servidor procesa la lógica de juego
        if (!IsServer) return;

        // Verificar que es un jugador
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[GameButton] Player entered trigger: {other.name}");

            IsPressed.Value = true;

            // Abrir puerta conectada
            if (connectedDoor != null)
            {
                connectedDoor.Open();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Solo el servidor procesa la lógica de juego
        if (!IsServer) return;

        // Verificar que es un jugador
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[GameButton] Player exited trigger: {other.name}");

            // Cerrar puerta conectada
            if (connectedDoor != null)
            {
                connectedDoor.Close();
            }
        }
    }
}