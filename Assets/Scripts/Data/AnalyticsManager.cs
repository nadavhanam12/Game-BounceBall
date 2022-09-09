using System;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class AnalyticsManager
{
    static AnalyticsManager instance = null;
    static bool m_initialize = false;

    public enum AnalyticsEvents
    {
        Event_App_Launched,
        Event_Match_Started,
        Event_Match_Ended,
        Event_Kick_Regular,
        Event_Kick_Special,
        Event_Jump,
        Event_Bomb_Throw,
        Event_Player_Hit_By_Bomb,
        Event_Retry_Button_Pressed,
        Event_Tutorial_Started,
        Event_Tutorial_Completed,
        Event_Tutorial_Step
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static AnalyticsManager Instance()
    {
        if (instance == null)
        {
            instance = new AnalyticsManager();
            Init();
        }
        return instance;
    }


    static async void Init()
    {
        try
        {
            var options = new InitializationOptions();
            options.SetEnvironmentName("development");
            await UnityServices.InitializeAsync(options);
            List<string> consentIdentifiers = await AnalyticsService.Instance.CheckForRequiredConsents();
            Debug.Log("Init AnalyticsService");
            m_initialize = true;
        }
        catch (ConsentCheckException e)
        {
            // Something went wrong when checking the GeoIP, check the e.Reason and handle appropriately.
            Debug.Log(e);
        }
    }

    public void CommitData(AnalyticsEvents eventName)
    {
        if (!m_initialize) return;
        Dictionary<string, object> parameters = new Dictionary<string, object>();

        AnalyticsService.Instance.CustomData(eventName.ToString(), parameters);
        AnalyticsService.Instance.Flush();

        //Debug.Log("------------------------------------------------------------------------------");
        //Debug.Log("eventName: " + eventName.ToString());

    }

    public void CommitData(AnalyticsEvents eventName, Dictionary<string, object> dir)
    {
        if (!m_initialize) return;
        Dictionary<string, object> parametersFormat = GenerateParametersFormat(dir);
        AnalyticsService.Instance.CustomData(eventName.ToString(), parametersFormat);
        AnalyticsService.Instance.Flush();
        //Debug.Log("------------------------------------------------------------------------------");
        //Debug.Log("eventName: " + eventName.ToString() + " Data: ");
        /*foreach (KeyValuePair<string, object> kvp in dir)
            Debug.Log("Key = " + kvp.Key + " Value = " + kvp.Value);*/
    }

    private Dictionary<string, object> GenerateParametersFormat(Dictionary<string, object> dir)
    {
        Dictionary<string, object> parametersFormat = new Dictionary<string, object>();
        Type curType;
        foreach (var pair in dir)
        {
            curType = (pair.Value.GetType());
            if (curType == typeof(string) || (curType == typeof(int) ||
                 curType == typeof(bool)) || curType == typeof(float))
                parametersFormat.Add(pair.Key, pair.Value);
            else
                parametersFormat.Add(pair.Key, pair.Value.ToString());

        }
        return parametersFormat;
    }
}