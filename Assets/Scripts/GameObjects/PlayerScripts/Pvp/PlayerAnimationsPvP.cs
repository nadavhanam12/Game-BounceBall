using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerAnimationsPvP : PlayerAnimations, IOnEventCallback
{
    int m_viewId;
    public void SetViewId(int viewId)
    {
        m_viewId = viewId;
    }
    public override void AnimSetTrigger(string triggerName)
    {
        object[] data = new object[] { m_viewId, triggerName };
        PvPEventsService.Instance.SendEventToAll
                   (PvPEventsService.Event_PlayerAnimationTrigger,
                    data);
    }
    public override void WinAnim()
    {
        object[] data = new object[] { m_viewId, "Win" };
        PvPEventsService.Instance.SendEventToAll
                   (PvPEventsService.Event_PlayerAnimationPlay,
                    data);
    }

    public override void LoseAnim()
    {
        object[] data = new object[] { m_viewId, "Lose" };
        PvPEventsService.Instance.SendEventToAll
                   (PvPEventsService.Event_PlayerAnimationPlay,
                    data);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == PvPEventsService.Event_PlayerAnimationTrigger)
            HandleAnimationTrigger(photonEvent.CustomData);
        if (eventCode == PvPEventsService.Event_PlayerAnimationPlay)
            HandleAnimationPlay(photonEvent.CustomData);
    }

    private void HandleAnimationTrigger(object data)
    {
        object[] dataArray = (object[])data;
        int viewId = (int)dataArray[0];
        if (m_viewId != viewId)
            return;
        string triggerName = (string)dataArray[1];
        //print("HandleAnimationTrigger- viewId: " + viewId + " triggerName: " + triggerName);

        if (triggerName == "KickReg Trigger") return;
        base.AnimSetTrigger(triggerName);
    }

    private void HandleAnimationPlay(object data)
    {
        object[] dataArray = (object[])data;
        int viewId = (int)dataArray[0];
        if (m_viewId != viewId)
            return;
        string animToPlay = (string)dataArray[1];
        if (animToPlay == "Win")
            base.WinAnim();
        else
            base.LoseAnim();
    }

    void OnEnable() { PhotonNetwork.AddCallbackTarget(this); }
    void OnDisable() { PhotonNetwork.RemoveCallbackTarget(this); }
}
