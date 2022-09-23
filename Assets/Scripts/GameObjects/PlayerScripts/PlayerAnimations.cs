using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    private Animator m_anim;


    public void Init()
    {
        m_anim = gameObject.GetComponent<Animator>();
        m_anim.speed = 1;
    }


    public void RunAnim()
    {
        AnimatorClipInfo[] animatorinfo = m_anim.GetCurrentAnimatorClipInfo(0);
        if (animatorinfo.Length > 0)
        {
            string current_animation = animatorinfo[0].clip.name;
            if (current_animation != "Run")
                AnimSetTrigger("Running Trigger");

        }
    }

    public virtual void AnimSetTrigger(string triggerName)
    {
        if (!m_anim.GetBool(triggerName))
            m_anim.SetTrigger(triggerName);
    }

    public virtual void WinAnim()
    {
        m_anim.enabled = true;
        m_anim.Play("Win", -1, 0f);
    }

    public virtual void LoseAnim()
    {
        m_anim.enabled = true;
        m_anim.Play("Lose", -1, 0f);
    }
    public void StartIdle()
    {
        AnimSetTrigger("Idle Trigger");
    }

    public void OnPlayIdle()
    {
        m_anim.enabled = true;
        AnimatorClipInfo[] animatorinfo = m_anim.GetCurrentAnimatorClipInfo(0);
        if (animatorinfo.Length > 0)
        {
            string current_animation = animatorinfo[0].clip.name;
            if (current_animation != "Idle")
            {
                AnimSetTrigger("Idle Trigger");
                //m_anim.Play("Idle", -1, 0f);
            }
        }
        else
            StartIdle();
    }

    public void SetGamePause(bool isPause)
    {
        m_anim.enabled = !isPause;
    }

}
