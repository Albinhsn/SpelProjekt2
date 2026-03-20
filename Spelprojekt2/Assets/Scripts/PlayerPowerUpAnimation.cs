using UnityEngine;

public class PlayerPowerUpAnimation : MonoBehaviour
{
    [SerializeField] AnimationCurve outlineAnimationCurve;
    [SerializeField] float animationTime;
    [SerializeField] bool testAnimationClip = false;
    float timer = -1;
    float animationT;
    SkinnedMeshRenderer sMR;
    void Start()
    {
        sMR = GetComponent<SkinnedMeshRenderer>();
        
    }
    void Update()
    {
        timer -= Time.deltaTime;
        animationT = Mathf.InverseLerp(animationTime,0,timer);
        if(timer >= 0 )
        {
            float alphaValue = outlineAnimationCurve.Evaluate(animationT);
            sMR.material.color = new Color(sMR.material.color.r,sMR.material.color.g,sMR.material.color.b,alphaValue);
            sMR.enabled = true;            
        }
        else if(sMR.enabled == true)
        {
            sMR.enabled = false;
        }

        //Debug
        if(testAnimationClip)
        {
            ActivateOutlineAnimation();
            testAnimationClip = false;
        }
    }
    public void ActivateOutlineAnimation()
    {
        timer = animationTime;
    }
}
