using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameScreen : MonoBehaviour
{
    [SerializeField] TMP_Text m_scoreText;
    [SerializeField] TMP_Text m_onePlayerGameNewScoreText;
    [SerializeField] TMP_Text m_onePlayerGameOldScoreText;

    [SerializeField] TMP_Text m_twoPlayerGameWinText;
    [SerializeField] TMP_Text m_twoPlayerGameLoseText;
    [SerializeField] TMP_Text m_playerOneScoreText;
    [SerializeField] TMP_Text m_playerTwoScoreText;
    [SerializeField] Button m_resetButton;


    int m_curScore1 = 0;
    int m_curScore2 = 0;
    [SerializeField] float m_countUpDuration;
    public void Activate(bool isNewBestScore, int scoreToShow)//its for Single Player Mode
    {
        m_twoPlayerGameWinText.gameObject.SetActive(false);
        m_twoPlayerGameLoseText.gameObject.SetActive(false);
        m_playerOneScoreText.gameObject.SetActive(false);
        m_playerTwoScoreText.gameObject.SetActive(false);

        m_onePlayerGameNewScoreText.gameObject.SetActive(isNewBestScore);
        m_onePlayerGameOldScoreText.gameObject.SetActive(!isNewBestScore);
        m_scoreText.gameObject.SetActive(true);
        m_curScore1 = 0;
        gameObject.SetActive(true);
        StartCoroutine(ScoreCoroutine1(scoreToShow));
    }

    IEnumerator ScoreCoroutine1(int scoreToShow)
    {
        while (m_curScore1 <= scoreToShow)
        {
            m_scoreText.text = m_curScore1.ToString();
            m_curScore1++;
            yield return new WaitForSeconds(m_countUpDuration / scoreToShow);
        }
    }
    IEnumerator ScoreCoroutine2(int scoreToShow1, int scoreToShow2)
    {
        int higherScore = Math.Max(scoreToShow1, scoreToShow2);
        while ((m_curScore1 <= scoreToShow1) || (m_curScore2 <= scoreToShow2))
        {
            if (m_curScore1 <= scoreToShow1)
            {
                m_playerOneScoreText.text = m_curScore1.ToString();
                m_curScore1++;
            }
            if (m_curScore2 <= scoreToShow2)
            {
                m_playerTwoScoreText.text = m_curScore2.ToString();
                m_curScore2++;
            }

            yield return new WaitForSeconds(m_countUpDuration / higherScore);
        }
    }

    public void ShareScore()
    {

    }

    internal void OnPvEEnd(bool victory, int playerOneScore, int playerTwoScore)
    {
        m_onePlayerGameNewScoreText.gameObject.SetActive(false);
        m_onePlayerGameOldScoreText.gameObject.SetActive(false);
        m_scoreText.gameObject.SetActive(false);

        m_playerOneScoreText.gameObject.SetActive(true);
        m_playerTwoScoreText.gameObject.SetActive(true);
        m_twoPlayerGameWinText.gameObject.SetActive(victory);
        m_twoPlayerGameLoseText.gameObject.SetActive(!victory);
        m_curScore1 = 0;
        m_curScore2 = 0;
        gameObject.SetActive(true);
        StartCoroutine(ScoreCoroutine2(playerOneScore, playerTwoScore));
    }

    internal void OnPvPEnd(bool victory, int playerOneScore, int playerTwoScore)
    {
        m_onePlayerGameNewScoreText.gameObject.SetActive(false);
        m_onePlayerGameOldScoreText.gameObject.SetActive(false);
        m_scoreText.gameObject.SetActive(false);

        m_playerOneScoreText.gameObject.SetActive(true);
        m_playerTwoScoreText.gameObject.SetActive(true);
        m_twoPlayerGameWinText.gameObject.SetActive(victory);
        m_twoPlayerGameLoseText.gameObject.SetActive(!victory);
        m_curScore1 = 0;
        m_curScore2 = 0;
        m_resetButton.gameObject.SetActive(false);
        gameObject.SetActive(true);
        StartCoroutine(ScoreCoroutine2(playerOneScore, playerTwoScore));
    }
}
