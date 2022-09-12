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

    public void AnimSetTrigger(string triggerName)
    {
        m_anim.SetTrigger(triggerName);
    }
    public void AnimSetAndResetTrigger(string triggerName)
    {
        m_anim.ResetTrigger(triggerName);
        m_anim.SetTrigger(triggerName);
    }

    public void WinAnim()
    {
        m_anim.enabled = true;
        m_anim.Play("Win", -1, 0f);
    }

    public void LoseAnim()
    {
        m_anim.enabled = true;
        m_anim.Play("Lose", -1, 0f);
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

    }

    public void SetGamePause(bool isPause)
    {
        m_anim.enabled = !isPause;
    }

}
