using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    public delegate void onTouchScreen();
    public onTouchScreen OnTouchScreen;
    private GameCanvasScript m_gameCanvas;
    public List<GameObject> Panels;
    public GameObject PlayerSpeechBubble;
    public GameObject KickBtn;
    public GameObject SlideBtn;
    public GameObject JumpBtn;
    public GameObject NextBallIcon;
    public GameObject m_highlightBtn;
    private Animator m_AnimSpeechBubble;
    public void Init(GameCanvasScript gameCanvas)
    {
        m_gameCanvas = gameCanvas;

        DisablePanels();
        gameObject.SetActive(false);
        PlayerSpeechBubble.SetActive(false);
        m_AnimSpeechBubble = PlayerSpeechBubble.GetComponent<Animator>();

        m_highlightBtn.SetActive(false);
    }

    public void Play()
    {
        gameObject.SetActive(true);
        m_gameCanvas.ShowScoreDelta(false);
        m_gameCanvas.ShowComboAndNextBall(false);
        m_gameCanvas.ShowSkipButton(true);
        PlayerSpeechBubble.SetActive(true);
        KickBtn.SetActive(false);
        SlideBtn.SetActive(false);
        JumpBtn.SetActive(false);
    }

    void DisablePanels()
    {
        for (int i = 1; i < Panels.Count; i++)
        {
            Panels[i].gameObject.SetActive(false);
        }
    }

    public void OpenPanel(StageInTutorial curStageTutorial)
    {
        for (int i = 0; i < Panels.Count; i++)
        {
            Panels[i].SetActive(false);
        }
        //Panels[panelToOpen].gameObject.SetActive(true);
        switch (curStageTutorial)
        {
            case StageInTutorial.WelcomePlayerText:
                Panels[0].SetActive(true);
                break;
            case StageInTutorial.KickTheBallText:
                m_AnimSpeechBubble.SetTrigger("NextStage");
                break;
            case StageInTutorial.FirstKickGamePlay:
                m_AnimSpeechBubble.SetTrigger("NextStage");
                KickBtn.SetActive(true);
                HighlightBtn(KickBtn);
                break;
            case StageInTutorial.BallSplitText1:
                m_AnimSpeechBubble.SetTrigger("NextStage");
                break;
            case StageInTutorial.BallSplitText2:
                HighlightBtn(NextBallIcon);
                m_AnimSpeechBubble.SetTrigger("NextStage");
                break;
            case StageInTutorial.PracticeKickGamePlay:
                m_AnimSpeechBubble.SetTrigger("NextStage");
                break;


            default:

                break;

        }


    }

    public List<GameObject> GetPanels()
    {
        return Panels;
    }

    public void FinishTutorial()
    {
        gameObject.SetActive(false);
        m_gameCanvas.ShowSkipButton(false);
        ShowScoreDelta(true);
        ShowComboAndNextBall(true);
    }
    public void TouchScreen()
    {
        //print("TouchScreen");
        OnTouchScreen();
    }

    public void HidePanel(int panelToHide)
    {
        Panels[panelToHide].gameObject.SetActive(false);
    }
    public void ShowScoreDelta(bool shouldShow)
    {
        m_gameCanvas.ShowScoreDelta(shouldShow);
    }
    public void ShowComboAndNextBall(bool shouldShow)
    {
        m_gameCanvas.ShowComboAndNextBall(shouldShow);
    }

    private void HighlightBtn(GameObject gameObject)
    {
        m_highlightBtn.transform.position = gameObject.transform.position;
        m_highlightBtn.SetActive(true);
        LeanTween.scale(m_highlightBtn, Vector3.one * 1.5f, 0.75f).setLoopPingPong();
    }


}
