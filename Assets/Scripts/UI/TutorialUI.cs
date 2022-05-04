using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    public delegate void onTouchScreen();
    public onTouchScreen OnTouchScreen;
    private GameCanvasScript m_gameCanvas;
    private Button m_buttonNext;
    [SerializeField] GameObject m_basicKickPNG;
    [SerializeField] GameObject m_KickKickPNG;


    private List<Image> Panels;
    public void Init(GameCanvasScript gameCanvas)
    {
        m_gameCanvas = gameCanvas;
        m_buttonNext = GetComponentInChildren<Button>();

        InitPanels();
        gameObject.SetActive(false);
    }

    public void Play()
    {
        gameObject.SetActive(true);
        m_gameCanvas.ShowScoreDelta(false);
        m_gameCanvas.ShowComboAndNextBall(false);
        m_gameCanvas.ShowSkipButton(true);
    }

    void InitPanels()
    {
        Panels = GetComponentsInChildren<Image>(true).ToList();
        Panels[0].gameObject.SetActive(true);
        for (int i = 1; i < Panels.Count; i++)
        {
            Panels[i].gameObject.SetActive(false);
        }
        m_buttonNext.gameObject.SetActive(true);
    }

    public void OpenPanel(int panelToOpen)
    {
        for (int i = 0; i < Panels.Count; i++)
        {
            Panels[i].gameObject.SetActive(false);
        }
        Panels[panelToOpen].gameObject.SetActive(true);
        m_buttonNext.gameObject.SetActive(true);
        TogglePNG(true);


    }
    private void TogglePNG(bool toShow)
    {
        m_basicKickPNG.gameObject.SetActive(toShow);
        m_KickKickPNG.gameObject.SetActive(toShow);

    }

    public List<Image> GetPanels()
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
    public void HideButton()
    {
        m_buttonNext.gameObject.SetActive(false);
        TogglePNG(false);
    }

    public void HidePanel(int panelToHide)
    {
        Panels[panelToHide].gameObject.SetActive(false);
        m_buttonNext.gameObject.SetActive(false);
    }
    public void ShowScoreDelta(bool shouldShow)
    {
        m_gameCanvas.ShowScoreDelta(shouldShow);
    }
    public void ShowComboAndNextBall(bool shouldShow)
    {
        m_gameCanvas.ShowComboAndNextBall(shouldShow);
    }

}
