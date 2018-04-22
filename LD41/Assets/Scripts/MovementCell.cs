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

	// weight of the node (used while looking for shortest path from seed to target cell)
	private float m_weight;
	public float Weight { get { return m_weight; } set { m_weight = value; } }

	// ═════════════════════════════════════════════════════════════ PRIVATES ════
	GameObject m_pathMark;
	MovingObject m_seed;

	// ══════════════════════════════════════════════════════════════ METHODS ════
	void Awake ()
	{
		m_coordinate = new Vector2 (transform.position.x, transform.position.y);

		if (pathMarkPrefab != null)
		{
			m_pathMark = Instantiate (pathMarkPrefab, transform.position, Quaternion.identity);
			m_pathMark.SetActive (false);
		}
	}

	void Start ()
	{
		if (m_seed)
		{
			m_neighbors = FindNeighbors (m_seed.LinkedMovementCells);
		}
	}

	void OnMouseEnter ()
	{
		m_seed.FindPathTo (this);
	}

	void OnMouseExit ()
	{
		m_seed.CleanPath ();
	}

	void OnMouseDown ()
	{
		m_seed.MoveInPath ();
	}

	public void TogglePathMark (bool active)
	{
		if (m_pathMark)
		{
			m_pathMark.SetActive (active);
		}
	}

	public void SetSeed (MovingObject seed)
	{
		m_seed = seed;
	}

	public List<MovementCell> FindNeighbors (List<MovementCell> cells)
	{
		List<MovementCell> cellsList = new List<MovementCell> ();
		foreach (Vector2 dir in Level.movementDirections)
		{
			MovementCell foundNeighbor = cells.Find (cell => cell.Coordinate == Coordinate + dir);

			if (foundNeighbor != null && !cellsList.Contains (foundNeighbor))
			{
				cellsList.Add (foundNeighbor);
			}
		}
		return cellsList;
	}

	public MovementCell FindNeighborAt (Vector2 dir)
	{
		return m_neighbors.Find (cell => cell.Coordinate == Coordinate + dir);
	}
}