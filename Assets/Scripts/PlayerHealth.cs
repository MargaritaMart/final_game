using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


/// Maneja la muerte y respawn de un player con autoridad de servidor.
public class PlayerHealth : NetworkBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 2f;

    private Vector3 spawnPosition;

    //  NETWORK STATE
    private NetworkVariable<bool> isAlive = new NetworkVariable<bool>(
        value: true,
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    //  Referencias del player 
    private SpriteRenderer spriteRenderer;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;

    //  DEBUG (Opcional - para testing sin death zone) 
    [Header("Debug - Muerte con una tecla")]
    [SerializeField] private bool enableDebugDeath = true;

    //  LIFECYCLE 
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        if (SpawnManager.Instance != null)
        {
            spawnPosition = SpawnManager.Instance.GetSpawnPosition(OwnerClientId);
        }
        else
        {
            spawnPosition = transform.position;
            Debug.LogWarning("[PlayerHealth] SpawnManager no encontrado, usando posición actual como spawn.");
        }

        transform.position = spawnPosition;

        isAlive.OnValueChanged += OnAliveStateChanged;
        UpdateVisualState(isAlive.Value);

        Debug.Log($"[PlayerHealth] OnNetworkSpawn - ClientID: {OwnerClientId}, Spawn: {spawnPosition}");
    }

    public override void OnNetworkDespawn()
    {
        isAlive.OnValueChanged -= OnAliveStateChanged;
    }

    void Update()
    {
        // DEBUG: Muerte manual con tecla K (solo para testing)
        if (enableDebugDeath && IsOwner && Keyboard.current != null)
        {
            if (Keyboard.current.kKey.wasPressedThisFrame)
            {
                Debug.Log("[PlayerHealth] DEBUG: Manual death triggered");
                Die();
            }
        }
    }

    /// Llamado por otros scripts (ej: DeathZone) para matar al player.
    public void Die()
    {
        if (!IsOwner) return;

        Debug.Log($"[PlayerHealth] Die() called by owner - ClientID: {OwnerClientId}");
        DieRpc();
    }


    /// RPC(llamada remota) que se ejecuta en el servidor cuando un client pide morir.
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DieRpc(RpcParams rpcParams = default)
    {
        // Esta funcion se ejecuta solo en el servidor

        if (!isAlive.Value)
        {
            Debug.LogWarning($"[PlayerHealth] DieRpc ignored - Already dead");
            return;
        }

        Debug.Log($"[PlayerHealth] SERVER: Player died - ClientID: {rpcParams.Receive.SenderClientId}");

        isAlive.Value = false;

        // Detener física
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Invoke(nameof(Respawn), respawnDelay); // llamar el respawn con delay
    }

    /// SERVER ONLY: Respawnea al player.
    private void Respawn()
    {
        if (!IsServer) return;

        Debug.Log($"[PlayerHealth] SERVER: Respawning at {spawnPosition}");

        TeleportPlayerRpc(spawnPosition);

        isAlive.Value = true;
    }

    [Rpc(SendTo.Owner)]
    private void TeleportPlayerRpc(Vector3 position)
    {
        transform.position = spawnPosition;

        // Reactivar física
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }

        Debug.Log("[PlayerHealth] Teleported to spawn point locally");
    }


    /// Se ejecuta en TODOS los clients cuando isAlive cambia.
    private void OnAliveStateChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"[PlayerHealth] Alive state changed: {previousValue} → {newValue}");
        UpdateVisualState(newValue);
        // Aca se pueden agregar los efectos visuales que todos van a ver
    }

    /// Actualiza visuals según estado de vida.
    private void UpdateVisualState(bool alive)
    {
        if (alive)
        {
            //Vivo activar sprite y el movimiento
            if (spriteRenderer != null)
                spriteRenderer.enabled = true;

            if (playerMovement != null)
                playerMovement.enabled = true;

            Debug.Log($"[PlayerHealth] Player is now ALIVE");
        }
        else
        {
            // Muerto
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            if (playerMovement != null)
                playerMovement.enabled = false;

            Debug.Log($"[PlayerHealth] Player is now DEAD");

            // TODO: Se puede agregar:
            // - Efecto de partículas
            // - Sonido de muerte
            // - Animación de muerte
        }
    }
}