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
	PatrolType m_patrolType;

	// ══════════════════════════════════════════════════════════════ METHODS ════
	protected override void Awake ()
	{
		base.Awake ();
		m_currentSpotTarget = transform.position;

		if (patrolContainer != null && patrolContainer.childCount > 0)
		{
			m_patrolType = patrolContainer.GetComponent<Patrol> ().patrolType;

			for (int i = 0; i < patrolContainer.childCount; i++)
			{
				m_patrolCoordinates.Add (Utility.Vector2Round (patrolContainer.GetChild (i).position));
			}

			if (!closedPattern && m_patrolType == PatrolType.GO)
			{
				// store the starting position in case it is needed to close an open pattern
				m_initialPos = Coordinate;

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
			// stop any iTween running on the object
			iTween.Stop (gameObject);
			return;
		}

		DrawViewZone (false);
		m_viewZone.Clear ();

		RaycastHit2D[] hit = Physics2D.LinecastAll (transform.position,
			m_currentSpotTarget,
			walkableLayer);

		if (hit != null && hit.Length > 0)
		{
			// counter that indicates how many tiles will the enemy watch from all of
			// the tiles hit by the ray
			int addedCount = 0;

			// ignore the first hit in the array since it is the tile in which the
			// enemy is standing
			for (int i = 1; i < hit.Length; i++)
			{
				Floor hitComponent = hit[i].transform.GetComponent<Floor> ();
				if (hitComponent != null && addedCount < viewDistance)
				{
					addedCount++;
					m_viewZone.Add (hitComponent);
				}
			}

			// mark the tiles that are in the view zone of the enemy
			DrawViewZone (true);

			// check if the player is inside the view zone
			foreach (Floor zone in m_viewZone)
			{
				if (zone.CheckPlayerOverlap ())
				{
					iTween.Stop (gameObject);
					playerOnSight = true;
					m_gameManager.PlayerDetected ();
					break;
				}
			}
		}
	}

	// make the tiles that are being watched to change their color
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

	// method that triggers the execution of the patrolling behaviour. This function
	// will keep calling itself until the current level ends
	public IEnumerator StartPatrolling ()
	{
		if (m_patrolCoordinates.Count == 0 || playerOnSight || m_gameManager.IsGameOver)
		{
			// if the level has ended, stop patrolling
			yield return null;
		}

		foreach (Vector2 spotCoordinate in m_patrolCoordinates)
		{
			if (!m_gameManager.IsGameOver)
			{
				m_currentSpotTarget = spotCoordinate;

				// based on the type of the patrol, the enemy will walk between its spots
				// or just look in their direction (without moving, without breathing)
				if (m_patrolType == PatrolType.GO)
				{
					yield return base.MoveRoutine (spotCoordinate,
						(m_firstPatrol) ? startWaitTime : waitTimeBeforeNext);
					m_firstPatrol = false;
				}
				else if (m_patrolType == PatrolType.WATCH)
				{
					yield return new WaitForSeconds (waitTimeBeforeNext);
				}
			}
		}

		StartCoroutine (StartPatrolling ());
	}

	// stop any tween running on the GameObject
	public void StopPatrolling ()
	{
		iTween.Stop (gameObject);
		DrawViewZone (false);
	}

	protected override void OnCantMove<T> (T component)
	{
		// TODO: set behaviour for collisions with other objects with interaction
	}

	// visual clue for the Editor that will help to know in which direction is
	// "pointing (or looking at)" the enemy
	void OnDrawGizmos ()
	{
		if (m_currentSpotTarget.magnitude > 0f)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawRay (transform.position, (m_currentSpotTarget - Coordinate).normalized);
		}

		if (patrolContainer.GetComponent<Patrol> ().patrolType == PatrolType.GO)
		{
			Gizmos.color = Color.red;
			for (int i = 0; i < patrolContainer.childCount; i++)
			{
				if (i == 0)
				{
					Gizmos.DrawLine (patrolContainer.GetChild (i).position, transform.position);
				}
				else
				{
					Gizmos.DrawLine (patrolContainer.GetChild (i).position, patrolContainer.GetChild (i - 1).position);
				}
			}
		}
	}
}