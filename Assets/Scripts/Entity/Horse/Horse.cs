using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Horse : Entity
{
    #region State
    public EntityStateMachine<Horse> stateMachine { get; private set; }

    public HorseGroundedState groundState { get; private set; }
    public HorseJumpState jumpState { get; private set; }
    public HorseAirState airState { get; private set; }
    #endregion
    #region Obstacle
    public event System.Action onObstacleInFront;
    public event System.Action onObstacleClear;
    #endregion
    #region Inspector
    #region Slide
    [Header("Slope Slide Settings")]
    [SerializeField] protected float slopeSpeed = 0f;
    [SerializeField] protected float slopeBufferDuration = 0.3f;
    private float _slopeAccumulatedTime = 0f;
    private float _slopeAngle = 0f;

    [field: SerializeField] public float maxSlopeSlideSpeed { get; protected set; } = 15f;
    [field: SerializeField] public float slopeSlideAcceleration { get; protected set; } = 3f;
    private Vector3 slopeSlideDirection = Vector3.zero;
    #endregion

    [Header("Other")]
    [SerializeField] private float minJumpForceRate;
    [field: SerializeField] public HorseInputReader InputReader { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EntityStateMachine<Horse>();

        groundState = new HorseGroundedState(this, stateMachine, "Grounded");
        jumpState = new HorseJumpState(this, stateMachine, "Air");
        airState = new HorseAirState(this, stateMachine, "Air");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(groundState);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        stateMachine.currentState.FixedUpdate();
    }
    

    public override bool IsObstacleInFront()
    {
        int count = GetObstacleInFront();
        if (count > 0)
        {
            onObstacleInFront?.Invoke();
            return true;
        }
        else
        {
            onObstacleClear?.Invoke();
            return false;
        }
    }

    public override void SetVerticalSpeed()
    {
        float verticalSpeedRate = Mathf.Clamp(horizontalSpeed / maxForwardSpeed, minJumpForceRate, 1);
        verticalSpeed = jumpForce * verticalSpeedRate;
    }

    protected override void ExecuteMovement()
    {
        Vector3 move;

        if (slopeSlideDirection != Vector3.zero && IsSlopeFall())
        {
            Vector3 slideDir = slopeSlideDirection.normalized;

            float gravityComponent = Mathf.Abs(Physics.gravity.y) * Mathf.Sin(_slopeAngle * Mathf.Deg2Rad);
            float acc = gravityComponent * 0.5f + slopeSlideAcceleration;
            acc = Mathf.Min(acc, maxSlopeSlideSpeed);

            Vector3 forward = transform.forward;
            float dot = Vector3.Dot(slideDir, forward);

            float direction = Mathf.Sign(dot);
            if (Mathf.Abs(dot) < 0.1f) direction = 1f;

            horizontalSpeed += acc * Time.fixedDeltaTime * direction;

            move = forward * horizontalSpeed * Time.fixedDeltaTime;
            move.y = verticalSpeed * Time.fixedDeltaTime;
        }
        else
        {
            move = transform.forward * horizontalSpeed * Time.fixedDeltaTime;
            move.y = verticalSpeed * Time.fixedDeltaTime;
        }

        _lastCollisionFlags = cc.Move(move);
    }

    #region Controller Collider Hit
    public bool IsOnSlope() => _slopeAngle > cc.slopeLimit;

    public bool IsSlopeFall() => _slopeAccumulatedTime > slopeBufferDuration;

    protected virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.moveDirection.y >= 0) return;

        Vector3 normal = hit.normal;
        _slopeAngle = Vector3.Angle(normal, Vector3.up);
        if (_slopeAngle > cc.slopeLimit)
        {
           

            Vector3 slopeDown = Vector3.ProjectOnPlane(Vector3.down, normal).normalized;
            slopeSlideDirection = slopeDown;

            _slopeAccumulatedTime += Time.fixedDeltaTime;
        }
        else
        {
            _slopeAccumulatedTime = 0f;
            slopeSlideDirection = Vector3.zero;
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (!showDebugBox) return;

        Vector3 center = GetObstacleDetectCenter();

        Gizmos.color = normalColor;
        Gizmos.DrawWireSphere(center, checkSphereRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(center, 0.08f);

        /*if (cc != null)
        {
            Vector3 groundCenter = transform.position
                                 + transform.forward * groundCheckOffset.z
                                 + transform.up * groundCheckOffset.y
                                 + transform.right * groundCheckOffset.x;

            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCenter, groundCheckRadius);
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(transform.position, groundCenter);
        }*/
    }
}
