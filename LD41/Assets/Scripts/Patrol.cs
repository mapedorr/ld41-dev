using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PatrolType { GO, WATCH }

public class Patrol : MonoBehaviour
{
	public PatrolType patrolType;

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