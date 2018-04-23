using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
	// ══════════════════════════════════════════════════════════════ PUBLICS ════
	public bool isExit;
	// ═══════════════════════════════════════════════════════════ PROPERTIES ════
	// TODO: define properties
	// ═════════════════════════════════════════════════════════════ PRIVATES ════
	// TODO: define prrivates

	// ══════════════════════════════════════════════════════════════ METHODS ════
	void OnDrawGizmos ()
	{
		if (isExit)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere (transform.position, 0.2f);
		}
	}
}