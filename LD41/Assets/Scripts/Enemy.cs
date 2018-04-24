using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
	public GameObject viewZonePrefab;
	public GameObject viewZoneHolder;
	public GameObject alertUI;

	// ═══════════════════════════════════════════════════════════ PROPERTIES ════
	// ═════════════════════════════════════════════════════════════ PRIVATES ════
	Vector2 m_initialPos;
	List<Vector2> m_patrolCoordinates = new List<Vector2> ();
	bool m_firstPatrol = true;
	Vector2 m_currentSpotTarget;
	Vector2 m_currentSpotTargetDir;
	List<GameObject> m_viewZone2 = new List<GameObject> ();
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

			if (viewZonePrefab != null && viewZoneHolder != null)
			{
				// get the direction of the current spot target
				m_currentSpotTargetDir = (m_currentSpotTarget - Coordinate).normalized;
				for (int i = 0; i < viewDistance; i++)
				{
					Vector3 position = Coordinate + (m_currentSpotTargetDir * (i + 1));
					m_viewZone2.Add (Instantiate (viewZonePrefab, position, Quaternion.identity, viewZoneHolder.transform));
					m_viewZone2[i].SetActive (false);
				}
			}
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

		// hide all the view zone marks
		DrawViewZone (false);

		// cast a ray in the direction of "view"
		RaycastHit2D hit = Physics2D.Raycast (Coordinate + m_currentSpotTargetDir,
			m_currentSpotTargetDir, Mathf.Infinity, walkableLayer);

		if (hit != null)
		{
			// calculate the amount of tiles the enemy can see based on the obstacles
			// on its view area
			int visibleZones = Mathf.FloorToInt (Mathf.Min (viewDistance, Mathf.CeilToInt (hit.distance)));

			if (visibleZones < viewDistance)
			{
				// something is interfering in the view of the enemy
				if (hit.transform.tag.Equals ("Player") || hit.transform.tag.Equals ("Trail"))
				{
					// end the game if the thing was the player or its trail
					iTween.Stop (gameObject);
					playerOnSight = true;
					if (alertUI != null)
					{
						alertUI.GetComponent<Text> ().text = hit.transform.tag.Equals ("Player") ?
							"DON'T MOVE! WHO ARE YOU!!!???" :
							"Someone peed on the floor, turn on the alarms.";
						alertUI.SetActive (true);
					}
					m_gameManager.PlayerDetected ();
					return;
				}
			}

			// draw on screen only the tiles that the enemy can see
			for (int i = 1; i <= visibleZones; i++)
			{
				Vector3 position = Coordinate + (m_currentSpotTargetDir * i);
				m_viewZone2[i - 1].transform.position = position;
				m_viewZone2[i - 1].SetActive (true);
			}
		}
	}

	// make the tiles that are being watched to change their color
	void DrawViewZone (bool mark)
	{
		foreach (GameObject zone in m_viewZone2)
		{
			zone.SetActive (mark);
		}
	}

	// method that triggers the execution of the patrolling behaviour. This function
	// will keep calling itself until the current level ends
	public IEnumerator StartPatrolling ()
	{
		if (alertUI != null)
		{
			alertUI.SetActive (false);
		}
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
				m_currentSpotTargetDir = (m_currentSpotTarget - Coordinate).normalized;

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
		if (alertUI != null)
		{
			alertUI.SetActive (false);
		}

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