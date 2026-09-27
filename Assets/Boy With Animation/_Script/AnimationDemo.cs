using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationDemo : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private int animIndex = 0;
    [SerializeField]
    private int maxAnim = 16;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ChangeAimation(bool _next = true)
    {
        if (_next)
        {
            animIndex++;
            if (animIndex >= maxAnim)
                animIndex = 0;
        }
        else
        {
            animIndex--;
            if (animIndex < 0)
                animIndex = maxAnim;
        }
        animator.SetInteger("AnimIndex", animIndex);
        // Debug.Log("AnimIndex: " + animIndex);
    }

    public void BtnNextAnim()
    {
        ChangeAimation(true);
    }
    public void BtnPrevAnim()
    {
        ChangeAimation(false);
    }
}
