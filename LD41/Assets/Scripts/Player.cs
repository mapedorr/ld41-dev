using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MovingObject
{
	// ══════════════════════════════════════════════════════════════ PUBLICS ════
	// ═══════════════════════════════════════════════════════════ PROPERTIES ════
	// ═════════════════════════════════════════════════════════════ PRIVATES ════

	// ══════════════════════════════════════════════════════════════ METHODS ════
	protected override void Awake ()
	{
		base.Awake ();
		movementPoints = 3;
	}

	protected override void Start ()
	{
		base.Start ();
		base.DrawMovementCells ();
	}
}