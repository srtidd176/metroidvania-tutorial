using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header ("Horizontal Movement Settings")]
    [SerializeField] private float walkSpeed = 1;
    private float xAxis;
    [Space(5)]

    [Header ("Vertical Movement Settings")]
    [SerializeField] private float jumpForce = 45;
    private int jumpBufferCounter;
    [SerializeField] private int jumpBufferFrames;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;
    [Space(5)]

    [Header ("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundcheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;
    [Space(5)]

    [Header ("Dash Settings")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    private bool canDash = true;
    private bool dashed;
    [SerializeField] GameObject dashEffect;
    [Space(5)]

    [Header ("Attacking Settings")]
    [SerializeField] private float timeBetweenAttack;
    private bool attack = false;
    private float timeSinceAttack;
    [SerializeField] Transform sideAttackTransform, upAttackTransform, downAttackTransform;
    [SerializeField] Vector2 sideAttackArea, upAttackArea, downAttackArea;
    [Space(5)]

    PlayerStateList pState;
    private Rigidbody2D rb;
    Animator anim;
    public static PlayerController Instance;
    private float gravity;


    private void Awake() {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        pState = GetComponent<PlayerStateList>();
        gravity = rb.gravityScale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(sideAttackTransform.position,sideAttackArea);
        Gizmos.DrawWireCube(upAttackTransform.position,upAttackArea);
        Gizmos.DrawWireCube(downAttackTransform.position,downAttackArea);   
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        UpdateJumpVariables();

        if(pState.dashing) return;
        Flip();
        Move();
        Jump();
        StartDash();
        Attack();
    }

    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        attack = Input.GetMouseButtonDown(0);
    }

    void Flip()
    {
        if(xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
    }

    private void Move()
    {
        rb.velocity = new Vector2(walkSpeed *xAxis, rb.velocity.y);
        anim.SetBool("Walking", rb.velocity.x != 0 && Grounded());
    }

    private bool Grounded()
    {
        if(Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround) 
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundcheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundcheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void StartDash()
    {
        if(Input.GetButtonDown("Dash") && canDash && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;
        }
        if (Grounded())
        {
            dashed = false;
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        pState.dashing = true;
        anim.SetBool("Dashing",true);
        rb.gravityScale = 0;
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);
        //If I'm grounded, spawn this dash visual effect at my position.
        if(Grounded()) Instantiate(dashEffect, transform);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        pState.dashing = false;
        anim.SetBool("Dashing",false);
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if (attack && timeSinceAttack >= timeBetweenAttack)
        {
            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");
        }
    
    }

    void Jump()
    {
        if(Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x,0);
            pState.jumping = false;
        }

        if (!pState.jumping)
        {
            if(jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);
                pState.jumping = true;
            }
            else if(!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                pState.jumping = true;
                airJumpCounter++;
                rb.velocity = new Vector3(rb.velocity.x, jumpForce);

            }
        }
        anim.SetBool("Jumping", !Grounded());
    }

    //Used for buffering jump
    void UpdateJumpVariables()
    {
        if (Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter--;
        }
    }
}
