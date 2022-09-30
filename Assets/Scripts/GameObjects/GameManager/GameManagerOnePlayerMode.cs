using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static PlayerScript;

public class GameManagerOnePlayerMode : GameManagerAbstract
{

    protected override void InitGameMood(bool throwNewBall = true)
    {
        m_curPlayerTurn = PlayerIndex.First;
        m_playerData1.PlayerScript.StartTurn(throwNewBall);
        m_playerData2.PlayerScript?.HidePlayer();
    }

    protected override async void SwitchPlayerTurn(bool shouldSwitchTurn = true)
    {
        if (!m_inTutorial)
            m_gameCanvas.SwitchTurn(m_curPlayerTurn != PlayerIndex.First);
        shouldSwitchTurn = false;

        await Task.Delay(2000);
        InitPlayersStatus();

        await Task.Delay(1000);
        SwitchPlayerTurnAfterWait(true, shouldSwitchTurn);
    }

    protected override void SwitchPlayerTurnAfterWait(bool throwNewBall = true, bool shouldSwitchTurn = true)
    {
        shouldSwitchTurn = false;
        base.SwitchPlayerTurnAfterWait(throwNewBall, shouldSwitchTurn);
    }

    protected override void InitPlayers()
    {
        m_playerContainer = FindObjectOfType<PlayerContainer>(true);

        m_playerData1.PlayerScript = m_playerContainer.SpawnRegularPlayer();
        m_playerData1.PlayerScript.gameObject.SetActive(false);
        m_playerData1.PlayerScript.Init(m_playerData1);
    }

    public override void onTurnLost()
    {
        PlayerArgs playerData;
        if (m_curPlayerTurn == PlayerIndex.First)
        {
            playerData = m_playerData1;
            EventManager.Broadcast(EVENT.EventOnBallLost);
        }
        else
            playerData = m_playerData2;


        playerData.CurComboIndex = -1;
        playerData.CurCombo = 0;
        m_gameCanvas.SetCombo(playerData.CurCombo);

        if (m_inTutorial)
            onTurnLostTutorial();
        else if (m_timeIsOver)
            MatchEnd();
        else
        {
            m_playerData1.CurScore = 0;
            m_ballsManager.OnNewBallInScene();
        }
    }

    public override void OnBallHit(PlayerIndex playerIndex)
    {
        if (m_inTutorial)
            m_tutorialManager.OnBallHit();

        BroadcastKickType();

        m_playerData1.CurScore += 1;
        m_gameCanvas.IncrementCombo();
        if (m_inTutorial && !m_tutorialManager.IsFreePlayMode())
            return;
        else if (m_playerData1.CurScore % m_gameArgs.SinglePlayerCheerFrequency == 0)
            m_gameCanvas.CheerActivate();

    }

    public override void GameIsOver()
    {
        //print("Time is over");
        if (!m_isGamePause)
        {
            SetGamePause(true);
            m_ballsManager.TimeIsOver(); //should turn off the balls

            int prevBestScore = m_gameCanvas.GetPrevBestCombo();
            int curBestScore = m_gameCanvas.GetCurBestCombo();
            if (prevBestScore < curBestScore)
            {
                PlayerPrefs.SetInt("SinglePlayerBestCombo", curBestScore);
                AnalyticsManager.Instance().CommitData(
                                       AnalyticsManager.AnalyticsEvents.Event_New_Best_score,
                                       new Dictionary<string, object> {
                 { "Score", curBestScore }
                                    }); m_gameCanvas.OnNewBestScore(curBestScore);
            }
            else
                m_gameCanvas.OnPrevBestScore(prevBestScore);
        }
    }

    protected override void UpdatePlayerPrefsCompletedTutorial()
    {
        string playerPrefsGameTutorial = "CompletedSinglePlayerTutorial";
        PlayerPrefs.SetInt(playerPrefsGameTutorial, 1);
    }


}
