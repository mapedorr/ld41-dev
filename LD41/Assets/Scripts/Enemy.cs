using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject
{
	// ══════════════════════════════════════════════════════════════ PUBLICS ════
	public Transform patrolContainer;
	public bool closedPattern = false;
	public float startWaitTime;
	public float waitTimeBeforeNext;
	public float viewDistance;
	public LayerMask walkableLayer;
	public bool playerOnSight = false;

	// ═══════════════════════════════════════════════════════════ PROPERTIES ════
	// ═════════════════════════════════════════════════════════════ PRIVATES ════
	Vector2 m_initialPos;
	List<Vector2> m_patrolCoordinates = new List<Vector2> ();
	bool m_firstPatrol = true;
	Vector2 m_currentSpotTarget;
	List<Floor> m_viewZone = new List<Floor> ();
	Color m_defaultWalkableColor;
	protected GameManager m_gameManager;

	// ══════════════════════════════════════════════════════════════ METHODS ════
	protected override void Awake ()
	{
		base.Awake ();
		m_gameManager = Object.FindObjectOfType<GameManager> ().GetComponent<GameManager> ();
		m_currentSpotTarget = transform.position;
		// store the starting position in case it is needed to close an open patrol
		// pattern
		m_initialPos = Coordinate;

		if (patrolContainer != null && patrolContainer.childCount > 0)
		{
			for (int i = 0; i < patrolContainer.childCount; i++)
			{
				m_patrolCoordinates.Add (Utility.Vector2Round (patrolContainer.GetChild (i).position));
			}

			if (!closedPattern)
			{
				// add to the list the spots in reverse till the initial position to close
				// the patrol pattern
				for (int i = patrolContainer.childCount - 2; i >= 0; i--)
				{
					m_patrolCoordinates.Add (Utility.Vector2Round (patrolContainer.GetChild (i).position));
				}

				m_patrolCoordinates.Add (m_initialPos);
			}

			m_currentSpotTarget = m_patrolCoordinates[0];
		}
	}

	protected override void Update ()
	{
		base.Update ();

		if (playerOnSight || m_gameManager.IsGameOver)
		{
			return;
		}

		DrawViewZone (false);
		m_viewZone.Clear ();

		// Vector2 viewRange = Coordinate + ((m_currentSpotTarget - Coordinate).normalized) * viewDistance;
		// RaycastHit2D[] hit = Physics2D.LinecastAll (transform.position,
		// 	(m_currentSpotTarget.magnitude > viewRange.magnitude) ? viewRange : m_currentSpotTarget,
		// 	walkableLayer);

		RaycastHit2D[] hit = Physics2D.LinecastAll (transform.position,
			m_currentSpotTarget,
			walkableLayer);

		if (hit != null && hit.Length > 0)
		{
			int addedCount = 0;
			for (int i = 1; i < hit.Length; i++)
			{
				Floor hitComponent = hit[i].transform.GetComponent<Floor> ();
				// if (hitComponent != null)
				if (hitComponent != null && addedCount < viewDistance)
				{
					addedCount++;
					m_viewZone.Add (hitComponent);
				}
			}

			DrawViewZone (true);

			// check if the player is inside the view zone
			foreach (Floor zone in m_viewZone)
			{
				if (zone.CheckPlayerOverlap ())
				{
					iTween.Stop (gameObject);
					playerOnSight = true;
				}
			}
		}
	}

	void DrawViewZone (bool mark)
	{
		foreach (Floor zone in m_viewZone)
		{
			if (mark)
			{
				zone.SetColor (new Color (1f, 0f, 0f, 0.3f));
			}
			else
			{
				zone.ResetColor ();
			}
		}
	}

	public IEnumerator StartPatrolling ()
	{
		if (m_patrolCoordinates.Count == 0)
		{
			yield return null;
		}

		foreach (Vector2 spotCoordinate in m_patrolCoordinates)
		{
			if (playerOnSight || m_gameManager.IsGameOver)
			{
				yield return null;
			}

			m_currentSpotTarget = spotCoordinate;
			yield return base.MoveRoutine (spotCoordinate,
				(m_firstPatrol) ? startWaitTime : waitTimeBeforeNext);
			m_firstPatrol = false;
		}

		StartCoroutine (StartPatrolling ());
	}

	protected override void OnCantMove<T> (T component)
	{
		Debug.Log ("Enemy? " + component.name);
		// TODO: set behaviour for collisions with objects with interaction
	}

	void OnDrawGizmos ()
	{
		if (m_currentSpotTarget.magnitude > 0f)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawRay (transform.position, (m_currentSpotTarget - Coordinate).normalized);
		}
	}

	public void StopPatrolling ()
	{
		iTween.Stop (gameObject);
	}
}