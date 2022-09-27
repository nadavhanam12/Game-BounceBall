using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RestartUI : MonoBehaviour
{
    [SerializeField] private Sprite restartImage;
    [SerializeField] private Sprite skipImage;


    public void ShowSkipButton(bool toShow)
    {
        if (toShow)
        {
            GetComponent<Image>().sprite = skipImage;
        }
        else
        {
            GetComponent<Image>().sprite = restartImage;
        }
    }

    public void DisableButton()
    {
        Color color = GetComponent<Image>().color;
        color.a = 0.5f;
        GetComponent<Image>().color = color;
        Destroy(GetComponent<EventTrigger>());
    }
}
