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

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform raycastCenter;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask slopeLayer;
    [SerializeField] private Animator animator;
    private BoxCollider2D cl;
    private HingeJoint2D hj;
    public GameObject checkPoint;
    [SerializeField] private ParticleSystem dust;
    [SerializeField] private PhysicsMaterial2D fullFriction;
    [SerializeField] private PhysicsMaterial2D frictionLess;


    [Header("Movement")]

    [SerializeField] private float movementAcceleration;
    [SerializeField] private float maxMoveSpeed;
    [SerializeField] private float groundLinearDrag;
    [SerializeField] private float horizontalDirection;
    private float verticalDirection;
    private bool crouch = false;
    private float horizontalPrev;
    private float verticalPrev;
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

    [Header("Swing")]

    public bool attached = false;
    [SerializeField] private float pushForce = 10f;
    [SerializeField] public Transform attachedTo;
    [SerializeField] private GameObject disregard;
  

    [Header("Collision")]

    [SerializeField] private float groundRaycastLength = 0.3f;
    [SerializeField] private Vector3 groundRaycastOffset;
    [SerializeField] private bool onGround;
    [SerializeField] private bool onSlopes;
    [SerializeField] private bool isFacingRight = true;
    private readonly Vector2 defaultColliderOffset = new Vector2(0.09457415f, -0.02695893f);
    private readonly Vector2 crouchColliderOffset = new Vector2(0.09457415f, -0.23f);
    private readonly Vector2 defaultColliderSize = new Vector2(0.3603824f, 0.9374076f);
    private readonly Vector2 crouchColliderSize = new Vector2(0.3603824f, 0.9374076f / 2);
    private bool coroutinesStart = false;
    private Vector2 slopeNormal;
    private float slopeAngle;
    
    // Start is called before the first frame update
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cl = GetComponent<BoxCollider2D>();
        hj = gameObject.GetComponent<HingeJoint2D>();
    }
    // Update is called once per frame
    private void Update()
    {
      animator.SetBool("Crouching",crouch);
      animator.SetBool("Jumping", jump);
      animator.SetBool("Attaching", attached);
      animator.SetBool("OnGround", onGround);
      animator.SetBool("Walking", MathF.Abs(horizontalDirection) >= 0.7f);
    }
    private void FixedUpdate()
    {
        if (!jump) jumpBufferCounter -= Time.deltaTime;

        if (attached)
        {
            disregard = attachedTo.gameObject;
        }

        CheckCollision();
        MoveCharacter();
        if (onGround)
        {
            rb.gravityScale = 1;
            ApplyGroundLinearDrag();
            if (!flipCharacter)
            {
                ApplyAirLinearDrag();
            }
        }
        else if (!onGround && !attached)
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

    #region INPUTS

    public void GetJumpInput(InputAction.CallbackContext context)
    {
        if (!attached)
        {
            if (context.performed)
            {
                jumpBufferCounter = jumpBufferTime;
            }
            if (jumpBufferCounter > 0f && coyoteCounter > 0f)
            {
                Jump();
                onGround = false;
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
        else
        {
            if (context.performed)
            {
                VineDetach();
            }
        }
       
    }
    public void GetHorizontalInput(InputAction.CallbackContext context)
    {


        horizontalDirection = context.ReadValue<Vector2>().x;

        verticalDirection = context.ReadValue<Vector2>().y;




        // if (context.performed)
        // {
        //     StopCoroutine(airDragEnumerator());
        // }
        // if (context.canceled)
        // {
        //     StartCoroutine(airDragEnumerator());
        //
        // }



    }

    public void GetCrouchInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Crouch();
        }

    }
    #endregion

    #region AESTHETIC
    void CreateDust()
    {
        dust.Play();
    }
    private void Flip()
    {
        CreateDust();
        isFacingRight = !isFacingRight;
        var localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    #endregion
    
    #region VINE

    private void VineSwing(bool right)
    {
        if (right)
        {
            Debug.Log(("Derecha"));
            rb.AddRelativeForce(new Vector2(1, 0) * pushForce);
           
        }
        else
        {
            Debug.Log(("Izquierda"));
            rb.AddRelativeForce(new Vector2(-1, 0) * pushForce);
          
        }
        rb.gravityScale = 1f;
    }

    private void VineSlide(bool up)
    {
        RopeSegment myConnection = hj.connectedBody.gameObject.GetComponent<RopeSegment>();
        GameObject newSeg = null;
        Debug.Log("Arriba");
        if (up)
        {
            Debug.Log("Arriba");
            if (myConnection.connectedAbove != null)
            {
                if (myConnection.connectedAbove.gameObject.GetComponent<RopeSegment>() != null)
                {
                    newSeg = myConnection.connectedAbove;

                }
            }
        }
        else
        {
            if (myConnection.connectedBelow != null)
            {
                newSeg = myConnection.connectedBelow;
            }
        }

        if (newSeg != null)
        {
            transform.position = newSeg.transform.position;
            myConnection.isPlayerAttached = false;
            newSeg.GetComponent<RopeSegment>().isPlayerAttached = true;
            hj.connectedBody = newSeg.GetComponent<Rigidbody2D>();
        }
        verticalDirection = 0;
    }


    private void VineAttach(Rigidbody2D ropeBone)
    {
        jump = false;
        //cl.enabled = false;
        ropeBone.gameObject.GetComponent<RopeSegment>().isPlayerAttached = true;
        hj.connectedBody = ropeBone;
        hj.enabled = true;
        attached = true;
        attachedTo = ropeBone.gameObject.transform.parent;
        disregard = attachedTo.gameObject;
        pushForce = 40;
        Debug.Log("Volvi");
    }
    private void VineDetach()
    {
        StartCoroutine(airDragEnumerator());
        cl.enabled = true;
        hj.connectedBody.gameObject.GetComponent<RopeSegment>().isPlayerAttached = false;
        attached = false;
        attachedTo = null;
        hj.enabled = false;
        hj.connectedBody = null;
        Debug.Log("Me fui");
        pushForce = 40;
       
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (!attached)
        {
            if (other.gameObject.tag == "Rope")
            {
                if (attachedTo != other.gameObject.transform.parent)
                {
                    if (disregard == null || other.gameObject.transform.parent.gameObject != disregard)
                    {
                        VineAttach(other.gameObject.GetComponent<Rigidbody2D>());
                        VineSlide(false);
                    }
                }
            }
        }
       
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Trap")
        {
            Debug.Log("Trap");
            rb.velocity = new Vector2(0, 0);
            gameObject.transform.position = checkPoint.transform.position;
        }
    }

    #endregion

    #region MOVE


    private void MoveCharacter()
    {
        if (!crouch && !attached &&!onSlopes)
        {
           
            rb.AddForce(new Vector2(horizontalDirection, 0) * movementAcceleration);

            if (MathF.Abs(rb.velocity.x) > maxMoveSpeed)
                rb.velocity = new Vector2(MathF.Sign(rb.velocity.x) * maxMoveSpeed, rb.velocity.y);

           
            rb.drag = 20.0f;
        }
        else if (onSlopes)
        {
            Debug.Log("llegue");
            rb.AddForce(new Vector2(-horizontalDirection*slopeNormal.x * movementAcceleration, slopeNormal.y * -horizontalDirection * movementAcceleration) * 2);

            if (MathF.Abs(rb.velocity.x) > maxMoveSpeed)
                rb.velocity = new Vector2(MathF.Sign(rb.velocity.x) * maxMoveSpeed, rb.velocity.y);
            rb.drag = 20.0f;
        }
        else if (crouch)
        {
            rb.AddForce(new Vector2(horizontalDirection, 0) * movementAcceleration / 2);
            if (MathF.Abs(rb.velocity.x) > maxMoveSpeed / 2)
                rb.velocity = new Vector2(MathF.Sign(rb.velocity.x) * (maxMoveSpeed / 2), rb.velocity.y);
        }
        else if (attached)
        {

            if (horizontalDirection > 0)
            {
                VineSwing(true);
            }
            else if (horizontalDirection < 0)
            {
                VineSwing(false);
            }
            if (verticalDirection > 0)
            {
                Debug.Log("Agarrado");
                VineSlide(true);
            }
            else if (verticalDirection < 0)
            {

                VineSlide(false);
            }
        }
    }

    #endregion

    #region CROUCH
    private void Crouch()
    {
        if (!crouch)
        {
            bool betweenCrawlSpace=Physics2D.Raycast(raycastCenter.transform.position * groundRaycastLength, Vector2.up, groundRaycastLength, groundLayer);
            if (!betweenCrawlSpace)
            {
                crouch = true;
                cl.size = crouchColliderSize;
                cl.offset = crouchColliderOffset;
            }
         
        }
        else
        { 
            cl.offset = defaultColliderOffset;
            cl.size = defaultColliderSize;
            crouch = false;
          
        }
    }
    #endregion

    #region JUMP

    public void Jump()
    {
        if (!crouch)
        {
            CreateDust();
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        }

    }
    public void FallMultiplier()
    {
      

        if (rb.velocity.y < 0 && !coroutinesStart)
        {
            rb.gravityScale = fallMultiplier;
            jump = false;
            animator.SetTrigger("Falling");
        }
        else if (rb.velocity.y > 0 && !jump && !coroutinesStart)
        {
            rb.gravityScale = lowJumpMultiplier;
            jump = false;
            animator.SetTrigger("Falling");
        }
        else if (!coroutinesStart)
        {
            rb.gravityScale = 1f;
        }
        
    }

    #endregion

    #region DRAG

    public void ApplyGroundLinearDrag()
    {

        rb.drag = MathF.Abs(horizontalDirection) < 0.4f || flipCharacter ? groundLinearDrag : 0f;

        // rb.drag = MathF.Abs(horizontalDirection) < 0.4f || flipCharacter ? airLinearDrag : groundLinearDrag;
    }

    public void ApplyAirLinearDrag()
    {
        rb.drag = airLinearDrag;
    }
    private IEnumerator airDragEnumerator()
    {
        coroutinesStart = true;

        float duration = 0.2f; // 3 seconds you can change this 
        //to whatever you want
        float normalizedTime = 0;
        while (normalizedTime <= 1f)
        {
            if (horizontalDirection > 0)
            {
                rb.AddRelativeForce(new Vector2(1, 0) * (pushForce));
            }
            else if (horizontalDirection < 0)
            {
                rb.AddRelativeForce(new Vector2(-1, 0) * (pushForce));
            }

            rb.drag = 0;
            // rb.gravityScale = 1;
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }


        rb.gravityScale = 10;
        coroutinesStart = false;
    }
    #endregion

    #region GROUNDCOLLISIONS

    public void CheckCollision()
    {
        onGround = Physics2D.Raycast(raycastCenter.transform.position * groundRaycastLength, Vector2.down, groundRaycastLength, groundLayer) ||
                   Physics2D.Raycast(raycastCenter.transform.position - groundRaycastOffset, Vector2.down, groundRaycastLength, groundLayer);
        onSlopes = Physics2D.Raycast(raycastCenter.transform.position * groundRaycastLength, Vector2.down, groundRaycastLength, slopeLayer) ||
                     Physics2D.Raycast(raycastCenter.transform.position - groundRaycastOffset, Vector2.down, groundRaycastLength, slopeLayer);

       RaycastHit2D hit = Physics2D.Raycast(raycastCenter.transform.position * groundRaycastLength, Vector2.down,
           groundRaycastLength, slopeLayer);
       if (hit)
       {
           slopeNormal = Vector2.Perpendicular(hit.normal).normalized;
           slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
       }

        if (onGround && onSlopes)
        {
            coyoteCounter = coyoteTime;
            disregard = null;
            rb.sharedMaterial = fullFriction;
            
        }
        else if (!onGround && onSlopes)
        {
            coyoteCounter = coyoteTime;
            disregard = null;
            rb.sharedMaterial = fullFriction;
           
        }
        else if (onGround && !onSlopes)
        {
            coyoteCounter = coyoteTime;
            disregard = null;
            rb.sharedMaterial = frictionLess;
          
        }
        else
            coyoteCounter -= Time.deltaTime;

        if (onSlopes && horizontalDirection != 0)
        {
            rb.sharedMaterial = frictionLess;
        }

        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(raycastCenter.transform.position, raycastCenter.transform.position + Vector3.down * groundRaycastLength);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(raycastCenter.transform.position - groundRaycastOffset, raycastCenter.transform.position - groundRaycastOffset + Vector3.down * groundRaycastLength);
    }

    #endregion

    
}