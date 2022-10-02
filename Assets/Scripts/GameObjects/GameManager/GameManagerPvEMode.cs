using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static PlayerScript;

public class GameManagerPvEMode : GameManagerAbstract
{

    protected override void InitGameMood(bool throwNewBall = true)
    {
        m_curPlayerTurn = PlayerIndex.First;
        m_playerData1.PlayerScript.StartTurn(throwNewBall);
        m_playerData2.PlayerScript?.LostTurn();
    }

    protected override void FinishedTutorial()
    {
        m_playerData2.PlayerScript?.ShowPlayer();
        base.FinishedTutorial();
    }

    protected override async void SwitchPlayerTurn(bool shouldSwitchTurn = true)
    {
        if (!m_inTutorial)
            m_gameCanvas.SwitchTurn(m_curPlayerTurn != PlayerIndex.First);

        if (m_curPlayerTurn != PlayerIndex.First)
        {//win player1
            m_playerData1.PlayerScript.Win();
            m_playerData2.PlayerScript?.Lose();
        }
        else
        {//win player2
            m_playerData1.PlayerScript.Lose();
            m_playerData2.PlayerScript?.Win();
        }

        await Task.Delay(2000);
        InitPlayersStatus();

        await Task.Delay(1000);
        SwitchPlayerTurnAfterWait(true, shouldSwitchTurn);
    }

    protected override void InitPlayers()
    {
        m_playerContainer = FindObjectOfType<PlayerContainer>(true);

        m_playerData1.PlayerScript = m_playerContainer.SpawnRegularPlayer();
        m_playerData1.PlayerScript.gameObject.SetActive(false);
        m_playerData1.PlayerScript.Init(m_playerData1);

        m_playerData2.PlayerScript = m_playerContainer.SpawnAutoPlayer();
        m_playerData2.PlayerScript.gameObject.SetActive(false);
        m_playerData2.AutoPlay = true;
        m_playerData2.Darker = true;
        m_playerData2.PlayerScript.Init(m_playerData2);

    }

    public override void onTurnLost()
    {
        PlayerArgs playerData;
        if (m_curPlayerTurn == PlayerIndex.First)
        {
            playerData = m_playerData1;
            EventManager.Broadcast(EVENT.EventOnBallLost);
            m_gameCanvas.SetSliderValue(0, m_gameArgs.ComboKicksAmount);
        }
        else
            playerData = m_playerData2;

        playerData.ComboSinceSpecialKick = 0;

        playerData.CurCombo = 0;
        playerData.PlayerScript.SetAllowedSpecialKick(false, true);
        m_gameCanvas.SetCombo(playerData.CurCombo);

        if (m_inTutorial)
            onTurnLostTutorial();
        else
        {
            playerData = playerData == m_playerData1 ? m_playerData2 : m_playerData1;
            playerData.CurScore++;
            if (playerData == m_playerData1)
                m_gameCanvas.CheerActivate();

            m_gameCanvas.SetNormalScore(m_playerData1.CurScore, m_playerData2.CurScore);
            if (m_timeIsOver)
                MatchEnd();
            else
                SwitchPlayerTurn(false);
        }
    }

    public override void OnBallHit(PlayerIndex playerIndex)
    {
        if (m_inTutorial)
            m_tutorialManager.OnBallHit();
        BroadcastKickType();

        m_gameCanvas.IncrementCombo();
        if (m_inTutorial && !m_tutorialManager.IsFreePlayMode())
            return;
        else
            SwitchPlayerTurnAfterWait(false);
        if (playerIndex == PlayerIndex.First)
            CheckPlayerCombo(playerIndex);

    }


    public override void GameIsOver()
    {
        //print("Time is over");
        if (!m_isGamePause)
        {
            SetGamePause(true);
            m_ballsManager.TimeIsOver();//should turn off the balls

            m_gameCanvas.OnPvEEnd(m_playerData1.CurScore, m_playerData2.CurScore);
        }
    }

    protected override void UpdatePlayerPrefsCompletedTutorial()
    {
        string playerPrefsGameTutorial = "CompletedTalTalTutorial";
        PlayerPrefs.SetInt(playerPrefsGameTutorial, 1);
    }


}
