using System.Collections;
using TMPro;
using UnityEngine;

public class EndGameScreen : MonoBehaviour
{
    [SerializeField] TMP_Text m_scoreText;
    [SerializeField] TMP_Text m_newScoreText;
    [SerializeField] TMP_Text m_oldScoreText;
    int m_curScore = 0;
    [SerializeField] float m_countUpDuration;
    public void Activate(bool isNewBestScore, int scoreToShow)
    {
        m_newScoreText.gameObject.SetActive(isNewBestScore);
        m_oldScoreText.gameObject.SetActive(!isNewBestScore);
        m_curScore = 0;
        gameObject.SetActive(true);
        StartCoroutine(ScoreCoroutine(scoreToShow));
    }

    IEnumerator ScoreCoroutine(int scoreToShow)
    {
        //scoreToShow = 20;
        while (m_curScore <= scoreToShow)
        {
            m_scoreText.text = m_curScore.ToString();
            m_curScore++;
            print(m_countUpDuration / scoreToShow);
            yield return new WaitForSeconds(m_countUpDuration / scoreToShow);
        }
    }

    public void ShareScore()
    {

    }
}
