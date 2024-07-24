using UnityEngine;
using UnityHelper.Templates;

public class PostProcessingAnimation : SingletonMono<PostProcessingAnimation>
{
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Blur()
    {
        anim.Play("Blur");
    }

    public void UnBlur()
    {
        anim.Play("Blur Rev");
    }
}
