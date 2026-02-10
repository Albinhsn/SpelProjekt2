using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class MovingPlatformController : MonoBehaviour
{
	[SerializeField] private int m_targetCount = 2;
	[Min(0)][SerializeField] private float m_speed = 1;
	[SerializeField] private int m_startIndex = 0;
	

	[SerializeField] private Transform m_platform;

	[SerializeField] private Transform[] m_targets = Array.Empty<Transform>();
	private int m_currentTarget = 0;
	private int m_currentPosition = 0;
	private Vector3 m_movementDirection;
	
	public bool moving => m_currentPosition != m_currentTarget;
	public event System.Action<Vector3> d_onDirectionChanged;
	
	private void OnValidate()
	{
		if (m_targetCount < 2) m_targetCount = 2;
		if (m_startIndex < 0) m_targetCount = 0;
		if (m_startIndex > m_targetCount - 1) m_startIndex = m_targetCount - 1;
		
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
		m_platform.transform.position = m_targets[m_startIndex].transform.position;
		m_currentPosition = m_startIndex;
		m_currentTarget = m_startIndex;
	}

	private void Update()
	{
		//Yes, it is unoptimized
		if (moving)
		{
			if ((m_targets[m_currentTarget].position - m_platform.position).magnitude < m_speed * Time.deltaTime)//Destination reached within next update;
			{
				m_platform.position = m_targets[m_currentTarget].position;
				m_currentPosition = m_currentTarget;
				d_onDirectionChanged?.Invoke(Vector3.zero);
			}

			m_platform.position += m_movementDirection * (m_speed * Time.deltaTime);//Move
		}
	}

	//Controls
	public void MoveToIndex(int index)
	{
		if (moving) return;//May be subject to change
		Assert.IsTrue(index < m_targetCount);

		if (index == m_currentTarget) return;
		
		m_currentTarget = index;
		m_movementDirection = (m_targets[m_currentTarget].position - m_platform.position).normalized;
		d_onDirectionChanged?.Invoke(m_movementDirection * m_speed);
		
	}
	public void IncrementPosition()
	{
		if (moving) return;
		MoveToIndex(m_currentPosition == m_targetCount - 1 ? 0 : m_currentPosition + 1);
	}
	public void DecrementPosition()
	{
		if (moving) return;
		MoveToIndex(m_currentPosition == 0 ? m_targetCount - 1 : m_currentPosition - 1);
	}
	
	
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawLineStrip(m_targets.Select(e => e.position).ToArray(), true);
	}
}