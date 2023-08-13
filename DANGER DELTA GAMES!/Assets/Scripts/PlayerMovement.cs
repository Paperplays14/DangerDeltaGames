using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour
{
    [Header("Refrences")]
    public Transform orientation;
    public Camera fpsCam;

    [Header("Inputs")]
    public KeyCode Jumpkey = KeyCode.Space;
    public KeyCode SprintKey = KeyCode.V;
    public KeyCode CrouchKey = KeyCode.C;

    [Header("Layers")]
    public LayerMask whatIsGround;
    public LayerMask whatIsStair;

    [Header("Movement Options")]
    public float WalkSpeed;
    public float SprintSpeed;
    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;

    [Header("Crouching")]
    public float crouchYScale;
    public float crouchSpeed;

    [Header("Slope Handling")]
    public float maxSlopeAngle;

    [Header("Other Options")]
    public float playerHeight;
    public States CurrentState;

    [Header("Booleans")]
    public bool grounded;
    public bool canJump;

    [Header("Debuging Variables")]
    public float moveSpeed;
    public float startYScale;
    [Space]
    public float horizontalInput;
    public float verticalInput;
    [Space]
    public bool exitingSlope;
    public bool dead;
    public Rigidbody rb;
    public RaycastHit slopeHit; 

    [Header("Private Variables")]
    private Vector3 movedirection;
    private bool isSprinting;
    private bool sentDeathRpc;
    private float MovmentFloat;
    public enum States{
        crouching,
        walking,
        sprinting,
        air
    }
    void Start()
    {
        dead = false;
        rb = GetComponent<Rigidbody>();
        canJump = true;

        startYScale = transform.localScale.y;
    }

    private void FixedUpdate()
    {
        if (!dead)
        {
            Movement();
        }
    }
    void Update()
    {
        GetInputs();
        ApplyDrag();
        SpeedControll();
        SyncPlayerAnimations();
        StateMachine();

        isSprinting = (CurrentState == States.sprinting);
    }
    void GetInputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        var horizontalInputs = Input.GetAxis("Horizontal");
        var verticalInputs = Input.GetAxis("Vertical");

        #region Counter Inputs

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
            horizontalInputs = 0;
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
            verticalInputs = 0;
        #endregion


        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        //jumping
        if(Input.GetKey(Jumpkey) && grounded && canJump && !dead)
        {
            Jump();
        }

        if (Input.GetKeyDown(CrouchKey))
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

        if (Input.GetKeyUp(CrouchKey))
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

}
// not needed
    void SyncPlayerAnimations()
    {
        var horizontalInputs = Input.GetAxis("Horizontal");
        var verticalInputs = Input.GetAxis("Vertical");

        #region Counter Inputs

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
            horizontalInputs = 0;
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
            verticalInputs = 0;
        #endregion

    }

    public void StateMachine()
    {
        if (!Input.GetKey(SprintKey) && !Input.GetKey(CrouchKey) && grounded)
        {
            CurrentState = States.walking;
            moveSpeed = WalkSpeed;

            fpsCam.DOFieldOfView(60f, 0.2f);
        }
        else if(Input.GetKey(SprintKey) && grounded && verticalInput > 0)
        {
            CurrentState = States.sprinting;
            fpsCam.DOFieldOfView(70f, 0.2f);
            moveSpeed = SprintSpeed;

        }
        else if(Input.GetKey(CrouchKey) && grounded)
        {
            CurrentState = States.crouching;
            fpsCam.DOFieldOfView(55f, 0.2f);
            moveSpeed = crouchSpeed;
            rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
        }
        else
        {
            CurrentState = States.air;
        }

        //animations
        if (horizontalInput != 0 || verticalInput != 0)
        {
            MovmentFloat = Mathf.MoveTowards(MovmentFloat, 1, Time.deltaTime * 3);
        }
        else
        {
            MovmentFloat = Mathf.MoveTowards(MovmentFloat, 0, Time.deltaTime * 3);
        }
    }
    void ApplyDrag()
    {
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    void Movement()
    {

        #region Applying Forces
        movedirection = orientation.right * horizontalInput + orientation.forward * verticalInput;
        rb.AddForce(movedirection * 10f * moveSpeed ,ForceMode.Force);
        //gravity is low so i apply force
        if(!OnSlope()) rb.AddForce(Vector3.down * 10f, ForceMode.Force);
        #endregion

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * 20f * moveSpeed, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 120f, ForceMode.Force);
        } //Slope Movement
        rb.useGravity = !OnSlope();
    }
    
    void SpeedControll()
    {
        if(OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }

        }
    }

    void Jump()
    {
        canJump = false;
        exitingSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        Invoke("ResetJump", jumpCooldown);
    }

    void ResetJump()
    {
        canJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down , out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(movedirection, slopeHit.normal).normalized;
    }
}
