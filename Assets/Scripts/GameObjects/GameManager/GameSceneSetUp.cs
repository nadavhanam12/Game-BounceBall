using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneSetUp : MonoBehaviour
{
    public GameArgs m_gameArgs;
    GameManagerAbstract m_gameManagerScript;
    MainMenu m_mainMenu;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    void Awake()
    {
        Application.targetFrameRate = 60;
        m_mainMenu = FindObjectOfType<MainMenu>(true);
        if (!m_mainMenu) //runs as solo scene
            SetGameSceneArgs(m_gameArgs);
    }

    public void SetGameSceneArgs(GameArgs gameArgs)
    {
        m_gameArgs = gameArgs;
        CreateGameManager();
        StartGameScene();
    }

    void CreateGameManager()
    {//should create game manager per game mode and SetGameArgs() it with game args
        switch (m_gameArgs.GameType)
        {
            case (GameType.SinglePlayer):
                m_gameManagerScript = gameObject.AddComponent<GameManagerOnePlayerMode>();
                break;
            case (GameType.PvE):
                m_gameManagerScript = gameObject.AddComponent<GameManagerPvEMode>();
                break;
            case (GameType.PvP):
                m_gameManagerScript = gameObject.AddComponent<GameManagerPvPMode>();
                break;
        }

        m_gameManagerScript.SetGameArgs(m_gameArgs);
    }

    public void StartGameScene()
    {
        m_gameManagerScript.StartGameScene();

    }


}
