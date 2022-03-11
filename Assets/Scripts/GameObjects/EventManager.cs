using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum EVENT
{
    EventStartApp, EventMainMenu, EventButtonClick, EventStartGameScene, EventCombo, EventNormalKick, EventPowerKick, EventUpKick,
    EventOnRestart, EventOnTimeOver, EventOnBallLost, EventOnCountdownEnds
}; // ... Other events

public static class EventManager
{
    // Stores the delegates that get called when an event is fired
    private static Dictionary<EVENT, Action> eventTable
                 = new Dictionary<EVENT, Action>();

    // Adds a delegate to get called for a specific event
    public static void AddHandler(EVENT evnt, Action action)
    {
        if (!eventTable.ContainsKey(evnt)) eventTable[evnt] = action;
        else eventTable[evnt] += action;
    }
    public static void RemoveHandler(EVENT evnt, Action action)
    {

        if (eventTable.ContainsKey(evnt))
        {
            if (eventTable[evnt] != null) eventTable[evnt] -= action;

            /*Delegate[] listeners = eventTable[evnt].GetInvocationList();
            foreach (Delegate delecate in listeners)
            {
                if (delecate.Equals(action))
                {
                    eventTable[evnt] -= action;
                }
            }*/
        }

    }

    public static void Clean()
    {
        eventTable = new Dictionary<EVENT, Action>();
    }

    // Fires the event
    public static void Broadcast(EVENT evnt)
    {
        //if (eventTable.ContainsKey(evnt)) eventTable[evnt]();
        /*if (evnt == EVENT.EventCombo)
        {
            var y = eventTable;
        }*/
        if ((eventTable.ContainsKey(evnt)) && (eventTable[evnt] != null)) eventTable[evnt]();
    }


}
