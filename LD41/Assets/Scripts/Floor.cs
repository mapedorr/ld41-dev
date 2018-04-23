using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Floor : MonoBehaviour
{
  // ══════════════════════════════════════════════════════════════ PUBLICS ════
  public LayerMask playerLayer;
  // ═══════════════════════════════════════════════════════════ PROPERTIES ════
  // ═════════════════════════════════════════════════════════════ PRIVATES ════
  BoxCollider2D m_boxCollider;
  SpriteRenderer m_spriteRenderer;
  Color m_defaultColor;

  // ══════════════════════════════════════════════════════════════ METHODS ════
  void Awake ()
  {
    m_spriteRenderer = GetComponent<SpriteRenderer> ();
    m_boxCollider = GetComponent<BoxCollider2D> ();
  }

  void Start ()
  {
    if (m_spriteRenderer != null)
    {
      m_defaultColor = m_spriteRenderer.color;
    }
  }

  // change the color of the floor to something that indicates the player the zone
  // that it's being watched by the enemy
  public void SetColor (Color newColor)
  {
    if (m_spriteRenderer != null)
    {
      m_spriteRenderer.color = newColor;
    }
  }

  // set the color of this piece of floor to its default
  public void ResetColor ()
  {
    if (m_spriteRenderer != null)
    {
      m_spriteRenderer.color = m_defaultColor;
    }
  }

  // check if the player is overlaping this piece of floor so the enemy watching
  // it can trigger the game over condition
  public bool CheckPlayerOverlap ()
  {
    if (m_boxCollider == null)
    {
      return false;
    }

    Collider2D[] result = new Collider2D[10];
    ContactFilter2D x = new ContactFilter2D ();
    x.layerMask = playerLayer;
    int hits = m_boxCollider.OverlapCollider (x, result);

    if (hits > 0)
    {
      for (int i = 0; i < result.Length; i++)
      {
        if (result[i] != null)
        {
          if (result[i].GetComponent<Player> () != null)
          {
            // the player is in the zone of view
            return true;
          }
          else if (result[i].tag.Equals ("Trail"))
          {
            // if the enemy see a trail of the player...the mission is fucked!
            // i.e. pee
            return true;
          }
        }
      }
    }

    return false;
  }
}