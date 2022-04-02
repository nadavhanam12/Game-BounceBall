using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    public delegate void onTouchScreen();
    public onTouchScreen OnTouchScreen;

    private List<Image> Panels;
    private int indexPanel = 0;
    public void InitAndPlay()
    {
        indexPanel = 0;
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

    public void NextPanel()
    {
        Panels[indexPanel].gameObject.SetActive(false);
        if (Panels.Count - 1 > indexPanel)
        {
            indexPanel++;
            Panels[indexPanel].gameObject.SetActive(true);
        }

    }

    public void FinishTutorial()
    {
        gameObject.SetActive(false);
    }
    public void TouchScreen()
    {
        OnTouchScreen();
    }
    public int GetCurIndex()
    {
        return indexPanel;
    }
    public void HideCurPanel()
    {
        Panels[indexPanel].gameObject.SetActive(false);
    }

}
