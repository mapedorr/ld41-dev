using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{
	// returns a Vector2 with coordinates rounded to whole numbers
	public static Vector2 Vector2Round (Vector2 inputVector)
	{
		return new Vector2 (Mathf.Round (inputVector.x), Mathf.Round (inputVector.y));
	}
}