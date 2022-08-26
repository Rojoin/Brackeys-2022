using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderHit : MonoBehaviour
{
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (rb.velocity.y < 1 && col.transform.tag == "Enemy")
        {
            SpriteRenderer spriteRenderer = col.gameObject.GetComponent<SpriteRenderer>();
            spriteRenderer.color = Color.red;
            Destroy(gameObject);
        }
    }
}
