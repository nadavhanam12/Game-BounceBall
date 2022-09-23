using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;

public sealed class PvPEventsService
{
    public static byte Event_PlayerReady = 1;
    public static byte Event_AllPlayersReady = 2;
    public static byte Event_PlayerAnimationTrigger = 3;
    public static byte Event_PlayerAnimationPlay = 4;

    public static readonly PvPEventsService Instance = new PvPEventsService();
    private PvPEventsService() { }


    bool IsConnected()
    {
        return PhotonNetwork.IsConnectedAndReady;
    }

    public void SendEventToAll(byte eventByte, object data)
    {
        PhotonNetwork.RaiseEvent(
                       eventByte,
                        data,
                        new RaiseEventOptions { Receivers = ReceiverGroup.All },
                        SendOptions.SendReliable
                        );
        //Debug.Log("SendEventToAll byte: " + eventByte);
    }
    public void SendEventToOthers(byte eventByte, object data)
    {
        PhotonNetwork.RaiseEvent(
                       eventByte,
                        data,
                        new RaiseEventOptions { Receivers = ReceiverGroup.Others },
                        SendOptions.SendReliable
                        );
        //Debug.Log("SendEventToAll byte: " + eventByte);
    }
    public void SendEventToMaster(byte eventByte, object data)
    {
        PhotonNetwork.RaiseEvent(
                       eventByte,
                        data,
                        new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                        SendOptions.SendReliable
                        );
        //Debug.Log("SendEventToMaster byte: " + eventByte);
    }


}
