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

	// returns a Vector2 with coordinates rounded to whole numbers from a Vector3
	public static Vector2 Vector2Round (Vector3 inputVector)
	{
		return new Vector2 (Mathf.Round (inputVector.x), Mathf.Round (inputVector.y));
	}

	public static Vector3 Vector3Transform (Vector2 source)
	{
		return new Vector3 (source.x, source.y, 0f);
	}
}