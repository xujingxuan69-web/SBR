using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class Horse : Entity
{
    #region Inspector
    [field: MergeHeader("Movement Info")]
    [field: SerializeField] protected float maxTurnSpeed { get; private set; } = 150f;
    [field: SerializeField] protected float minTurnSpeed { get; private set; } = 40f;

    [MergeHeader("Jump Info")]
    [SerializeField] private float minJumpForceRate;

    [Header("Slope Movement")]
    [SerializeField, Min(0f)] private float groundVelocitySharpness = 15f;
    [SerializeField, Min(0f)] private float steepSlopeContactGraceTime = 0.12f;
    [SerializeField, Min(0f)] private float groundedStateGraceTime = 0.12f;

    [MergeHeader("Collision Detection")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Vector3 obstacleCheckSphereOffset = new Vector3(0, 0.5f, 0.8f);
    [field: SerializeField] public float obstacleCheckSphereRadius { get; private set; } = 0.6f;

    [field: Header("Other")]
    [field: SerializeField] public HorseInputReader InputReader { get; private set; }
    #endregion
    #region RuntimeData
    private float turnInput = 0f;
    private bool jumpVelocityPending;
    private Vector3 steepSlopeUphillDirection;
    private float lastSteepSlopeContactTime = float.NegativeInfinity;
    private float lastGroundContactTime = float.NegativeInfinity;
    private Quaternion bodyRotation;

    private Collider[] obstacleHitBuffer = new Collider[10];

    public Quaternion shareRotation { get; private set; }
    #endregion
    #region Action
    public event System.Action onObstacleInFront;
    public event System.Action onObstacleClear;
    public event System.Action<float> onTurn;
    #endregion
    #region State
    public EntityStateMachine<Horse> stateMachine { get; private set; }

    public HorseGroundedState groundedState { get; private set; }
    public HorseJumpState jumpState { get; private set; }
    public HorseAirState airState { get; private set; }
    #endregion

    public bool IsBlockedBySteepSlope =>
        stateMachine != null && stateMachine.currentState != jumpState &&
        Time.time - lastSteepSlopeContactTime <= steepSlopeContactGraceTime;

    public bool HasGroundingGrace =>
        stateMachine != null && stateMachine.currentState != jumpState &&
        Time.time - lastGroundContactTime <= groundedStateGraceTime;

    protected override void Awake()
    {
        base.Awake();
        bodyRotation = transform.rotation;
        stateMachine = new EntityStateMachine<Horse>();

        groundedState = new HorseGroundedState(this, stateMachine, "Grounded");
        jumpState = new HorseJumpState(this, stateMachine, "Air");
        airState = new HorseAirState(this, stateMachine, "Air");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(groundedState);
    }

    protected override void FixedUpdate()
    {
        stateMachine.currentState.FixedUpdate();
        base.FixedUpdate();
    }

    public override void SetJumpToVerticalSpeed()
    {
        float verticalSpeedRate = Mathf.Clamp(horizontalSpeed / maxForwardSpeed, minJumpForceRate, 1);
        verticalSpeed = jumpForce * verticalSpeedRate;
        jumpVelocityPending = true;
    }

    public override void ChangeHorizontalSpeedBy(float _acc)
    {
        if (IsObstacleInFront() && horizontalSpeed > maxObstacleInFrontSpeed)
        {
            _acc = Mathf.Min(_acc, 0f);
        }

        horizontalSpeed += _acc * Time.fixedDeltaTime;
        horizontalSpeed = Mathf.Clamp(horizontalSpeed, -maxBackwardSpeed, maxForwardSpeed);
    }

    public void SetTurnInput(float _turnInput) => turnInput = Mathf.Clamp(_turnInput, -1f, 1f);

    public void SetBodyRotation(Quaternion rotation) => bodyRotation = rotation;

    public override void PostGroundingUpdate(float deltaTime)
    {
        base.PostGroundingUpdate(deltaTime);

        if (stateMachine.currentState != jumpState &&
            (IsStableGrounded || IsUnstableGrounded || IsBlockedBySteepSlope))
        {
            lastGroundContactTime = Time.time;
        }
    }

    public override void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport)
    {
        base.OnMovementHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
        /*if (stateMachine != null && stateMachine.currentState == jumpState)
            return;

        float slopeAngle = Vector3.Angle(kcc.CharacterUp, hitNormal);
        if (slopeAngle <= kcc.MaxStableSlopeAngle || slopeAngle >= 89f)
            return;

        Vector3 horizontalNormal = Vector3.ProjectOnPlane(hitNormal, kcc.CharacterUp);
        if (horizontalNormal.sqrMagnitude <= 0.0001f)
            return;

        Vector3 uphillDirection = -horizontalNormal.normalized;
        if (Vector3.Dot(kcc.BaseVelocity, uphillDirection) <= 0.01f)
            return;

        steepSlopeUphillDirection = uphillDirection;
        lastSteepSlopeContactTime = Time.time;*/
    }

    #region KCC
    public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        base.UpdateRotation(ref currentRotation, deltaTime);
        UpdateTurnRotation(ref currentRotation, deltaTime);
        shareRotation = currentRotation;
    }

    private void UpdateTurnRotation(ref Quaternion currentRotation, float deltaTime)
    {
        float speedRatio = Mathf.Clamp01(Mathf.Abs(horizontalSpeed) / maxForwardSpeed);
        float turnSpeed = IsStableGrounded ? Mathf.Lerp(minTurnSpeed, maxTurnSpeed, speedRatio) : minTurnSpeed * 0.5f;

        float turnDirection = turnInput * (horizontalSpeed < -0.1f ? -1 : 1); //后退需要反转旋转方向
        float yaw = turnDirection * turnSpeed * deltaTime;  //当前帧需要旋转的角度大小

        currentRotation = Quaternion.AngleAxis(yaw, Vector3.up) * currentRotation;
        //四元数Quaternion表示旋转的行为，如这里的Quaternion.AngleAxis就表示根据Vector3.up所在的Y轴，旋转yaw度，也就是在XZ平面顺时针旋转yaw度
        //四元数相乘，就表示旋转的叠加，但是旋转顺序的不同，就会导致结果不同
        //举个例子，空间坐标系如下：朝前是X轴，朝右是Y轴，朝上是Z轴
        //目前一个向量从原点出发朝向X轴，即(1,0,0)，现在有两个动作：①绕Y轴逆时针旋转90°；②绕X轴逆时针旋转90°
        //这两个动作的叠加，如果①*②，会出现(0,1,0)，如果是②*①，则会出现(0,0,1)
        //这里把currentRotation放右边，就是先按照初始的当前坐标轴（世界坐标轴）进行左右旋转，再叠加自身的旋转
        //如果是currentRotation * Quaternion.AngleAxis(yaw, Vector3.up)，则是在自身的旋转上叠加当前坐标轴旋转，这样可能导致旋转错误

        onTurn?.Invoke(turnDirection);
    }

    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        var grounding = kcc.GroundingStatus;
        bool justJumped = jumpVelocityPending;
        bool blockedBySteepSlope = !justJumped && IsBlockedBySteepSlope;

        if (justJumped)
        {
            currentVelocity = transform.forward * horizontalSpeed;
            currentVelocity.y = verticalSpeed;
            jumpVelocityPending = false;
        }

        if (!justJumped && grounding.IsStableOnGround && !blockedBySteepSlope)
        {
            Vector3 targetVelocity = kcc.GetDirectionTangentToSurface(
                transform.forward, grounding.GroundNormal) * horizontalSpeed;

            if (currentVelocity.sqrMagnitude > 0.0001f)
            {
                currentVelocity = kcc.GetDirectionTangentToSurface(
                    currentVelocity, grounding.GroundNormal) * currentVelocity.magnitude;
            }

            float blend = 1f - Mathf.Exp(-groundVelocitySharpness * deltaTime);
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, blend);
            verticalSpeed = 0f;
            return;
        }

        bool unstableGround = grounding.FoundAnyGround && !grounding.IsStableOnGround;
        if (!justJumped && (unstableGround || blockedBySteepSlope))
        {
            Vector3 uphillDirection = blockedBySteepSlope
                ? steepSlopeUphillDirection
                : -Vector3.ProjectOnPlane(grounding.GroundNormal, kcc.CharacterUp).normalized;

            if (uphillDirection.sqrMagnitude > 0.0001f)
            {
                uphillDirection.Normalize();
                float verticalVelocity = Vector3.Dot(currentVelocity, kcc.CharacterUp);
                if (blockedBySteepSlope && verticalVelocity > 0f)
                    verticalVelocity = 0f;
                Vector3 desiredVelocity = transform.forward * horizontalSpeed;
                float uphillVelocity = Vector3.Dot(desiredVelocity, uphillDirection);
                if (uphillVelocity > 0f)
                    desiredVelocity -= uphillDirection * uphillVelocity;

                currentVelocity = desiredVelocity + kcc.CharacterUp * verticalVelocity;
            }

        }

        currentVelocity += Physics.gravity * deltaTime;
        verticalSpeed = currentVelocity.y;
    }
    #endregion

    #region ObstacleDetection
    public int GetObstacleInFront()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            GetObstacleDetectCenter(), obstacleCheckSphereRadius, obstacleHitBuffer, obstacleLayer);
        int supportedCount = 0;

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = obstacleHitBuffer[i];
            if (!SupportsClosestPoint(hit))
                continue;

            obstacleHitBuffer[supportedCount++] = hit;
        }

        return supportedCount;
    }

    private static bool SupportsClosestPoint(Collider collider) =>
        collider is BoxCollider || collider is SphereCollider || collider is CapsuleCollider ||
        collider is MeshCollider meshCollider && meshCollider.convex;

    public bool IsObstacleInFront()
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
    public Collider[] GetObstacleHitBuffer() => obstacleHitBuffer;
    public Vector3 GetObstacleDetectCenter() => transform.position + bodyRotation * obstacleCheckSphereOffset;
    #endregion

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Vector3 center = GetObstacleDetectCenter();

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, obstacleCheckSphereRadius);
        Gizmos.DrawSphere(center, 0.08f);
    }
}
