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
    // [SerializeField] private Animator animator;
    private BoxCollider2D cl;
    private HingeJoint2D hj;


    [Header("Movement")]

    [SerializeField] private float movementAcceleration;
    [SerializeField] private float maxMoveSpeed;
    [SerializeField] private float groundLinearDrag;
    private float horizontalDirection;
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
    [SerializeField] private float pushForse = 10f;
    public Transform attachedTo;
    [SerializeField] private GameObject disregard;


    [Header("Collision")]

    [SerializeField] private float groundRaycastLength = 0.3f;
    [SerializeField] private bool onGround;
    [SerializeField] private bool isFacingRight = true;
    private readonly Vector2 defaultColliderOffset = new Vector2(0.09457415f, -02695893f);
    private readonly Vector2 defaultColliderSize = new Vector2(0.3603824f, 0.9374076f);
    private readonly Vector2 crouchColliderSize = new Vector2(0.3603824f, 0.9374076f / 2);

    private bool corroutineStart = false;
    // Start is called before the first frame update
    private void Awake()
    {
        cl = GetComponent<BoxCollider2D>();
        hj = gameObject.GetComponent<HingeJoint2D>();
    }
    // Update is called once per frame
    private void Update()
    {

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

            ApplyGroundLinearDrag();

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

    IEnumerator airDragEnumerator()
    {
        corroutineStart = true;
        yield return new WaitForSeconds(1);
        ApplyAirLinearDrag();
        FallMultiplier();
        corroutineStart = false;
    }
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
       // verticalDirection = context.ReadValue<Vector2>().y;

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


    private void Flip()
    {
        isFacingRight = !isFacingRight;
        var localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    #region VINE

    private void VineSwing(bool right)
    {
        if (right)
        {
            Debug.Log(("Derecha"));
            rb.AddRelativeForce(new Vector3(4, 0, 0) * pushForse);
        }
        else
        {
            Debug.Log(("Izquierda"));
            rb.AddRelativeForce(new Vector3(-4, 0, 0) * pushForse);
        }

    }

    private void VineSlide(bool up)
    {
        RopeSegment myConnection = hj.connectedBody.gameObject.GetComponent<RopeSegment>();
        GameObject newSeg = null;
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

        //cl.enabled = false;
        ropeBone.gameObject.GetComponent<RopeSegment>().isPlayerAttached = true;
        hj.connectedBody = ropeBone;
        hj.enabled = true;
        attached = true;
        attachedTo = ropeBone.gameObject.transform.parent;
        disregard = attachedTo.gameObject;
        Debug.Log("Volvi");
    }
    private void VineDetach()
    {
        cl.enabled = true;
        hj.connectedBody.gameObject.GetComponent<RopeSegment>().isPlayerAttached = false;
        attached = false;
        attachedTo = null;
        hj.enabled = false;
        hj.connectedBody = null;
        Debug.Log("Me fui");
    }

    void OnTriggerEnter2D(Collider2D other)
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
                    }
                }
            }
        }
    }


    #endregion

    #region MOVE


    private void MoveCharacter()
    {
        if (!crouch && !attached)
        {

            rb.AddForce(new Vector2(horizontalDirection, 0) * movementAcceleration);

            if (MathF.Abs(rb.velocity.x) > maxMoveSpeed)
                rb.velocity = new Vector2(MathF.Sign(rb.velocity.x) * maxMoveSpeed, rb.velocity.y);

        }
        else if (crouch)
        {
            rb.AddForce(new Vector2(horizontalDirection, 0) * movementAcceleration / 2);
            if (MathF.Abs(rb.velocity.x) > maxMoveSpeed / 2)
                rb.velocity = new Vector2(MathF.Sign(rb.velocity.x) * (maxMoveSpeed / 2), rb.velocity.y);
        }
        else if (attached)
        {
            Debug.Log("Agarrado");
            if (horizontalDirection > 0)
            {
                VineSwing(true);
            }
            else if (horizontalDirection < 0)
            {
                VineSwing(false);
            }
            else if (verticalDirection > 0)
            {
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
            crouch = true;
            cl.size = crouchColliderSize;
        }
        else
        {
            crouch = false;
            cl.size = defaultColliderSize;
        }
    }
    #endregion

    #region JUMP

    public void Jump()
    {
        if (!crouch)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        }

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
    #endregion

    #region COLLISIONS

    public void CheckCollision()
    {
        onGround = Physics2D.Raycast(raycastCenter.transform.position * groundRaycastLength, Vector2.down,
            groundRaycastLength, groundLayer);
        if (onGround)
        {
            coyoteCounter = coyoteTime;
            disregard = null;
        }

        else
            coyoteCounter -= Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(raycastCenter.transform.position,
            raycastCenter.transform.position + Vector3.down * groundRaycastLength);
    }

    #endregion


}