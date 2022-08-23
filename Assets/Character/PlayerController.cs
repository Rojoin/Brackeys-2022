using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField] private Transform raycastCenter;

    [SerializeField] private LayerMask groundLayer;

    [Header("Movement")]

    [SerializeField] private float movementAcceleration;
    [SerializeField] private float maxMoveSpeed;
    [SerializeField] private float groundLinearDrag;
    private float horizontalDirection;

    private bool flipCharacter => (rb.velocity.x > 0f && horizontalDirection < 0f) ||
                                  (rb.velocity.x < 0f && horizontalDirection > 0f);

    [Header("Jump")]

    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float airLinearDrag = 10f;
    [SerializeField] private float fallMultiplier = 8f;
    [SerializeField] private float lowJumpMultiplier = 5f;
    private float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;
    private float coyoteTime = 0.2f;
    private float coyoteCounter;
    private bool jump = false;
    [Header("Collision")] [SerializeField] private float groundRaycastLength = 0.3f;

    [SerializeField] private bool onGround;
    [SerializeField] private bool isFacingRight = true;

    // Start is called before the first frame update

    // Update is called once per frame
    private void Update()
    {
       
    }

    private void FixedUpdate()
    {
        if (!jump) jumpBufferCounter -= Time.deltaTime;


        CheckCollision();
        MoveCharacter();
        if (onGround)
        {
            ApplyGroundLinearDrag();
        }
        else
        {
            ApplyAirLinearDrag();
            FallMultiplier();
          
        }

        if (isFacingRight && horizontalDirection < 0f)
        {
            Flip();
        }
        else if (!isFacingRight && horizontalDirection > 0f)
        {
            Flip();
        }
    }


    private void Flip()
    {
        isFacingRight = !isFacingRight;
        var localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    public void GetHorizontalInput(InputAction.CallbackContext context)
    {
        horizontalDirection = context.ReadValue<Vector2>().x;
    }

    private void MoveCharacter()
    {
        rb.AddForce(new Vector2(horizontalDirection, 0) * movementAcceleration);

        if (MathF.Abs(rb.velocity.x) > maxMoveSpeed)
            rb.velocity = new Vector2(MathF.Sign(rb.velocity.x) * maxMoveSpeed, rb.velocity.y);

    }

    public void GetJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            Jump();
            jumpBufferCounter = 0f;
            jump = true;
        }

        if (context.canceled && rb.velocity.y > 0f)
        {
            //rb.velocity +=  Vector2.up*Physics2D.gravity.y*(lowJumpMultiplier-1)*Time.deltaTime;
           rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);


            coyoteCounter = 0f;
            jump = false;
        }
    }

    public void Jump()
    {
    
        rb.velocity = new Vector2(rb.velocity.x, 0);
        
       
      rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public void ApplyGroundLinearDrag()
    {
        rb.drag = MathF.Abs(horizontalDirection) < 0.4f || flipCharacter ? groundLinearDrag : 0f;
    }

    public void ApplyAirLinearDrag()
    {
        rb.drag = airLinearDrag;
    }

    public void CheckCollision()
    {
        onGround = Physics2D.Raycast(raycastCenter.transform.position * groundRaycastLength, Vector2.down,
            groundRaycastLength, groundLayer);
        if (onGround)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(raycastCenter.transform.position,
            raycastCenter.transform.position + Vector3.down * groundRaycastLength);
    }

    public void FallMultiplier()
    {
       
        if (rb.velocity.y < 0)
        {
            Debug.Log("Se cambio la gravedad1");
            rb.gravityScale = fallMultiplier;
        }
        else if (rb.velocity.y > 0 && !jump)
        {
            Debug.Log("Se cambio la gravedad2");
            rb.gravityScale = lowJumpMultiplier;
        }
        else
           rb.gravityScale = 1f;
    }
}