using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class Entity : MonoBehaviour, ICharacterController
{
    public ulong playerId;

    #region Inspector
    [field: Header("Movement Info")]
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

    [field: Header("Collider Detection")]
    [field: SerializeField] public bool isGrounded { get; protected set; }

    [Header("Collider Detection Debug")]
    [SerializeField] protected bool showDebugBox = true;
    #endregion
    #region Component
    public Animator anim { get; private set; }
    public KinematicCharacterMotor kcc { get; private set; }
    #endregion
    #region Properties
    public bool IsMoving => Mathf.Abs(horizontalSpeed) > 0.1f;
    public bool IsGrounded => isGrounded;
    #endregion



    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        kcc = GetComponent<KinematicCharacterMotor>();
        kcc.CharacterController = this;
    }

    protected virtual void FixedUpdate()
    {
    }

    #region HorizontalSpeed
    public virtual void SetHorizontalSpeedAs(float _speed)
    {
        horizontalSpeed = Mathf.Clamp(_speed, -maxBackwardSpeed, maxForwardSpeed);
    }

    public virtual void ChangeHorizontalSpeedBy(float _acc)
    {
        horizontalSpeed += _acc * Time.fixedDeltaTime;
        horizontalSpeed = Mathf.Clamp(horizontalSpeed, -maxBackwardSpeed, maxForwardSpeed);
    }
    #endregion
    #region VerticalSpeed
    public virtual void ResetVerticalSpeed()
    {
        verticalSpeed = 0f;
    }

    public virtual void ApplyGravityToVerticalSpeed()
    {
        verticalSpeed += Physics.gravity.y * Time.fixedDeltaTime;
    }

    public virtual void SetJumpToVerticalSpeed()
    {
        verticalSpeed = jumpForce;
    }

    public virtual void AddVerticalSpeed(float _verticalSpeed)
    {
        verticalSpeed += _verticalSpeed;
    }

    public virtual void SetVerticalSpeed(float _verticalSpeed)
    {
        verticalSpeed = _verticalSpeed;
    }

    #endregion
    #region KCC
    public virtual void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
    }

    public virtual void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        currentVelocity = transform.forward * horizontalSpeed;
        currentVelocity.y = verticalSpeed;
    }

    public virtual void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public virtual void PostGroundingUpdate(float deltaTime)
    {
        isGrounded = kcc.GroundingStatus.IsStableOnGround;
    }

    public virtual void AfterCharacterUpdate(float deltaTime)
    {
    }

    public virtual bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public virtual void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public virtual void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public virtual void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    public virtual void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }
    #endregion

    protected virtual void OnDrawGizmos()
    {
        if (!showDebugBox) return;
    }
}
