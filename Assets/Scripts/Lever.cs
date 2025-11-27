using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Lever : NetworkBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color offColor = new Color(0.6f, 0.2f, 0.2f);
    [SerializeField] private Color onColor = new Color(0.2f, 0.6f, 0.2f);

    [Header("State")]
    public NetworkVariable<bool> IsActivated = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Interaction")]
    [SerializeField] private float interactionCooldown = 0.5f; // Evita spam
    private float lastInteractionTime = -999f;

    private HashSet<PlayerMovement> playersInRange = new HashSet<PlayerMovement>();

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        IsActivated.OnValueChanged += OnStateChanged;
        UpdateVisuals(IsActivated.Value);
    }

    public override void OnNetworkDespawn()
    {
        IsActivated.OnValueChanged -= OnStateChanged;
        playersInRange.Clear();
    }

    //  CALLBACKS 

    private void OnStateChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"[Lever] State changed: {(previousValue ? "ON" : "OFF")} → {(newValue ? "ON" : "OFF")}");
        UpdateVisuals(newValue);
    }

    private void UpdateVisuals(bool activated)
    {
        spriteRenderer.color = activated ? onColor : offColor;

        // Opcional: Rotar sprite para indicar estado
        transform.localRotation = Quaternion.Euler(0, 0, activated ? -45f : 0f);
    }

    //  INTERACTION 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Obtener PlayerMovement 
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playersInRange.Add(playerMovement);
            Debug.Log($"[Lever] Player entered interaction range: {other.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Obtener PlayerMovement (mismo patrón que Enter)
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playersInRange.Remove(playerMovement); // Remove en vez de Add
            Debug.Log($"[Lever] Player exited interaction range: {other.name}");
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Debug: Ver qué está detectando el trigger
        Debug.Log($"[Lever DEBUG] OnTriggerStay2D detected: {other.name} | Tag: {other.tag}");
    }

    private void Update()
    {
        if (!IsServer) return;

        if (Time.time < lastInteractionTime + interactionCooldown)
            return;

        foreach (PlayerMovement playerMovement in playersInRange) // Cambiar tipo
        {
            if (playerMovement == null) continue;

            if (playerMovement.IsInteractPressed())
            {
                ToggleLever();
                break;
            }
        }
    }

    private void ToggleLever()
    {
        lastInteractionTime = Time.time;
        IsActivated.Value = !IsActivated.Value;
        Debug.Log($"[Lever] Toggled to: {(IsActivated.Value ? "ON" : "OFF")}");
    }

    //  PUBLIC METHODS

    /// <summary>
    /// Obtener estado actual
    /// </summary>
    public bool GetState()
    {
        return IsActivated.Value;
    }

    // Testing
    [ContextMenu("Toggle Lever (Server Only)")]
    private void DebugToggle()
    {
        if (IsServer)
            ToggleLever();
    }
}