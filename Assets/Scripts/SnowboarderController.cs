using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SnowboarderController : MonoBehaviour
{
    #region Inspector Fields

    public Rigidbody2D rb;

    [Header("Control Settings")]
    public bool useMobileControls = true;

    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float brakeForce = 20f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float slowDownMultiplier = 0.5f;
    [SerializeField] private float minForwardSpeed = 2f;
    [SerializeField] private float constantForwardForce = 5f; // Always apply this force

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float airFallMultiplier = 1.5f; // Makes falling faster

    [Header("Touch Settings")]
    [SerializeField] private float doubleTapThreshold = 0.3f;
    [SerializeField] private float swipeThreshold = 50f;

    [Header("Air Control")]
    public float rotationSpeed = 200f;

    // Trick Detection (Will be used by ScoreManager)
    public bool isInAir;
    public float rotationStart;

    #endregion

    #region Private Variables


    private float inputDirection;
    private bool isSlowingDown;
    private bool jumpRequested;
    private float lastTapTime;
    private Vector2 startTouchPos;
    private float airRotationInput;
    private bool wasGrounded;

    public bool isGrounded;
    public bool isBraking;
    public float currentSpeed;
    public Vector2 localForward;

    private bool controlsDisabled = false;
    public float raw;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = checkObj.transform;
            Debug.LogWarning("GroundCheck was missing! Created a new one.");
        }
    }

    private void Start()
    {
#if UNITY_EDITOR
        useMobileControls = false;
#else
        useMobileControls = true;
#endif
    }
    private void Update()
    {
        if (controlsDisabled) return;

        CheckIfGrounded();

        if (useMobileControls)
            ReadTouchInput();
        else
            ReadKeyboardInput();

        HandleBoostTimer();
        HandleShieldTimer();
    }

    private void FixedUpdate()
    {
        wasGrounded = isGrounded;

        if (!isGrounded)
            ApplyAirRotation();
        else
            ApplyMovement();
            
        // Always apply some forward momentum regardless of ground state
        ApplyConstantForwardForce();
        
        // Apply faster falling
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (airFallMultiplier - 1) * Time.fixedDeltaTime;
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

#endregion

    #region Input Handling

    private void ReadTouchInput()
    {
        inputDirection = 0f;
        isSlowingDown = false;

        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                startTouchPos = touch.position;

                if (isGrounded)
                {
                    DetectDoubleTap();
                    inputDirection = (touch.position.x > Screen.width / 2) ? 1f : -1f;
                }
                break;

            case TouchPhase.Ended:
                DetectSwipeDown(touch.position);
                break;
        }
    }

    private void ReadKeyboardInput()
    {
        raw = Input.GetAxisRaw("Horizontal");

        if (isGrounded)
        {
            inputDirection = raw;
        }
        else
        {
            airRotationInput = raw;
            inputDirection = 0f;
        }

        isSlowingDown = Input.GetKey(KeyCode.S);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            jumpRequested = true;
    }

    #endregion

    #region Physics and Movement

    private void CheckIfGrounded()
    {
        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck transform is missing!");
            return;
        }
        
        if (groundLayer.value == 0)
        {
            groundLayer = ~(1 << gameObject.layer);
            Debug.LogWarning("GroundLayer was not set! Using all layers except player layer.");
        }
        
        // Cast a ray down to check for ground
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRadius * 1.5f, groundLayer);
        
        // Use both methods for more reliable ground detection
        bool circleCheck = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        isGrounded = circleCheck || hit.collider != null;
        
        // Debug visualization
        if (hit.collider != null)
        {
            Debug.DrawLine(groundCheck.position, hit.point, Color.green, 0.1f);
        }
        else
        {
            Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckRadius * 1.5f, Color.red, 0.1f);
        }
    }

    private void ApplyAirRotation()
    {
        if (Mathf.Abs(airRotationInput) > 0.01f)
        {
            float rotationAmount = -airRotationInput * rotationSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation + rotationAmount);
        }
    }

    private void ApplyMovement()
    {
        if (!isGrounded) return;

        currentSpeed = Vector2.Dot(rb.linearVelocity, transform.right.normalized);
        isBraking = inputDirection != 0 &&
                    Mathf.Sign(inputDirection) != Mathf.Sign(currentSpeed) &&
                    Mathf.Abs(currentSpeed) > 0.1f;

        if (isBraking)
        {
            float boost = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
            Vector2 brakeDirection = -transform.right.normalized;
            rb.AddForce(brakeDirection * brakeForce * boost, ForceMode2D.Force);
        }
        else if (inputDirection != 0)
        {
            float appliedForce = moveForce * (isSlowingDown ? slowDownMultiplier : 1f);
            Vector2 moveDirection = transform.right.normalized;
            rb.AddForce(moveDirection * inputDirection * appliedForce, ForceMode2D.Force);

            Vector3 scale = transform.localScale;
            scale.x = inputDirection > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else
        {
            // Apply less slowdown to maintain momentum
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.95f, rb.linearVelocity.y);
            
            if (Mathf.Abs(currentSpeed) < minForwardSpeed)
            {
                float directionSign = transform.localScale.x > 0 ? 1 : -1;
                rb.AddForce(transform.right.normalized * moveForce * 0.75f * directionSign, ForceMode2D.Force);
            }
        }

        rb.linearVelocity = new Vector2(
            Mathf.Clamp(rb.linearVelocity.x, -maxSpeed, maxSpeed),
            rb.linearVelocity.y
        );

        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            
            // Add a bit of forward momentum when jumping
            float directionSign = transform.localScale.x > 0 ? 1 : -1;
            rb.AddForce(transform.right.normalized * moveForce * 0.5f * directionSign, ForceMode2D.Impulse);
            
            jumpRequested = false;
        }
    }

    private void ApplyConstantForwardForce()
    {
        // Always apply some forward momentum (direction based on character orientation)
        float directionSign = transform.localScale.x > 0 ? 1 : -1;
        rb.AddForce(transform.right.normalized * constantForwardForce * directionSign, ForceMode2D.Force);
    }

    #endregion

    #region Touch Gestures

    private void DetectDoubleTap()
    {
        if (Time.time - lastTapTime < doubleTapThreshold)
        {
            jumpRequested = true;
            lastTapTime = 0f;
        }
        else
        {
            lastTapTime = Time.time;
        }
    }

    private void DetectSwipeDown(Vector2 endTouchPos)
    {
        if (startTouchPos.y - endTouchPos.y > swipeThreshold)
        {
            isSlowingDown = true;
        }
    }

    #endregion

    #region Power-Ups

    #region Speed Boost

    private float originalMoveForce;
    private float boostTimer;
    private bool isBoosted;

    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        if (!isBoosted)
        {
            originalMoveForce = moveForce;
            moveForce *= multiplier;
            boostTimer = duration;
            isBoosted = true;
        }
    }

    private void HandleBoostTimer()
    {
        if (isBoosted)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f)
            {
                moveForce = originalMoveForce;
                isBoosted = false;
            }
        }
    }

    #endregion

    #region Shield

    public bool isShielded;
    [SerializeField] public GameObject shieldVisual;

    public float shieldTimer;
    public float shieldDuration = 5;

    public void ActivateShield(float duration = 5)
    {
        isShielded = true;
        shieldDuration = duration;
        shieldTimer = duration;

        if (shieldVisual != null)
            shieldVisual.SetActive(true);
    }

    private void HandleShieldTimer()
    {
        if (isShielded)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                isShielded = false;
                if (shieldVisual != null)
                    shieldVisual.SetActive(false);
            }
        }

    }

    #endregion

    #region extra life
    public int extraLives = 0;

    public bool HasExtraLife()
    {
        return extraLives > 0;
    }

    public void UseExtraLife()
    {
        extraLives--;
    }
    #endregion
    #endregion

    #region Utilities

    public void DisableControls()
    {
        controlsDisabled = true;
        rb.linearVelocity = Vector2.zero;
    }

    public void CorrectRotation()
    {
        transform.rotation = Quaternion.identity;
        rb.angularVelocity = 0f;

        transform.position += new Vector3(0f, 1f, 0f);
    }

    #endregion
}
