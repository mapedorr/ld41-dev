using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NeedIndicator : MonoBehaviour
{
	// ══════════════════════════════════════════════════════════════ PUBLICS ════
	public Color goodColor;
	public Color badColor;

	// ═══════════════════════════════════════════════════════════ PROPERTIES ════
	// TODO: define properties
	// ═════════════════════════════════════════════════════════════ PRIVATES ════
	Image m_image;

	// ══════════════════════════════════════════════════════════════ METHODS ════
	void Awake ()
	{
		m_image = GetComponent<Image> ();
	}

	// change the appearance of the indicator to show the player if it is in good
	// or bad level
	public void SetGood (bool good)
	{
		if (m_image == null)
		{
			return;
		}

		if (good)
		{
			m_image.color = goodColor;
		}
		else
		{
			m_image.color = badColor;
		}
	}
}