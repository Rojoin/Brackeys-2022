using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RopeSegment : MonoBehaviour
{
    public GameObject connectedAbove, connectedBelow;

    public bool isPlayerAttached = false;

  private void Start()
    {
        connectedAbove = GetComponent<HingeJoint2D>().connectedBody.gameObject;
        RopeSegment aboveSegment = connectedAbove.GetComponent<RopeSegment>();

        if (aboveSegment != null)
        {
            aboveSegment.connectedBelow = gameObject;

            float spriteBottom = connectedAbove.GetComponent<SpriteRenderer>().bounds.size.y;
            Debug.Log(spriteBottom);
            GetComponent<HingeJoint2D>().connectedAnchor = new Vector2(0, spriteBottom * -1);
            Debug.Log(spriteBottom);
        }
        else
        {
            GetComponent<HingeJoint2D>().connectedAnchor = new Vector2(0, 0);
        }

    }

  public void ResetAnchor()
  {
      connectedAbove = GetComponent<HingeJoint2D>().connectedBody.gameObject;
      RopeSegment aboveSegment = connectedAbove.GetComponent<RopeSegment>();

      if (aboveSegment != null)
      {
          aboveSegment.connectedBelow = gameObject;

          float spriteBottom = connectedAbove.GetComponent<SpriteRenderer>().bounds.size.y;
          GetComponent<HingeJoint2D>().connectedAnchor = new Vector2(0, spriteBottom * -1);
      }
      else
      {
          GetComponent<HingeJoint2D>().connectedAnchor = new Vector2(0, 0);
      }
}
}
