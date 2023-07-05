using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace NetworkManagement
{
    public class RoomButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameTMP;

        public RoomData RoomInformation { get; set; }

        public void Init(RoomData roomInfo)
        {
            RoomInformation = roomInfo;
            //nameTMP.text = Globals.GetRequiredAmountForTable(RoomInformation.mode).ToString();
        }

        public void OnTapped()
        {
            NetworkManager.instance.JoinRoom(RoomInformation);
        }
    }

    public class RoomData
    {
        public byte maxPlayers;
        public byte mode;
    }
}
