using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    public GameObject Player2Location;
    public GameObject NextPlayerIcon;
    public GameObject Player1Score;
    public GameObject Player2Score;
    public GameObject m_highlightBtn;
    public GameObject m_highlightBtn2;
    private Animator m_AnimSpeechBubble;

    HandGesturesUI m_handGestures;

    public TMP_Text m_welcomeText;

    public void Init(GameCanvasScript gameCanvas)
    {
        m_gameCanvas = gameCanvas;

        DisablePanels();
        SetPlayerName();
        gameObject.SetActive(false);
        PlayerSpeechBubble.SetActive(false);
        m_AnimSpeechBubble = PlayerSpeechBubble.GetComponent<Animator>();

        m_highlightBtn.SetActive(false);
        m_highlightBtn2.SetActive(false);


        m_handGestures = GetComponentInChildren<HandGesturesUI>();
        m_handGestures.Init();
    }
    void SetPlayerName()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            string name = PlayerPrefs.GetString("PlayerName");
            string text = m_welcomeText.text;
            text = text.Replace("player", name);
            m_welcomeText.text = text;
        }
    }
    public void Play()
    {
        gameObject.SetActive(true);
        m_gameCanvas.ShowSkipButton(true);

        m_gameCanvas.ShowScoreDelta(false);
        //m_gameCanvas.ShowComboAndNextBall(false);
        PlayerSpeechBubble.SetActive(true);

        //KickBtn.SetActive(false);
        SlideBtn.SetActive(false);
        JumpBtn.SetActive(false);
    }

    void DisablePanels()
    {
        for (int i = 0; i < Panels.Count; i++)
        {
            Panels[i].gameObject.SetActive(false);
        }
    }

    public void OpenPanel(StageInTutorial curStageTutorial)
    {

        //Panels[panelToOpen].gameObject.SetActive(true);
        switch (curStageTutorial)
        {
            case StageInTutorial.WelcomePlayerText:
                //Panels[0].SetActive(true);
                m_gameCanvas.ToggleAllInput(false);
                break;
            case StageInTutorial.KickTheBallText:
                m_AnimSpeechBubble.SetTrigger("NextStage");
                break;
            case StageInTutorial.FirstKickGamePlay:
                Panels[0].SetActive(true);
                m_AnimSpeechBubble.SetTrigger("NextStage");
                KickBtn.SetActive(true);
                m_gameCanvas.ToggleSingleInput("Move", true);
                m_gameCanvas.ToggleSingleInput("RegKick", true);
                m_handGestures.PlayRegularKickGesture();
                break;
            case StageInTutorial.BallSplitText1:
                Panels[0].SetActive(false);
                m_gameCanvas.ToggleAllInput(false);
                m_AnimSpeechBubble.SetTrigger("NextStage");
                m_handGestures.StopGesture();
                break;
            case StageInTutorial.BallSplitText2:
                HighlightBtn(NextBallIcon);
                m_AnimSpeechBubble.SetTrigger("NextStage");
                break;
            case StageInTutorial.PracticeKickGamePlay:
                m_gameCanvas.ToggleSingleInput("Move", true);
                m_gameCanvas.ToggleSingleInput("RegKick", true);
                Panels[1].SetActive(true);
                m_AnimSpeechBubble.SetTrigger("NextStage");
                m_handGestures.PlayRegularKickGesture();
                break;
            case StageInTutorial.PracticeKickFinishText:
                m_gameCanvas.ToggleAllInput(false);
                Panels[1].SetActive(false);
                m_highlightBtn.SetActive(false);
                m_AnimSpeechBubble.SetTrigger("NextStage");
                m_handGestures.StopGesture();
                break;
            case StageInTutorial.SlideIntroductionText:
                SlideBtn.SetActive(true);
                m_AnimSpeechBubble.SetTrigger("NextStage");
                break;
            case StageInTutorial.SlideExplanationText:
                m_AnimSpeechBubble.SetTrigger("NextStage");
                break;
            case StageInTutorial.PracticeSlideGamePlay:
                m_gameCanvas.ToggleSingleInput("Slide", true);
                m_AnimSpeechBubble.SetTrigger("NextStage");
                Panels[2].SetActive(true);
                m_handGestures.PlaySlideKickGesture();
                break;
            case StageInTutorial.PracticeSlideFinishText:
                m_gameCanvas.ToggleAllInput(false);
                m_AnimSpeechBubble.SetTrigger("NextStage");
                Panels[2].SetActive(false);
                m_handGestures.StopGesture();
                break;
            case StageInTutorial.OpponentAppears:
                m_AnimSpeechBubble.SetTrigger("NextStage");
                JumpBtn.SetActive(true);
                HighlightBtn(Player2Location);
                break;
            case StageInTutorial.TurnsExplanationText:
                m_AnimSpeechBubble.SetTrigger("NextStage");

                break;
            case StageInTutorial.TurnsUIExplanationText:
                HighlightBtn(NextPlayerIcon);
                m_AnimSpeechBubble.SetTrigger("NextStage");

                break;
            case StageInTutorial.PracticeOpponentGamePlay:
                m_gameCanvas.ToggleAllInput(true);
                m_AnimSpeechBubble.SetTrigger("NextStage");
                Panels[3].SetActive(true);
                break;
            case StageInTutorial.PointsMechanismText:
                m_gameCanvas.ToggleAllInput(false);
                m_AnimSpeechBubble.SetTrigger("NextStage");
                Panels[3].SetActive(false);
                HighlightBtn(Player1Score);
                HighlightBtn2(Player2Score);
                break;
            case StageInTutorial.WinStateText:
                m_AnimSpeechBubble.SetTrigger("NextStage");
                break;
            case StageInTutorial.BounceThatBallText:
                m_AnimSpeechBubble.SetTrigger("NextStage");
                m_gameCanvas.ToggleAllInput(true);
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
        LeanTween.cancel(m_highlightBtn);
        m_highlightBtn.transform.position = gameObject.transform.position;
        m_highlightBtn.transform.localScale = Vector3.one;
        m_highlightBtn.SetActive(true);
        LeanTween.scale(m_highlightBtn, Vector3.one * 1.5f, 0.25f).setLoopPingPong();
    }
    private void HighlightBtn2(GameObject gameObject)
    {
        LeanTween.cancel(m_highlightBtn2);
        m_highlightBtn2.transform.position = gameObject.transform.position;
        m_highlightBtn2.transform.localScale = Vector3.one;
        m_highlightBtn2.SetActive(true);
        LeanTween.scale(m_highlightBtn2, Vector3.one * 1.5f, 0.25f).setLoopPingPong();
    }


}
