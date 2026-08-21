using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Horse : Entity
{
    #region State
    public EntityStateMachine<Horse> stateMachine { get; private set; }

    public HorseGroundState groundState { get; private set; }
    public HorseJumpState jumpState { get; private set; }
    public HorseFallState fallState { get; private set; }
    #endregion
    #region Obstacle
    public event System.Action onObstacleInFront;
    public event System.Action onObstacleClear;
    #endregion
    #region Slide
    [Header("Slope Slide Settings")]
    [SerializeField] protected float slopeBufferDuration = 0.3f;
    private float _slopeAccumulatedTime = 0f;
    private float _slopeAngle = 0f;

    [field: SerializeField] public float maxSlopeSlideSpeed { get; protected set; } = 15f;
    [field: SerializeField] public float slopeSlideAcceleration { get; protected set; } = 3f;
    public Vector3 SlopeSlideDirection { get; private set; } = Vector3.zero;
    #endregion
    [SerializeField] private float minJumpForceRate;

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EntityStateMachine<Horse>();

        groundState = new HorseGroundState(this, stateMachine, "Grounded");
        jumpState = new HorseJumpState(this, stateMachine, "Air");
        fallState = new HorseFallState(this, stateMachine, "Air");
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
    public bool IsOnSlope() => _slopeAccumulatedTime > slopeBufferDuration;

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

        if (SlopeSlideDirection != Vector3.zero && IsOnSlope())
        {
            Vector3 slideDir = SlopeSlideDirection.normalized;

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
    protected virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.moveDirection.y >= 0) return;

        Vector3 normal = hit.normal;
        float angle = Vector3.Angle(normal, Vector3.up);

        if (angle > cc.slopeLimit)
        {
            _slopeAngle = angle;

            Vector3 slopeDown = Vector3.ProjectOnPlane(Vector3.down, normal).normalized;
            SlopeSlideDirection = slopeDown;

            _slopeAccumulatedTime += Time.fixedDeltaTime;
        }
        else
        {
            _slopeAccumulatedTime = 0f;
            SlopeSlideDirection = Vector3.zero;
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