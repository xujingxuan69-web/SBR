using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Horse : Entity
{
    #region Inspector
    [field: MergeHeader("Movement Info")]
    [field: SerializeField] protected float maxTurnSpeed { get; private set; } = 150f;
    [field: SerializeField] protected float minTurnSpeed { get; private set; } = 40f;

    [MergeHeader("Jump Info")]
    [SerializeField] private float minJumpForceRate;

    [MergeHeader("Collision Detection")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Vector3 obstacleCheckSphereOffset = new Vector3(0, 0.5f, 0.8f);
    [field: SerializeField] public float obstacleCheckSphereRadius { get; private set; } = 0.6f;

    [field: Header("Other")]
    [field: SerializeField] public HorseInputReader InputReader { get; private set; }
    #endregion
    #region RuntimeData
    private float turnSpeed = 0f;
    private float turnInput = 0f;

    private Collider[] obstacleHitBuffer = new Collider[10];
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

    protected override void Awake()
    {
        base.Awake();
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

    public override void SetVerticalSpeed()
    {
        float verticalSpeedRate = Mathf.Clamp(horizontalSpeed / maxForwardSpeed, minJumpForceRate, 1);
        verticalSpeed = jumpForce * verticalSpeedRate;
    }

    public override void ChangeHorizontalSpeedBy(float _acc)
    {
        if (IsObstacleInFront() && horizontalSpeed > maxObstacleInFrontSpeed)
        {
            _acc = Mathf.Min(_acc, 0f);
        }

        horizontalSpeed += _acc * Time.fixedDeltaTime;
        horizontalSpeed = Mathf.Clamp(horizontalSpeed, -maxBackwardSpeed, maxForwardSpeed);

        float speedRatio = Mathf.Clamp01(Mathf.Abs(horizontalSpeed) / maxForwardSpeed);
        float turnSpeed = Mathf.Lerp(minTurnSpeed, maxTurnSpeed, speedRatio);
    }

    public void SetTurnInput(float _turnInput) => turnInput = Mathf.Clamp(_turnInput, -1f, 1f);

    #region KCC
    public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        base.UpdateRotation(ref currentRotation, deltaTime);

    }
    #endregion

    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        base.UpdateVelocity(ref currentVelocity, deltaTime);
    }

    #region ObstacleDetection
    public int GetObstacleInFront() => Physics.OverlapSphereNonAlloc(GetObstacleDetectCenter(), obstacleCheckSphereRadius, obstacleHitBuffer, obstacleLayer);
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
    public Vector3 GetObstacleDetectCenter() => transform.position + transform.forward * obstacleCheckSphereOffset.z
                                       + transform.up * obstacleCheckSphereOffset.y
                                       + transform.right * obstacleCheckSphereOffset.x;
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
