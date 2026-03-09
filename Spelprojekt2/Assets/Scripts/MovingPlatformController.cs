using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using AudioKit.FMOD;
using FMODUnity;

public class MovingPlatformController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioCueSO m_onPlatformStartMoveCue;

    [SerializeField] private AudioCueSO m_onPlatformMoveCue;
    private FMOD.Studio.EventInstance m_soundInstance;



    [Header("Platform settings")]
	[SerializeField] private int m_targetCount = 2;
	[Min(0)][SerializeField] private float m_speed = 1;
	[SerializeField] private int m_startIndex = 0;
	

	[SerializeField] private MovingPlatformReceiver m_platform;

	[SerializeField] private Transform[] m_targets = Array.Empty<Transform>();
	private int m_currentTarget = 0;
	private int m_currentPosition = 0;
	private Vector3 m_movementDirection;
	
	public bool moving => m_currentPosition != m_currentTarget;
    private FilterObject m_filter;

    void OnDestroy()
    {
    }
	
	private void OnValidate()
	{
		if (m_targetCount < 2) m_targetCount = 2;
		if (m_startIndex < 0) m_targetCount = 0;
		if (m_startIndex > m_targetCount - 1) m_startIndex = m_targetCount - 1;

		m_platform = GetComponentInChildren<MovingPlatformReceiver>();
		if (m_platform is null) Debug.LogError("MovingPlatformController found no moving platform child object. pls add one");
		
		if (m_targets.Length < m_targetCount) //Add new target gizmos
		{
			Transform[] new_targets = new Transform[m_targetCount];
			for (int a = 0; a < m_targetCount; a++) 
			{
				if (a < m_targets.Length)
				{
					new_targets[a] = m_targets[a];
					continue;
				}
				
				new_targets[a] = new GameObject($"Destination {a}").transform;
				new_targets[a].parent = transform;
				new_targets[a].localPosition = Vector3.zero;
			}

			m_targets = new_targets;
		}
		else if (m_targets.Length > m_targetCount)//Destroy unused targets
		{
			Transform[] new_targets = new Transform[m_targetCount];
			for (int a = m_targets.Length - 1; a >= 0; a--) 
			{
				if (a < m_targetCount)
				{
					new_targets[a] = m_targets[a];
				}
				else
				{
					StartCoroutine(DestroyObject(m_targets[a].gameObject));
				}
			}

			m_targets = new_targets;
		}

		m_platform.transform.position = m_targets[m_startIndex].transform.position;
		m_currentPosition = m_startIndex;
		m_currentTarget = m_startIndex;
	}

	private IEnumerator DestroyObject(GameObject obj)
	{
		yield return null;
		DestroyImmediate(obj);
		yield break;
	}

	private void Awake()
	{

		m_platform = GetComponentInChildren<MovingPlatformReceiver>();
		if (m_platform is null) Debug.LogError("MovingPlatformController found no moving platform child object. pls add one");
		m_platform.UpdateReference(Vector3.zero);
		
		m_platform.transform.position = m_targets[m_startIndex].transform.position;
		m_currentPosition = m_startIndex;
		m_currentTarget = m_startIndex;

        m_filter = GetComponentInChildren<FilterObject>();
	}

    private bool IsFiltered()
    {
        return m_filter != null && m_filter.m_kind == FilterManager.m_activeFilter;
    }

	private void Update()
	{
		//Yes, it is unoptimized
		if (moving && !IsFiltered())
		{
            m_soundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(m_platform.transform.position));

			if (Vector3.Dot(m_movementDirection, m_platform.transform.position) > 
                Vector3.Dot(m_movementDirection, m_targets[m_currentTarget].position)) //Destination reached within next update;
			{
				m_platform.transform.position = m_targets[m_currentTarget].position;
				m_currentPosition = m_currentTarget;
				m_platform.UpdateReference(Vector3.zero);

                m_soundInstance.setParameterByName("looping", 0);
                m_soundInstance.release();

			}

			m_platform.rb.position += m_movementDirection * (m_speed * Time.deltaTime);//Move
		}
	}

	//Controls
	public void MoveToIndex(int index, Transform activator_transform)
	{
        SfxDirector.PlayCue2(m_onPlatformStartMoveCue, activator_transform.position);
        m_soundInstance = RuntimeManager.CreateInstance(m_onPlatformMoveCue.evt);
        m_soundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(m_platform.transform.position));
        m_soundInstance.setParameterByName("looping", 1);
        m_soundInstance.start();

		Assert.IsTrue(index < m_targetCount);

		if (index == m_currentTarget) return;


		m_currentPosition = m_currentTarget;
		m_currentTarget = index;
		m_movementDirection = (m_targets[m_currentTarget].position - m_platform.transform.position).normalized;
		m_platform.UpdateReference(m_movementDirection * m_speed);
		
	}
	public void IncrementPosition(Transform source)
	{
		if (moving) return;
		MoveToIndex(m_currentPosition == m_targetCount - 1 ? 0 : m_currentPosition + 1, source);
	}

	public void DecrementPosition(Transform source)
	{
		if (moving) return;
		MoveToIndex(m_currentPosition == 0 ? m_targetCount - 1 : m_currentPosition - 1, source);
	}
	
	
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawLineStrip(m_targets.Select(e => e.position).ToArray(), true);
	}
}
