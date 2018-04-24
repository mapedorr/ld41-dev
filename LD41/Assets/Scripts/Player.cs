using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MovingObject
{
	// ══════════════════════════════════════════════════════════════ PUBLICS ════
	public GameObject hungerIndicatorsHolder;
	public GameObject baddlerIndicatorsHolder;
	// how many seconds should pass each times the needs change
	public float needsDecreaseRate = 60f;
	// decrease rate of the Hunger need
	public float hungerDR;
	// decrease rate of the Baddler need
	public float baddlerDR;
	public bool peeing;
	public GameObject peeMarkPrefab;
	// the following to will be used after the PC satisfies the related need. one
	// can think of them as the hours passed between each one
	//   - indicates how many ticks should pass before hunger starts decreasing again
	public int hungerCooldown = 4;
	//   - indicates how many ticks should pass before baddler starts decreasing again
	public int baddlerCooldown = 2;
	public GameObject cantLeaveUI;
	public GameObject documentsCountUI;

	// ═══════════════════════════════════════════════════════════ PROPERTIES ════
	float m_h;
	public float H { get { return m_h; } }

	float m_v;
	public float V { get { return m_v; } }

	bool m_inputEnabled;
	public bool InputEnabled { get { return m_inputEnabled; } set { m_inputEnabled = value; } }

	private float m_levelStartTime;
	public float LevelStartTime { get { return m_levelStartTime; } set { m_levelStartTime = value; } }

	// ═════════════════════════════════════════════════════════════ PRIVATES ════
	float m_hungerNeedLvl;
	float m_baddlerNeedLvl;
	NeedIndicator[] m_hungerIndicators;
	NeedIndicator[] m_baddlerIndicators;
	bool m_peed = false;
	float m_defaultMovementSpeed;
	List<GameObject> m_peeTrace = new List<GameObject> ();
	int m_noHungerCount;
	int m_noBaddlerCount;
	SpriteRenderer m_sprite;

	// ══════════════════════════════════════════════════════════════ METHODS ════
	protected override void Awake ()
	{
		base.Awake ();

		m_defaultMovementSpeed = movementSpeed;
		m_sprite = GetComponent<SpriteRenderer> ();

		if (hungerIndicatorsHolder)
		{
			m_hungerIndicators = hungerIndicatorsHolder.GetComponentsInChildren<NeedIndicator> ();
		}

		if (baddlerIndicatorsHolder != null)
		{
			m_baddlerIndicators = baddlerIndicatorsHolder.GetComponentsInChildren<NeedIndicator> ();
		}
	}

	protected override void Start ()
	{
		base.Start ();
	}

	// Update is called once per frame
	protected override void Update ()
	{
		base.Update ();

		if (isMoving)
		{
			return;
		}

		if (peeing && peeMarkPrefab)
		{
			bool putMark = true;
			// pee over the tile where the player stands
			// look if there's a pee mark on the current tile
			foreach (GameObject peeMark in m_peeTrace)
			{
				if (peeMark.transform.position == transform.position)
				{
					putMark = false;
				}
			}

			if (putMark)
			{
				m_peeTrace.Add (Instantiate (peeMarkPrefab, transform.position, Quaternion.identity));
				if (m_peeTrace.Count == 3)
				{
					peeing = false;
					m_peeTrace.Clear ();
				}
			}
		}

		if (m_inputEnabled)
		{
			m_h = Input.GetAxisRaw ("Horizontal");
			m_v = Input.GetAxisRaw ("Vertical");
		}
		else
		{
			m_h = 0f;
			m_v = 0f;
		}

		// this will prevent the Player to move diagonally
		if (m_h != 0)
		{
			m_v = 0;
			if (m_sprite)
			{
				m_sprite.flipX = (m_h < 0) ? false : true;
			}
		}

		if (m_h != 0 || m_v != 0)
		{
			if (cantLeaveUI != null && cantLeaveUI.active)
			{
				cantLeaveUI.SetActive (false);

				if (documentsCountUI != null)
				{
					documentsCountUI.SetActive (true);
				}
			}

			if (m_hungerNeedLvl <= 0)
			{
				// the PC might not follow orders if he is hungry
				if (NotObey ())
				{
					return;
				}
				else
				{
					// ...and although she follows orders, her speed will be affected
					movementSpeed = m_defaultMovementSpeed * Random.Range (0.1f, 0.8f);
				}
			}

			// maybe crates or other things might appear later, wee need to check
			// collisions against those objects in orther to trigger specific
			// behaviours
			AttemptMove<Component> (new Vector2 (m_h, m_v));
		}
	}

	// method called when the object attempts to move in the direction of an
	// object that triggers an action
	protected override void OnCantMove<T> (T component)
	{
		if (component.tag.Equals ("Goal"))
		{
			Goal goal = component.GetComponent<Goal> ();

			if (goal.isExit)
			{
				if (m_gameManager.CanEnterExit ())
				{
					// if all the goals were achieved, then allow the player to enter the
					// extractio room
					StartCoroutine (EnterExit (component.transform.position - transform.position));
					if (cantLeaveUI != null)
					{
						cantLeaveUI.SetActive (false);
					}

					if (documentsCountUI != null)
					{
						documentsCountUI.SetActive (false);
					}
				}
				else
				{
					if (cantLeaveUI != null)
					{
						cantLeaveUI.SetActive (true);
					}
				}
			}
			else
			{
				// TODO: trigger any specific behaviour when the player reaches this common goal
				component.gameObject.SetActive (false);
				m_gameManager.GoalAchieved ();

				if (documentsCountUI != null)
				{
					documentsCountUI.GetComponent<Text> ().text = m_gameManager.GetGoalsString ();
					documentsCountUI.SetActive (true);
				}
			}
		}
		else if (component.tag.Equals ("Bath"))
		{
			SatisfyBaddler ();
		}
		else if (component.tag.Equals ("Snacks"))
		{
			SatisfyHunger ();
		}
	}

	IEnumerator EnterExit (Vector2 destination)
	{
		yield return StartCoroutine (ForceMove (destination));
		yield return new WaitForSeconds (1.5f);

		m_gameManager.IsGameOver = true;
	}

	// set the PC needs to its default values (the optimal)
	public void ResetNeeds ()
	{
		// set the needs to its optimal condition
		m_baddlerNeedLvl = m_gameManager.NeedIndicators;
		m_hungerNeedLvl = m_gameManager.NeedIndicators;

		m_noHungerCount = 0;
		m_noBaddlerCount = 0;

		// update the UI so the player can see its agent healthy
		UpdateNeedIndicators ();

		StartCoroutine (DecreaseNeeds ());
	}

	IEnumerator DecreaseNeeds ()
	{
		while (!m_gameManager.IsGameOver)
		{
			yield return new WaitForSeconds (needsDecreaseRate);

			if (m_noHungerCount-- <= 0)
			{
				m_hungerNeedLvl -= hungerDR;
			}

			if (m_noBaddlerCount-- <= 0)
			{
				m_baddlerNeedLvl -= baddlerDR;
			}

			if (m_baddlerNeedLvl <= 0)
			{
				// you can walk as normal when you want to pee
				movementSpeed = 1f;

				if (m_baddlerNeedLvl <= -2f)
				{
					// make the PC pee...her baddle needs are satisfied, but think on
					// the consecuences
					SatisfyBaddler (true);
				}
			}

			UpdateNeedIndicators ();
		}
	}

	void UpdateNeedIndicators ()
	{
		SetNeedIndicatorsTo (m_hungerIndicators, Mathf.CeilToInt (m_hungerNeedLvl));
		SetNeedIndicatorsTo (m_baddlerIndicators, Mathf.CeilToInt (m_baddlerNeedLvl));
	}

	// changes the status of the UI indicators for a need based on the level received
	// as parameter
	void SetNeedIndicatorsTo (NeedIndicator[] indicators, int level)
	{
		for (int i = 0; i < indicators.Length; i++)
		{
			if (i <= level - 1)
			{
				indicators[i].SetGood (true);
			}
			else
			{
				indicators[i].SetGood (false);
			}
		}
	}

	bool NotObey ()
	{
		return Random.value >= 0.2f;
	}

	void SatisfyBaddler (bool badWay = false)
	{
		if (badWay)
		{
			peeing = true;
			m_peed = true;
		}

		movementSpeed = m_defaultMovementSpeed;
		m_baddlerNeedLvl = m_gameManager.NeedIndicators;
		m_noBaddlerCount = baddlerCooldown;

		UpdateNeedIndicators ();
	}

	void SatisfyHunger ()
	{
		movementSpeed = m_defaultMovementSpeed;
		m_hungerNeedLvl = m_gameManager.NeedIndicators;
		m_noHungerCount = hungerCooldown;

		UpdateNeedIndicators ();
	}
}