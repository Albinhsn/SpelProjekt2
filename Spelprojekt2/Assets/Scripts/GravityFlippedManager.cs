using UnityEngine;

public class GravityFlippedManager : MonoBehaviour
{

    public static bool m_unlocked = true;

    void Awake()
    {
#if UNITY_EDITOR
            GravityFlippedManager.m_unlocked = true;
#else
            GravityFlippedManager.m_unlocked = false;
#endif

    }

    void Update()
    {
        if(InputManager.FlipGravity())
        {
            // ah: flip each object that is not filtered
            GravityFlippedObject[] objs = FindObjectsByType<GravityFlippedObject>(FindObjectsSortMode.None);
            for(int i = 0; i < objs.Length; i++)
            {
                GravityFlippedObject obj = objs[i];
                FilterObject filter      = obj.gameObject.GetComponent<FilterObject>();

                bool can_flip            = filter == null || filter.m_kind != FilterManager.m_activeFilter;
                if(can_flip)
                {
                    obj.Flip();
                }

            }
        }
        
    }
}
