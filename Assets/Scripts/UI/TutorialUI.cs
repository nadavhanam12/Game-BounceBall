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

    private List<Image> Panels;
    public void InitAndPlay(GameCanvasScript gameCanvas)
    {
        m_gameCanvas = gameCanvas;
        m_gameCanvas.ShowScoreDelta(false);
        m_gameCanvas.ShowComboAndNextBall(false);
        InitPanels();
        gameObject.SetActive(true);
    }

    void InitPanels()
    {
        Panels = GetComponentsInChildren<Image>(true).ToList();
        Panels[0].gameObject.SetActive(true);
        for (int i = 1; i < Panels.Count; i++)
        {
            Panels[i].gameObject.SetActive(false);
        }
    }

    public void OpenPanel(int panelToOpen)
    {
        for (int i = 1; i < Panels.Count; i++)
        {
            Panels[i].gameObject.SetActive(false);
        }
        Panels[panelToOpen].gameObject.SetActive(true);



    }

    public void FinishTutorial()
    {
        gameObject.SetActive(false);
    }
    public void TouchScreen()
    {
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

}
