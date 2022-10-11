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
    int m_tweenId = -1;
    ParticleSystem m_particleSystem;
    bool onTutorialPlayParticles;

    public void Init()
    {
        m_slider = GetComponent<Slider>();
        m_particleSystem = GetComponentInChildren<ParticleSystem>();
        m_slider.value = 0;
        m_updateValue = 0;
        onTutorialPlayParticles = false;
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
        if (sliderValue > 0.1f || onTutorialPlayParticles)
        {
            m_particleSystem.Emit(5);
        }
        if (m_tweenId != -1) return;
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

    public void ActivateFillingAnim(bool active)
    {
        if (active)
        {
            onTutorialPlayParticles = true;
            m_tweenId = LeanTween.value(gameObject, 0f, 1f, 1f)
        .setOnUpdate((float val) => { m_slider.value = val; })
            .setLoopPingPong(999).id;
        }
        else
        {
            LeanTween.cancel(m_tweenId);
            m_tweenId = -1;
            SetSliderValue(0, 1);
            onTutorialPlayParticles = false;

        }
    }
}
