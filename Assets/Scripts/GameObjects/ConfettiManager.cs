using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfettiManager : MonoBehaviour
{
    [SerializeField] GameObject leftConfetti;
    [SerializeField] GameObject rightConfetti;

    private ParticleSystem[] m_arrayConfetti;

    public void Init(GameBounds bounds)
    {
        Vector3 posLeft = leftConfetti.transform.position;
        posLeft.x = bounds.GameLeftBound;
        leftConfetti.transform.position = posLeft;

        Vector3 posRight = rightConfetti.transform.position;
        posRight.x = bounds.GameRightBound;
        rightConfetti.transform.position = posRight;

        m_arrayConfetti = GetComponentsInChildren<ParticleSystem>();
    }

    public void Activate()
    {
        foreach (ParticleSystem particleSystem in m_arrayConfetti)
        {
            particleSystem.Play();
        }
    }
}
