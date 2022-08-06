using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;



public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance = null;
    private static List<string> m_eventsList = new List<string>
    { "App_Launched","Match_Started","Match_Ended","Kick_Regular","Kick_Special","Jump","Bomb_Throw","Player_Hit_By_Bomb",
    "Retry_Button_Pressed","Tutorial_Started","Tutorial_Completed","Tutorial_Step"};

    private void Awake()
    {
        // If there is not already an instance of AnalyticsManager, set it to this.
        if (Instance == null)
        {
            Instance = this;
        }
        //If an instance already exists, destroy whatever this object is to enforce the singleton.
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        //Set AnalyticsManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
        DontDestroyOnLoad(gameObject);
    }

    public static void CommitData(string eventName)
    {
        if (!m_eventsList.Contains(eventName))
        {
            Debug.Log("Events List doesnt contains this event: " + eventName);
            return;
        }
        AnalyticsResult analyticsResult = Analytics.CustomEvent(eventName);
        //Debug.Log("analyticsResult: " + analyticsResult);

        /*Debug.Log("------------------------------------------------------------------------------");
        Debug.Log("eventName: " + eventName);*/

    }

    public static void CommitData(string eventName, Dictionary<string, object> dir)
    {
        if (!m_eventsList.Contains(eventName))
        {
            Debug.Log("Events List doesnt contains this event: " + eventName);
            return;
        }
        AnalyticsResult analyticsResult = Analytics.CustomEvent(eventName, dir);
        //Debug.Log("analyticsResult: " + analyticsResult);
        /*Debug.Log("------------------------------------------------------------------------------");
        Debug.Log("eventName: " + eventName + " Data: ");
        foreach (KeyValuePair<string, object> kvp in dir)
            Debug.Log("Key = " + kvp.Key + " Value = " + kvp.Value);*/
    }

}