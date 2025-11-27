using Unity.Netcode;
using UnityEngine;

public class MovingPlatform : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 offsetA = Vector3.zero;  // Posición cuando Lever OFF
    [SerializeField] private Vector3 offsetB = new Vector3(0, 4, 0);  // Posición cuando Lever ON
    [SerializeField] private float moveSpeed = 2f;  // Velocidad de movimiento

    // Posiciones absolutas calculadas
    private Vector3 worldPositionA;
    private Vector3 worldPositionB;

    [Header("State")]
    // NetworkVariable para sincronizar posición objetivo
    public NetworkVariable<Vector3> TargetPosition = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Calcular posiciones absolutas basadas en posición inicial + offsets
        worldPositionA = transform.position + offsetA;
        worldPositionB = transform.position + offsetB;
    }

    public override void OnNetworkSpawn()
    {
        // Suscribirse a cambios de target position
        TargetPosition.OnValueChanged += OnTargetPositionChanged;

        // Inicializar en posición A
        if (IsServer)
        {
            TargetPosition.Value = worldPositionA;
        }

        // Mover inmediatamente a la posición inicial (sin lerp)
        transform.position = TargetPosition.Value;
    }

    public override void OnNetworkDespawn()
    {
        TargetPosition.OnValueChanged -= OnTargetPositionChanged;
    }

    private void FixedUpdate()
    {
        // Mover suavemente hacia la posición objetivo
        Vector3 currentPos = transform.position;
        Vector3 targetPos = TargetPosition.Value;

        // Solo mover si no estamos en el target
        if (Vector3.Distance(currentPos, targetPos) > 0.01f)
        {
            // Lerp usando Rigidbody2D.MovePosition para física correcta
            Vector3 newPos = Vector3.MoveTowards(currentPos, targetPos, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }
    }

    // CALLBACKS

    private void OnTargetPositionChanged(Vector3 previousValue, Vector3 newValue)
    {
        Debug.Log($"[MovingPlatform] Target changed: {previousValue} → {newValue}");
    }

    // PUBLIC METHODS (para Lever)

    /// <summary>
    /// Mover a posición A (Lever OFF)
    /// </summary>
    public void MoveToPositionA()
    {
        if (!IsServer) return;
        TargetPosition.Value = worldPositionA;
    }

    /// <summary>
    /// Mover a posición B (Lever ON)
    /// </summary>
    public void MoveToPositionB()
    {
        if (!IsServer) return;
        TargetPosition.Value = worldPositionB;
    }

    /// <summary>
    /// Toggle entre A y B según estado del Lever
    /// </summary>
    public void SetPosition(bool usePosB)
    {
        if (!IsServer) return;
        TargetPosition.Value = usePosB ? worldPositionB : worldPositionA;
    }

    // EDITOR HELPERS

    // Visualizar posiciones A y B en Scene view
    private void OnDrawGizmosSelected()
    {
        // Calcular posiciones en tiempo de editor
        Vector3 posA = transform.position + offsetA;
        Vector3 posB = transform.position + offsetB;

        // Posición A (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(posA, transform.localScale);
        Gizmos.DrawLine(transform.position, posA);

        // Posición B (verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(posB, transform.localScale);
        Gizmos.DrawLine(transform.position, posB);

        // Label (opcional, útil)
#if UNITY_EDITOR
        UnityEditor.Handles.Label(posA, "Position A (OFF)");
        UnityEditor.Handles.Label(posB, "Position B (ON)");
#endif
    }

    // Testing
    [ContextMenu("Move to Position A")]
    private void DebugMoveToA()
    {
        if (IsServer) MoveToPositionA();
    }

    [ContextMenu("Move to Position B")]
    private void DebugMoveToB()
    {
        if (IsServer) MoveToPositionB();
    }
}