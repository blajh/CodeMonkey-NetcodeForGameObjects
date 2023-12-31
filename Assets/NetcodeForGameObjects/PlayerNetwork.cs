using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

// Scripts that should run on the Network need to inherit from NetworkBehaviour

public class PlayerNetwork : NetworkBehaviour
{

    [SerializeField]
    private Transform spawnedObjectPrefab;

    private Transform spawnedObjectTransform;

    // By default Network Variables can be changed only by the Server, that's why we change the Write Permission to Owner
    // Network Variables can only be value types (int, float, bool, enum, struct), not reference types (object, class, array, string)

    private NetworkVariable<int> randomNumberInt = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData {
            _int = 56,
            _bool = true,
            _message = "Default message."
        }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    // NetworkVariable does not know how to serialize custom structs, so we need to implement INetworkSerializable
    // Strings are reference types, but you can use FixedString for NetworkVariable

    public struct MyCustomData : INetworkSerializable {
        public int _int;
        public bool _bool;
        public FixedString128Bytes _message;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref _message);
        }
	}

    // Don't use Awake and Start on Network, rather override OnNetworkSpawn

    public override void OnNetworkSpawn() {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) => {
            Debug.Log(OwnerClientId + "; randomNumber: " + newValue._int + newValue._int);
        };
    }


    void Update() {

        // IsOwner is a nice check when we need it

        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T)) {

            // Instantiate spawns just on Server, not on Client - that's what the second line is for
            // Client can't spawn objects, but he can call a ServerRpc to spawn the object for the client

            spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);

            // TestClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 } } });

            // TestServerRpc(new ServerRpcParams());

            /*
            randomNumber.Value = new MyCustomData {
                _int = 10,
                _bool = false,
                _message = "All your base are belong to us!"
            };
            */
        }

        if (Input.GetKeyDown(KeyCode.Y)) {
            Destroy(spawnedObjectTransform.gameObject);
           
        }

        Vector3 moveDir = new Vector3 (0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;

        float moveSpeed = 3f;
        transform.position += moveSpeed * Time.deltaTime * moveDir;

    }

    // RemoteProcedureCalls (RPCs) must end with ServerRpc or ClientRpc and have the right [Attribute]
    // ServerRpc queues up to run on the server, even if called from the client
    // RPCs must be defined inside class that inherits NetworkBehaviour
	// RPCs must be attached to GameObjects that are NetworkObjects
    // RPCs can use value type parameters, the exception being strings
    // RPCs have an included paramter ServerRpcParams

    [ServerRpc]
    public void TestServerRpc(ServerRpcParams serverRpcParams) {
        Debug.Log("TestServerRpc " + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);

    }

    // The server calls the ClientRpc and all the Clients run it
    // Clients can not call ClientRpc's
    // ClientServerParams can choose which Target Client ID's will run the code

    [ClientRpc]
    public void TestClientRpc(ClientRpcParams clientRpcParams) {
        Debug.Log("TestClientRpc");
	}
}
