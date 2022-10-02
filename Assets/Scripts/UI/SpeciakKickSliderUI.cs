using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SpeciakKickSliderUI : MonoBehaviour
{
    Slider m_slider;
    float m_updateValue;
    [SerializeField][Range(0, 0.02f)] float valuePerFrame;


    public void Init()
    {
        m_slider = GetComponent<Slider>();
        m_slider.value = 0;
        m_updateValue = 0;
    }

    public void SetSliderValue(int amount, int maxAmount)
    {
        float value = (float)amount / (float)maxAmount;
        //print("value " + value);
        m_updateValue = value;

    }

    void Update()
    {
        if (!m_slider) return;
        float sliderValue = m_slider.value;
        if (Math.Abs(sliderValue - m_updateValue) > valuePerFrame)
            if (sliderValue > m_updateValue)
            {
                m_slider.value -= valuePerFrame * 2;
            }
            else if (sliderValue < m_updateValue)
            {
                m_slider.value += valuePerFrame;
            }


    }
}
