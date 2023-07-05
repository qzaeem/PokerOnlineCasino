using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine;

public class NetworkTest : MonoBehaviourPunCallbacks
{
    [SerializeField] Button joinRoomButton, sendMessageButton;
    [SerializeField] Text roomName, status;

    Room lastJoinedRoom;

    // Start is called before the first frame update
    void Start()
    {
        Connect("Player" + Random.Range(1, 51));
    }

    void Reconnect()
    {
        bool isReconnecting = PhotonNetwork.ReconnectAndRejoin();

        if (isReconnecting)
        {
            status.text = "Reconnecting...";
        }
        else
        {
            Invoke("Reconnect", 1);
        }
    }

    public void Connect(string nickeName)
    {
        joinRoomButton.interactable = false;
        sendMessageButton.interactable = false;
        if (PhotonNetwork.IsConnected)
        {

        }
        else
        {
            PhotonNetwork.NickName = nickeName;
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = "1"; // TODO
            status.text = "Connecting";
        }
    }

    public void JoinOrCreateRandomRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = Globals.maxPlayersInRoom;
        roomOptions.EmptyRoomTtl = Globals.roomTTL;
        roomOptions.PlayerTtl = Globals.playerTTL;
        PhotonNetwork.JoinRandomOrCreateRoom(null, Globals.maxPlayersInRoom, MatchmakingMode.FillRoom, null, null,
            null, roomOptions);
        status.text = "Joining room";
    }

    public override void OnCreatedRoom()
    {
        print("Room Created!");
        status.text = "Room created";
    }

    public override void OnJoinedRoom()
    {
        lastJoinedRoom = PhotonNetwork.CurrentRoom;
        roomName.text = lastJoinedRoom.Name;
        status.text = "Room joined";
        joinRoomButton.interactable = false;
        sendMessageButton.interactable = true;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogError(message);
        print("Joining failed!");
        status.text = "Join room failed";
        joinRoomButton.interactable = true;
    }

    public override void OnLeftRoom()
    {
        print("I Left Room!");
        status.text = "Left room";
        joinRoomButton.interactable = false;
        sendMessageButton.interactable = false;
    }

    public override void OnConnectedToMaster()
    {
        print("Connected to Master!");
        status.text = "Connected to master";
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        print("Disconnected!");
        joinRoomButton.interactable = false;
        sendMessageButton.interactable = false;
        Debug.LogError(cause.ToString());

        status.text = "Disconnected";
        Invoke("Reconnect", 1);
    }

    public override void OnJoinedLobby()
    {
        print("Joined Lobby!");
        status.text = "Joined Lobby!";
        joinRoomButton.interactable = true;
    }

    public void SendMsg()
    {
        Dictionary<int, int> dict = new Dictionary<int, int>();
        dict.Add(1, PhotonNetwork.LocalPlayer.ActorNumber);
        photonView.RPC("ReceiveMessage", RpcTarget.Others, dict);
    }

    [PunRPC]
    void ReceiveMessage(Dictionary<int, int> dict)
    {
        Debug.LogError("Message received from Player " + dict[1]);
    }
}
