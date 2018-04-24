using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{
	// ══════════════════════════════════════════════════════════════ PUBLICS ════
	// the layer in which the collisions will be checked
	public LayerMask blockingLayer;
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
	// ═══════════════════════════════════════════════════════════ PROPERTIES ════
	protected Vector2 m_coordinate;
	public Vector2 Coordinate { get { return Utility.Vector2Round (transform.position); } }
	// ═════════════════════════════════════════════════════════════ PRIVATES ════
	protected BoxCollider2D m_boxCollider;
	protected GameManager m_gameManager;

	// ══════════════════════════════════════════════════════════════ METHODS ════
	// called when the script instance is being loaded
	protected virtual void Awake ()
	{
		m_boxCollider = GetComponent<BoxCollider2D> ();
		m_gameManager = Object.FindObjectOfType<GameManager> ().GetComponent<GameManager> ();
	}

	// called on the frame when a script is enabled just before any of the Update
	// methods is called the first time
	protected virtual void Start ()
	{
		// TODO: initialize values
	}

	protected virtual void Update ()
	{
		// TODO: execute per frame common behavior
	}

	protected virtual bool AttemptMove<T> (Vector2 direction, float delayTime = 0f) where T : Component
	{
		RaycastHit2D hit;
		bool canMove = Move (direction, out hit, delayTime);

		if (hit.transform == null)
		{
			return true;
		}

		T hitComponent = hit.transform.GetComponent<T> ();

		if (!canMove && hitComponent != null)
		{
			OnCantMove (hitComponent);
		}

		return canMove;
	}

	// move the game object in a direction
	protected bool Move (Vector2 direction, out RaycastHit2D hit, float delayTime)
	{
		Vector2 start = transform.position;
		Vector2 end = start + direction;

		// assure that the casted rays will not hit the collider of the object that is going to be moved
		m_boxCollider.enabled = false;

		// cast a line from the start point to the end point checking collisions in the blockingLayer
		hit = Physics2D.Linecast (start, end, blockingLayer);

		m_boxCollider.enabled = true;

		if (hit.transform == null)
		{
			// init the tween that will move the GameObject
			StartCoroutine (MoveRoutine (end, delayTime));
			return true;
		}

		// the GameObject can't move
		return false;
	}

	// move the game object in a direction regardless if there is an obstacle in
	// its path (added because some levels might require the player to push or
	// pull objects used to push other objects)
	protected IEnumerator ForceMove (Vector2 direction, float delayTime = 0.25f)
	{
		Vector2 start = transform.position;
		Vector2 end = start + direction;

		yield return StartCoroutine (MoveRoutine (end, delayTime));
	}

	// coroutine used to move the player
	protected IEnumerator MoveRoutine (Vector2 destinationPos, float delayTime)
	{
		if (!m_gameManager.IsGameOver)
		{
			// set moving to true in order to prevent some behaviours
			isMoving = true;

			// set the destination to the destinationPos being passed into the coroutine
			destination = destinationPos;

			// pause the coroutine before the movement starts
			yield return new WaitForSeconds (delayTime);

			// move the player toward its destination
			iTween.MoveTo (gameObject, iTween.Hash (
				"x", destinationPos.x,
				"y", destinationPos.y,
				"delay", iTweenDelay,
				"easetype", easeType,
				"speed", movementSpeed
			));

			// check if the distance to the destination is small enough to finish the
			// animation
			while (Vector2.Distance (destinationPos, transform.position) > float.Epsilon)
			{
				if (m_gameManager.IsGameOver)
				{
					transform.position = destinationPos;
					break;
				}

				yield return null;
			}

			// stop the animation
			iTween.Stop (gameObject);

			// set the player position to the destination
			transform.position = destinationPos;

			// set moving to true in order to allow some behaviours
			isMoving = false;
		}
	}

	// method that can be used by classes inheriting from this one in order to
	// react to specific collisions
	protected abstract void OnCantMove<T> (T component) where T : Component;
}