using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float torqueAmount = 1f;
    [SerializeField] float normalSpeed = 20f;
    [SerializeField] float boostSpeed = 40f;
    [SerializeField] float directMovementForce = 10f;
    [SerializeField] float constantForce = 5f;
    [SerializeField] float jumpForce = 8f;
    Rigidbody2D rb2d;
    SurfaceEffector2D surfaceEffector2D;
    bool canMove = true;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        surfaceEffector2D = FindObjectOfType<SurfaceEffector2D>();
        
        if (surfaceEffector2D == null)
        {
            Debug.LogWarning("No SurfaceEffector2D found in scene. Using direct movement instead.");
        }
        
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = checkObj.transform;
            Debug.LogWarning("Created new groundCheck object");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            RotatePlayer();
            RespondToBoost();
            
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                Jump();
            }
        }
        
        CheckIfGrounded();
    }

    void FixedUpdate()
    {
        if (!canMove) return;
        
        ApplyConstantForce();
        
        if (surfaceEffector2D == null)
        {
            ApplyDirectMovement();
        }
    }

    private void CheckIfGrounded()
    {
        if (groundCheck == null || groundLayer.value == 0) return;
        
        bool circleCheck = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRadius * 1.5f, groundLayer);
        
        isGrounded = circleCheck || hit.collider != null;
    }
    
    private void Jump()
    {
        rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, 0f);
        rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        
        rb2d.AddForce(transform.right * directMovementForce * 0.5f, ForceMode2D.Impulse);
    }
    
    private void ApplyConstantForce()
    {
        rb2d.AddForce(transform.right * constantForce, ForceMode2D.Force);
    }

    public void DisableControls()
    {
        canMove = false;
    }

    void RespondToBoost()
    {
        if (surfaceEffector2D == null) return;
        
        if(Input.GetKey(KeyCode.UpArrow))
        {
            surfaceEffector2D.speed = boostSpeed;
        }
        else 
        {
            surfaceEffector2D.speed = normalSpeed;
        }
    }

    void ApplyDirectMovement()
    {
        float speed = Input.GetKey(KeyCode.UpArrow) ? directMovementForce * 2 : directMovementForce;
        rb2d.AddForce(transform.right * speed);
    }

    void RotatePlayer()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rb2d.AddTorque(torqueAmount);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            rb2d.AddTorque(-torqueAmount);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
