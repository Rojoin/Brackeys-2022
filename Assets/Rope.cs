using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public Rigidbody2D hook;

    public GameObject[] prefabsRopeSegs;

   // public PlayerController player;
    public int numLinks = 5;
    
    void Start()
    {
        GenerateRope();
    }

    private void GenerateRope()
    {
        Rigidbody2D prevBod = hook;
        for (int vine = 0; vine < numLinks; vine++)
        {
            int index = 0;
            if (vine == numLinks-1)
            {
                index = prefabsRopeSegs.Length-1;
            }
            GameObject newSeg = Instantiate(prefabsRopeSegs[index]);

            newSeg.transform.parent = transform;
            newSeg.transform.position = transform.position;
            HingeJoint2D hj = newSeg.GetComponent<HingeJoint2D>();
            hj.connectedBody = prevBod;
            prevBod = newSeg.GetComponent<Rigidbody2D>();

        }
    }
    
}
