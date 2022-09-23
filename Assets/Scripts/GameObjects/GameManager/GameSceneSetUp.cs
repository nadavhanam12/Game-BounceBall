using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneSetUp : MonoBehaviour
{
    private string m_gameSceneName = "GameScene";
    public GameArgs m_gameArgs;
    GameManagerAbstract m_gameManagerScript;
    MainMenu m_mainMenu;

    GameSceneSetUpPvPMode m_helperPvPMode;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    void Awake()
    {
        Application.targetFrameRate = 60;
        m_mainMenu = FindObjectOfType<MainMenu>(true);
        if (!m_mainMenu)
        {//runs as solo scene
            SetGameSceneArgs(m_gameArgs);
            PlayGameScene();
        }
    }

    public void SetGameSceneArgs(GameArgs gameArgs)
    {
        m_gameArgs = gameArgs;
        CreateGameManager();
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

                m_helperPvPMode = gameObject.AddComponent<GameSceneSetUpPvPMode>();
                if (m_helperPvPMode.IsConnected())
                {
                    m_gameManagerScript = gameObject.AddComponent<GameManagerPvPMode>();
                    m_helperPvPMode.Init((GameManagerPvPMode)m_gameManagerScript, m_mainMenu);
                }
                else
                    m_gameManagerScript = gameObject.AddComponent<GameManagerPvEMode>();
                break;
        }
        if (SceneManager.sceneCount > 1) //in case we run only game scene from editor
            SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
        m_gameManagerScript.SetGameArgs(m_gameArgs);
        PlayGameScene();
    }

    public void PlayGameScene()
    {
        Debug.Log("PlayGameScene");
        if (m_gameManagerScript.GetType() == typeof(GameManagerPvPMode))
            m_helperPvPMode.ReadyToStartGameScene();
        else
        {
            m_mainMenu?.UnloadMenu();
            m_gameManagerScript.StartGameScene();
        }
    }
}
