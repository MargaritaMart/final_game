using Unity.Netcode;
using UnityEngine;

public class PlayerVisuals : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            int playerIndex = (int)OwnerClientId % 2;
            GetComponent<SpriteRenderer>().color = 
                playerIndex == 0 ? Color.red : Color.blue;
        }
    }
}