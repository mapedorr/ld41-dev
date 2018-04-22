using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementCell : MonoBehaviour
{
	// ══════════════════════════════════════════════════════════════ PUBLICS ════
	public GameObject pathMarkPrefab;

	// ═══════════════════════════════════════════════════════════ PROPERTIES ════
	// property that returns the coordinate of the movement indicator
	Vector2 m_coordinate;
	public Vector2 Coordinate { get { return Utility.Vector2Round (m_coordinate); } }

	// list of MovementIndicator neighbors to this object
	private List<MovementCell> m_neighbors = new List<MovementCell> ();
	public List<MovementCell> Neighbors { get { return m_neighbors; } }
	// ═════════════════════════════════════════════════════════════ PRIVATES ════
	// TODO: define privates

	// ══════════════════════════════════════════════════════════════ METHODS ════
	void Awake ()
	{
		m_coordinate = new Vector2 (transform.position.x, transform.position.y);
	}

	void Start ()
	{
		// TODO: initialize something
	}
}