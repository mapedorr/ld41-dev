using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
	// ══════════════════════════════════════════════════════════════ PUBLICS ════
	// determines if the object is moving
	public bool isMoving = false;
	// the current cell destination
	public Vector2 destination;
	// the iTween's ease type to use
	public iTween.EaseType easeType = iTween.EaseType.easeInSine;
	// movement speed
	public float movementSpeed = 3f;
	// delay to use before any call to iTween
	public float iTweenDelay = 0f;
	// max distance in which the object can move
	public int movementPoints = 0;
	// max action points the object can use
	public int actionPoints = 0;
	// prefab to draw movement range possibilities
	public MovementCell MovementCellPrefab;
	// ═══════════════════════════════════════════════════════════ PROPERTIES ════
	List<MovementCell> m_linkedMovementCells = new List<MovementCell> ();
	public List<MovementCell> LinkedMovementCells { get { return m_linkedMovementCells; } }
	// ═════════════════════════════════════════════════════════════ PRIVATES ════
	// list that stores the movement path positions
	List<MovementCell> m_movementPath = new List<MovementCell> ();
	// the current cell of the moving object
	MovementCell m_currentCell;

	// ══════════════════════════════════════════════════════════════ METHODS ════
	// called when the script instance is being loaded
	protected virtual void Awake ()
	{
		movementPoints = 1;
		actionPoints = 1;
	}

	// called on the frame when a script is enabled just before any of the Update
	// methods is called the first time
	protected virtual void Start ()
	{
		// TODO: do something
	}

	// move the game object in a direction
	protected IEnumerator Move ()
	{
		if (isMoving)
		{
			yield return null;
		}

		foreach (MovementCell targetCell in m_movementPath)
		{
			yield return StartCoroutine (MoveRoutine (targetCell.Coordinate));
		}

		CleanPath ();
	}

	// the coroutine that make the object moves using an iTween animation
	protected virtual IEnumerator MoveRoutine (Vector2 destinationPos, float delayTime = 0f)
	{
		isMoving = true;
		destination = destinationPos;

		yield return new WaitForSeconds (delayTime);

		iTween.MoveTo (gameObject, iTween.Hash (
			"x", destinationPos.x,
			"y", destinationPos.y,
			"delay", iTweenDelay,
			"easetype", easeType,
			"speed", movementSpeed
		));

		while (Vector2.Distance (destinationPos, transform.position) > 0.01f)
		{
			yield return null;
		}

		iTween.Stop (gameObject);
		transform.position = destinationPos;
		isMoving = false;
	}

	public void DrawMovementCells ()
	{
		if (MovementCellPrefab != null)
		{
			for (int i = movementPoints; i >= -movementPoints; i--)
			{
				for (int j = movementPoints; j >= -movementPoints; j--)
				{
					int sum = Mathf.Abs (i) + Mathf.Abs (j);
					if (sum != 0 && sum <= movementPoints)
					{
						GameObject movementCellInstance = Instantiate (MovementCellPrefab.transform.gameObject,
							transform.position + new Vector3 (j, i, 0f), Quaternion.identity, transform);
						movementCellInstance.name = "" + j + "-" + i;
						MovementCell movementCell = movementCellInstance.GetComponent<MovementCell> ();
						m_linkedMovementCells.Add (movementCell);
						movementCell.SetSeed (this);
					}
				}
			}
		}
	}

	// method that looks for the shortest path to the target cell (that's the path
	// that will be used to move this object)
	public void FindPathTo (MovementCell targetCell)
	{
		if (isMoving)
		{
			return;
		}

		m_movementPath = new List<MovementCell> ();
		Vector2 targetDirection = targetCell.Coordinate - Utility.Vector2Round (transform.position);
		int stepsToTarget = Mathf.CeilToInt (targetDirection.magnitude);
		int doneSteps = 0;
		if (stepsToTarget <= movementPoints)
		{
			MovementCell pathStep = targetCell;

			while (doneSteps < stepsToTarget)
			{
				doneSteps++;
				m_movementPath.Add (pathStep);

				Vector2 direction;
				if (Mathf.Abs (targetDirection.x) >= Mathf.Abs (targetDirection.y))
				{
					// start the path in the X axis
					direction = new Vector2 (Mathf.Round (-targetDirection.normalized.x), 0f);
				}
				else
				{
					// start the path in the Y axis
					direction = new Vector2 (0f, Mathf.Round (-targetDirection.normalized.y));
				}

				pathStep = pathStep.FindNeighborAt (direction);

				if (pathStep)
				{
					targetDirection = pathStep.Coordinate - Utility.Vector2Round (transform.position);
				}
			}
		}

		m_movementPath.Reverse ();
		foreach (MovementCell step in m_movementPath)
		{
			step.TogglePathMark (true);
		}
	}

	public void CleanPath ()
	{
		if (isMoving)
		{
			return;
		}

		foreach (MovementCell step in m_movementPath)
		{
			step.TogglePathMark (false);
		}
		m_movementPath.Clear ();
	}

	public void MoveInPath ()
	{
		if (isMoving)
		{
			return;
		}

		StartCoroutine (Move ());
	}
}