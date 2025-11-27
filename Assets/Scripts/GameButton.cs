using Unity.Netcode;
using UnityEngine;

public class GameButton : NetworkBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.yellow;
    [SerializeField] private Color pressedColor = Color.red;

    [Header("State")]
    // NetworkVariable para sincronizar si está presionado
    public NetworkVariable<bool> IsPressed = new NetworkVariable<bool>(
        value: true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

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
        // Cambiar color según estado
        spriteRenderer.color = pressed ? pressedColor : normalColor;
    }

    // COLLISION DETECTION (siguiente paso)
    // Lo implementaremos en Paso 2

    private void Update()
    {
        // Solo para testing: presiona T para cambiar estado (solo server)
        if (Input.GetKeyDown(KeyCode.T) && IsServer)
        {
            IsPressed.Value = !IsPressed.Value;
            Debug.Log($"[Button TEST] Toggled to: {IsPressed.Value}");
        }
    }
}