using UnityEngine;


/// Gestiona las posiciones de spawn de los jugadores.
/// Proporciona acceso centralizado a spawn points desde cualquier script.
public class SpawnManager : MonoBehaviour
{
    // Singleton para acceso global
    public static SpawnManager Instance { get; private set; }

    [Header("Spawn Points")]
    [Tooltip("Asigna los spawn points desde el Inspector en orden: P1, P2, ...")]
    [SerializeField] private Transform[] spawnPoints;

    void Awake()
    {
        // Configurar singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("[SpawnManager] Ya existe una instancia, destruyendo duplicado.");
            return;
        }

        Instance = this;

        // Validar que hay spawn points configurados
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[SpawnManager] No hay spawn points asignados! Asígnalos en el Inspector.");
        }
        else
        {
            Debug.Log($"[SpawnManager] Inicializado con {spawnPoints.Length} spawn points.");
        }
    }

    /// <summary>
    /// Obtiene la posición de spawn para un jugador según su ClientID.
    /// </summary>
    /// <param name="clientId">ID del cliente (0 = Host, 1 = Client1, etc.)</param>
    /// <returns>Vector3 con la posición de spawn</returns>
    public Vector3 GetSpawnPosition(ulong clientId)
    {
        // Safety check
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[SpawnManager] No spawn points configurados, usando posición por defecto (0,0,0)");
            return Vector3.zero;
        }

        // Calcular índice usando módulo (permite wrap-around si hay más jugadores que spawn points)
        int index = (int)(clientId % (ulong)spawnPoints.Length);

        // Safety check para transforms nulos
        if (spawnPoints[index] == null)
        {
            Debug.LogError($"[SpawnManager] Spawn point {index} es null! Usando posición por defecto.");
            return Vector3.zero;
        }

        Vector3 position = spawnPoints[index].position;
        Debug.Log($"[SpawnManager] ClientID {clientId} → Spawn point {index} → Position {position}");

        return position;
    }

    /// <summary>
    /// (Opcional) Para debugging en Scene view.
    /// </summary>
    void OnDrawGizmos()
    {
        if (spawnPoints == null) return;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null) continue;

            // Dibujar esfera en cada spawn point
            Gizmos.color = i == 0 ? Color.red : Color.blue;
            Gizmos.DrawWireSphere(spawnPoints[i].position, 0.5f);

            // Dibujar número del spawn point
#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                spawnPoints[i].position + Vector3.up * 0.7f,
                $"P{i + 1}"
            );
#endif
        }
    }
}