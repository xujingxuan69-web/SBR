using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public ulong playerId;


    #region Inspector
    [Header("Movement Info")]
    [SerializeField] protected float maxTurnSpeed = 150f;
    [SerializeField] protected float minTurnSpeed = 40f;
    [field: SerializeField] public float maxForwardSpeed { get; protected set; } = 12f;
    [field: SerializeField] public float maxBackwardSpeed { get; protected set; } = 2f;
    [field: SerializeField] public float maxObstacleInFrontSpeed { get; protected set; } = 3f;
    [field: SerializeField] public float forwardAcceleration { get; protected set; } = 3f;
    [field: SerializeField] public float backwardAcceleration { get; protected set; } = 3f;
    [field: SerializeField] public float deceleration { get; protected set; } = 6f;
    [field: SerializeField] public float horizontalSpeed { get; protected set; } = 0f;
    [field: SerializeField] public float verticalSpeed { get; protected set; } = 0f;


    [Header("Jump Info")]
    [SerializeField] protected float jumpForce = 12f;
    [SerializeField] protected float jumpAirDuration = 0.1f;
    private float lastJumpAirTime = -1f;

    [Header("Collision Detection")]
    [SerializeField] protected Vector3 checkSphereOffset = new Vector3(0, 0.5f, 0.8f);
    [field: SerializeField] public float checkSphereRadius { get; protected set; } = 0.6f;
    private Collider[] obstacleHitBuffer = new Collider[10];

    [field: SerializeField] public bool isGrounded { get; protected set; } 
    /*[SerializeField] protected Vector3 groundCheckOffset = new Vector3(0, 0f, 0f);
    [field: SerializeField] public float groundCheckRadius { get; protected set; } = 0.3f;
    [SerializeField] protected LayerMask groundLayerMask;
    [SerializeField] protected float groundedStabilityTime = 0.1f;
    private float _lastGroundedTime = -999f;*/

    [Header("Collider Detection Debug")]
    [SerializeField] protected bool showDebugBox = true;
    [SerializeField] protected Color normalColor = Color.green;
    [SerializeField] protected Color obstacleColor = Color.red;

    [field: SerializeField] public LayerMask groundLayer { get; protected set; }
    #endregion
    #region Component
    public Animator anim { get; private set; }
    public CharacterController cc { get; private set; }
    #endregion
    #region Properties
    public bool IsMoving => Mathf.Abs(horizontalSpeed) > 0.1f;
    public bool IsGrounded => cc.isGrounded;

    protected CollisionFlags _lastCollisionFlags;
    #endregion


    public event System.Action<float> onTurn;


    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
    }

    protected virtual void FixedUpdate()
    {
        //UpdateGroundedState();
        ExecuteMovement();
    }

    #region HorizontalSpeed
    public virtual void SetHorizontalSpeedAs(float _speed)
    {
        horizontalSpeed = Mathf.Clamp(_speed, -maxBackwardSpeed, maxForwardSpeed);
    }

    public virtual void ChangeHorizontalSpeedBy(float _acc)
    {
        bool hasObstacle = IsObstacleInFront();

        if (hasObstacle && horizontalSpeed > maxObstacleInFrontSpeed)
        {
            _acc = Mathf.Min(_acc, 0f);
        }

        horizontalSpeed += _acc * Time.fixedDeltaTime;
        horizontalSpeed = Mathf.Clamp(horizontalSpeed, -maxBackwardSpeed, maxForwardSpeed);
    }
    #endregion
    #region VerticalSpeed
    public virtual void ResetVerticalSpeed()
    {
        verticalSpeed = -1f;
    }

    public virtual void AddVerticalSpeed()
    {
        verticalSpeed += Physics.gravity.y * Time.fixedDeltaTime;
        verticalSpeed = Mathf.Max(verticalSpeed, -30f);
    }

    public virtual void SetVerticalSpeed()
    {
        verticalSpeed = jumpForce;
    }
    #endregion

    public virtual void Turn(float horizontalInput)
    {
        if (!IsMoving) return;

        float speedRatio = Mathf.Abs(horizontalSpeed / maxForwardSpeed);
        float turnSpeed = Mathf.Lerp(minTurnSpeed, maxTurnSpeed, speedRatio * speedRatio);

        float direction = horizontalSpeed > 0 ? 1f : -1f;
        onTurn?.Invoke(direction * horizontalInput);
        transform.Rotate(0, horizontalInput * turnSpeed * direction * Time.fixedDeltaTime, 0);
    }

    protected virtual void ExecuteMovement()
    {
        Vector3 move = transform.forward * horizontalSpeed * Time.fixedDeltaTime;
        move.y = verticalSpeed * Time.fixedDeltaTime;

        _lastCollisionFlags = cc.Move(move);
    }

    #region JumpAirTime
    public void SetJumpAirTime() => lastJumpAirTime = Time.time;
    public bool CheckJumpAirTime() => Time.time <= lastJumpAirTime + jumpAirDuration;
    #endregion
    #region ColliderDetection
    public int GetObstacleInFront() => Physics.OverlapSphereNonAlloc(GetObstacleDetectCenter(), checkSphereRadius, obstacleHitBuffer, groundLayer);
    public virtual bool IsObstacleInFront() => GetObstacleInFront() > 0;
    public Collider[] GetObstacleHitBuffer() => obstacleHitBuffer;
    public Vector3 GetObstacleDetectCenter() => transform.position + transform.forward * checkSphereOffset.z
                                       + transform.up * checkSphereOffset.y
                                       + transform.right * checkSphereOffset.x;
    #endregion
    #region GroundedDetection
    /*private void UpdateGroundedState()
    {
        bool isGroundedFromCollision = (_lastCollisionFlags & CollisionFlags.Below) != 0;
        bool currentlyGrounded = isGroundedFromCollision || CheckIfGrounded();

        if (currentlyGrounded)
        {
            _isGrounded = true;
            _lastGroundedTime = Time.time;
        }
        else
        {
            if (Time.time - _lastGroundedTime < groundedStabilityTime)
            {
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;
            }
        }
    }

    private bool CheckIfGrounded()
    {
        if (cc == null) return false;

        Vector3 sphereCenter = transform.position
                             + transform.forward * groundCheckOffset.z
                             + transform.up * groundCheckOffset.y
                             + transform.right * groundCheckOffset.x;

        Collider[] hitColliders = Physics.OverlapSphere(sphereCenter, groundCheckRadius, groundLayerMask);
        return hitColliders.Length > 0;
    }*/
    #endregion
}