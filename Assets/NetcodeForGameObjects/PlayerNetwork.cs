using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

// Scripts that should run on the Network need to inherit from NetworkBehaviour

public class PlayerNetwork : NetworkBehaviour
{
    // By default Network Variables can be changed only by the Server, that's why we change the Write Permission to Owner
    // Network Variables can only be value types (int, float, bool, enum, struct), not reference types (object, class)

    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    // Don't use Awake and Start on Network, rather override OnNetworkSpawn

    public override void OnNetworkSpawn() {
        randomNumber.OnValueChanged += (int previousValue, int newValue) => {
            Debug.Log(OwnerClientId + "; randomNumber: " + randomNumber.Value);
        };
    }


    void Update() {

        // IsOwner is a nice check when we need it

        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T)) {
            randomNumber.Value = Random.Range(0, 100);
		}

        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;

        float moveSpeed = 3f;
        transform.position += moveSpeed * Time.deltaTime * moveDir;

    }
}
