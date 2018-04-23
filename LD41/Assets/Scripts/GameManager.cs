using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	// ══════════════════════════════════════════════════════════════ PUBLICS ════
	public static GameManager instance = null;
	// delay between game stages
	public float stageDelay = 1f;

	// events invoked for StartLevel/PlayLevel/EndLevel coroutines
	public UnityEvent setupEvent;
	public UnityEvent startLevelEvent;
	public UnityEvent playLevelEvent;
	public UnityEvent endLevelEvent;
	public UnityEvent loseLevelEvent;

	public Texture2D pointerTexture;

	// ═══════════════════════════════════════════════════════════ PROPERTIES ════
	// indicates if the player clicked start
	bool m_hasLevelStarted = true;
	public bool HasLevelStarted { get { return m_hasLevelStarted; } set { m_hasLevelStarted = value; } }

	// indicates if the player is playing (moving the PC, infiltrating the thing)
	bool m_isGamePlaying = false;
	public bool IsGamePlaying { get { return m_isGamePlaying; } set { m_isGamePlaying = value; } }

	// indicates if the player finished the level
	bool m_isGameOver = false;
	public bool IsGameOver { get { return m_isGameOver; } set { m_isGameOver = value; } }

	// indicates if the player is not seeing the level anymore
	bool m_hasLevelFinished = false;
	public bool HasLevelFinished { get { return m_hasLevelFinished; } set { m_hasLevelFinished = value; } }

	int m_needIndicators = 3;
	public int NeedIndicators { get { return m_needIndicators; } }

	// ═════════════════════════════════════════════════════════════ PRIVATES ════
	Player m_player;
	List<Enemy> m_enemies = new List<Enemy> ();
	bool m_playerDetected = false;

	// ══════════════════════════════════════════════════════════════ METHODS ════
	void Awake ()
	{
		// apply Singleton pattern
		// if (instance == null) instance = this;
		// else if (instance != this) Destroy (gameObject);
		// DontDestroyOnLoad (gameObject);

		// get components and dependencies
		m_player = Object.FindObjectOfType<Player> ().GetComponent<Player> ();
		m_enemies = new List<Enemy> (Object.FindObjectsOfType<Enemy> ());

		if (pointerTexture != null)
		{
			Cursor.SetCursor (pointerTexture, Vector2.zero, CursorMode.Auto);
		}
	}

	// Use this for initialization
	void Start ()
	{
		m_playerDetected = false;

		if (m_player != null && m_enemies != null && m_enemies.Count > 0)
		{
			StartCoroutine ("RunGameLoop");
		}
	}

	// run the main game loop, separated into different stages/coroutines
	IEnumerator RunGameLoop ()
	{
		yield return StartCoroutine ("StartLevelRoutine");
		yield return StartCoroutine ("PlayLevelRoutine");
		yield return StartCoroutine ("EndLevelRoutine");
	}

	IEnumerator StartLevelRoutine ()
	{
		if (setupEvent != null)
		{
			// show start screen
			setupEvent.Invoke ();
		}

		m_player.InputEnabled = false;
		m_player.ResetNeeds ();

		while (!m_hasLevelStarted)
		{
			// wait until the player clicks Start
			yield return null;
		}

		if (startLevelEvent != null)
		{
			startLevelEvent.Invoke ();
		}
	}

	IEnumerator PlayLevelRoutine ()
	{
		m_isGamePlaying = true;

		// wait some time before the game is playable
		// yield return new WaitForSeconds (stageDelay);

		if (playLevelEvent != null)
		{
			playLevelEvent.Invoke ();
		}

		StartLevel ();

		while (!m_isGameOver)
		{
			// pause one frame
			yield return null;
		}
	}

	void StartLevel ()
	{
		m_player.InputEnabled = true;
		m_player.LevelStartTime = Time.fixedTime;
		foreach (Enemy enemy in m_enemies)
		{
			StartCoroutine (enemy.StartPatrolling ());
		}
	}

	// end stage after gameplay is complete
	IEnumerator EndLevelRoutine ()
	{
		// don't allow the player to move, make the enemies stop patrolling
		m_player.InputEnabled = false;
		foreach (Enemy enemy in m_enemies)
		{
			enemy.StopPatrolling ();
		}

		if (endLevelEvent != null)
		{
			// show game over screen
			endLevelEvent.Invoke ();
		}

		if (!m_playerDetected)
		{
			// TODO: do something if the player finished the mission successfully
		}
		else
		{
			if (loseLevelEvent != null)
			{
				loseLevelEvent.Invoke ();
			}
		}

		while (!m_hasLevelFinished)
		{
			// user presses button to continue
			yield return null;
		}

		// play the level again
		RestartLevel ();
	}

	public void PlayerDetected ()
	{
		m_playerDetected = true;
		m_isGameOver = true;
	}

	void RestartLevel ()
	{
		Scene scene = SceneManager.GetActiveScene ();
		SceneManager.LoadScene (scene.name);
	}

	bool WasPlayerDetected ()
	{
		foreach (Enemy enemy in m_enemies)
		{
			if (enemy.playerOnSight)
			{
				return true;
			}
		}

		return false;
	}
}