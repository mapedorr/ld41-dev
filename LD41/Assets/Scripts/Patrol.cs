using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PatrolType { GO, WATCH }

public class Patrol : MonoBehaviour
{
	// ══════════════════════════════════════════════════════════════ PUBLICS ════
	// defines if the spots on the patrol will mark a place for the enemy to walk
	// to or to watch to.
	public PatrolType patrolType;
	// ═══════════════════════════════════════════════════════════ PROPERTIES ════
	// TODO: put some properties here
	// ═════════════════════════════════════════════════════════════ PRIVATES ════
	// TODO: put some privates here

	// ══════════════════════════════════════════════════════════════ METHODS ════
	// to help recognize the type of the patrol, draw in the Editor spheres (in the positions
	// of the spots) with different colors for each behaviour
	void OnDrawGizmos ()
	{
		switch (patrolType)
		{
			case PatrolType.GO:
				Gizmos.color = Color.red;
				break;
			case PatrolType.WATCH:
				Gizmos.color = Color.yellow;
				break;
		}

		for (int i = 0; i < transform.childCount; i++)
		{
			Gizmos.DrawSphere (transform.GetChild (i).position, 0.1f);
		}
	}
}