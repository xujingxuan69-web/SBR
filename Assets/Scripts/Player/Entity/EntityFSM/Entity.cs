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
    [field: SerializeField] public LayerMask groundLayer { get; protected set; }


    [Header("Collider Detection Debug")]
    [SerializeField] protected bool showDebugBox = true;
    [SerializeField] protected Color normalColor = Color.green;
    [SerializeField] protected Color obstacleColor = Color.red;

    #endregion
    #region Component
    public Animator anim { get; private set; }
    protected KinematicCharacterMotor kcc;
    #endregion
    #region Properties
    public bool IsMoving => Mathf.Abs(horizontalSpeed) > 0.1f;
    public bool IsGrounded => isGrounded;
    #endregion


    public event System.Action<float> onTurn;


    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        kcc = GetComponent<KinematicCharacterMotor>();
    }

    protected virtual void FixedUpdate()
    {
        //UpdateGroundedState();
        //ExecuteMovement();
    }

    #region HorizontalSpeed
    public virtual void SetHorizontalSpeedAs(float _speed)
    {
        horizontalSpeed = Mathf.Clamp(_speed, -maxBackwardSpeed, maxForwardSpeed);
    }

    public virtual void ChangeHorizontalSpeedBy(float _acc)
    {
        if ( horizontalSpeed > maxObstacleInFrontSpeed)
        {
            _acc = Mathf.Min(_acc, 0f);
        }//!没看懂我这里为什么要这样

        horizontalSpeed += _acc * Time.fixedDeltaTime;
        horizontalSpeed = Mathf.Clamp(horizontalSpeed, -maxBackwardSpeed, maxForwardSpeed);
    }
    #endregion
    #region VerticalSpeed
    public virtual void ResetVerticalSpeed()
    {
        verticalSpeed = 0f;
    }

    public virtual void AddVerticalSpeed()
    {
        verticalSpeed += Physics.gravity.y * Time.fixedDeltaTime;
    }

    public virtual void SetVerticalSpeed()
    {
        verticalSpeed = jumpForce;
    }

    #endregion
    #region KCC
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        throw new System.NotImplementedException();
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        throw new System.NotImplementedException();
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        throw new System.NotImplementedException();
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        throw new System.NotImplementedException();
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        throw new System.NotImplementedException();
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        throw new System.NotImplementedException();
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        throw new System.NotImplementedException();
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        throw new System.NotImplementedException();
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        throw new System.NotImplementedException();
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        throw new System.NotImplementedException();
    }
    #endregion


    /* protected virtual void ExecuteMovement()
     {
         Vector3 move = transform.forward * horizontalSpeed * Time.fixedDeltaTime;
         move.y = verticalSpeed * Time.fixedDeltaTime;

         _lastCollisionFlags = cc.Move(move);
     }*/


    /* #region GroundedDetection
     private void UpdateGroundedState()
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
     }
     #endregion*/
}
