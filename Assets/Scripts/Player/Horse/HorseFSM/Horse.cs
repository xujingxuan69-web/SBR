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

    [Header("Ground Rotation")]
    [SerializeField] private float groundRotationRatio = 5f;

    [field: Header("Other")]
    [field: SerializeField] public HorseInputReader InputReader { get; private set; }
    #endregion
    #region RuntimeData
    private float turnInput = 0f;
    private float groundRotationSpeed = 0f;

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

    public override void SetJumpToVerticalSpeed()
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

        
    }

    public void SetTurnInput(float _turnInput) => turnInput = Mathf.Clamp(_turnInput, -1f, 1f);

    #region KCC
    public override void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        base.UpdateRotation(ref currentRotation, deltaTime);
        currentRotation = UpdateTurnRotation(currentRotation, deltaTime);

        if (IsGrounded)
        {
            Vector3 groundNomal = kcc.GroundingStatus.GroundNormal; //表面的朝外法线方向
            Vector3 currentForward = currentRotation * Vector3.forward; //四元数*向量-根据四元数旋转向量，这里Vector3.forward是人物头部的朝向
            //所以currentForward是用于获取马匹当前的前方在世界坐标下的朝向

            
        }

    }

    private Quaternion UpdateTurnRotation(Quaternion currentRotation, float deltaTime)
    {
        float speedRatio = Mathf.Clamp01(Mathf.Abs(horizontalSpeed) / maxForwardSpeed);
        float turnSpeed = Mathf.Lerp(minTurnSpeed, maxTurnSpeed, speedRatio);

        float turnDirection = turnInput * (horizontalSpeed < -0.1f ? -1 : 1); //后退需要反转旋转方向
        float yaw = turnDirection * turnSpeed * deltaTime;  //当前帧需要旋转的角度大小

        currentRotation = Quaternion.AngleAxis(yaw, Vector3.up) * currentRotation;
        //四元数Quaternion表示旋转的行为，如这里的Quaternion.AngleAxis就表示根据Vector3.up所在的Y轴，旋转yaw度，也就是在XZ平面顺时针旋转yaw度
        //四元数相乘，就表示旋转的叠加，但是旋转顺序的不同，就会导致结果不同
        //举个例子，空间坐标系如下：朝前是X轴，朝右是Y轴，朝上是Z轴
        //目前一个向量从原点出发朝向X轴，即(1,0,0)，现在有两个动作：①绕Y轴逆时针旋转90°；②绕X轴逆时针旋转90°
        //这两个动作的叠加，如果①*②，会出现(0,1,0)，如果是②*①，则会出现(0,0,1)
        //

        onTurn?.Invoke(turnDirection);
        return currentRotation;
    }

    public override void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        base.UpdateVelocity(ref currentVelocity, deltaTime);
    }
    #endregion
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
