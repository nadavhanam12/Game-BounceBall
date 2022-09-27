using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchPointer : MonoBehaviour
{
    TrailRenderer m_trail;
    SpriteRenderer m_highlightBtn;
    bool m_gameEnded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    void Awake()
    {
        gameObject.SetActive(false);
        m_trail = GetComponentInChildren<TrailRenderer>(true);
        m_highlightBtn = GetComponentInChildren<SpriteRenderer>(true);
        m_gameEnded = false;
    }

    public void Init()
    {
        if (m_gameEnded) return;
        gameObject.SetActive(true);
    }

    public void Activate(Vector2 position)
    {
        if (m_gameEnded) return;
        transform.position = position;
        if (!this.isActiveAndEnabled)
        {
            gameObject.SetActive(true);
            HighlightBtn(m_highlightBtn.gameObject);
        }

    }

    public void StopTouch()
    {
        gameObject.SetActive(false);
        m_trail.Clear();
    }

    public void GameEnded()
    {
        m_gameEnded = true;
    }


    private void HighlightBtn(GameObject gameObject)
    {
        LeanTween.cancel(gameObject);
        gameObject.transform.position = gameObject.transform.position;
        gameObject.transform.localScale = Vector3.one;
        gameObject.SetActive(true);
        LeanTween.scale(gameObject, Vector3.one * 2f, 0.25f).setLoopPingPong();
    }

}
