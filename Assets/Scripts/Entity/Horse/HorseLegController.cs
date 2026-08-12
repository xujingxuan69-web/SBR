using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HorseLegController : MonoBehaviour
{
    [Header("IK 约束")]
    [SerializeField] private TwoBoneIKConstraint twoBoneIK;

    [Header("脚底骨骼（动画位置）")]
    [SerializeField] private Transform footBone;

    [Header("IK 目标（独立 Transform）")]
    [SerializeField] private Transform footTarget;

    [Header("地面检测")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 2f;
    [SerializeField] private float footOffset = 0.05f;

    [Header("IK 混合")]
    [SerializeField] private float ikTransitionSpeed = 8f;

    
    
    
    
    
    
    
    
    //公开数据，供身体控制器使用
    public Vector3 TargetPosition { get; private set; }
    public float IKWeight { get; private set; }
    public bool IsGrounded { get; private set; }

    private float currentIKWeight = 0f;

    void Start()
    {
        //初始化 IK 约束设置
        if (twoBoneIK != null)
        {
            twoBoneIK.data.targetPositionWeight = 1f;
            twoBoneIK.data.targetRotationWeight = 0f; //通常只控制位置
        }

        //确保初始目标位置在脚底
        if (footTarget != null && footBone != null)
        {
            footTarget.position = footBone.position;
        }
    }

    //在 LateUpdate 中更新 IK，此时动画已经应用
    void LateUpdate()
    {
        if (footBone == null || footTarget == null) return;

        //1. 获取动画中脚的当前位置
        Vector3 animationFootPos = footBone.position;

        //2. 从脚的位置向下进行地面检测
        Vector3 origin = animationFootPos + Vector3.up * 0.3f;
        RaycastHit hit;

        float ikWeight = 0f;
        Vector3 targetPos = animationFootPos; //默认保持在动画位置
        IsGrounded = false;

        if (Physics.Raycast(origin, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            float distanceToGround = hit.distance - 0.3f; //计算脚底到地面的距离

            //3. 核心逻辑：只有脚明显穿模（在地面以下）时才修正
            if (distanceToGround < -0.02f)
            {
                //将目标位置抬升到地面
                targetPos = hit.point + Vector3.up * footOffset;
                ikWeight = Mathf.Clamp01(Mathf.Abs(distanceToGround) * 3f);
                IsGrounded = true;
            }
            //脚在地面上方或刚好接触，保持动画位置，不启动IK修正
        }
        //未检测到地面，脚悬空，保持动画位置

        //保存公开数据
        TargetPosition = targetPos;
        IKWeight = ikWeight;

        //4. 更新独立的 IK 目标位置
        footTarget.position = targetPos;

        //5. 平滑调整 IK 权重，让修正效果淡入淡出
        currentIKWeight = Mathf.Lerp(currentIKWeight, ikWeight, Time.deltaTime * ikTransitionSpeed);
        if (Mathf.Abs(currentIKWeight) < 0.001f)
        {
            currentIKWeight = 0f;
        }
        twoBoneIK.weight = currentIKWeight;
    }

    //提供一个重置方法，方便外部调用
    public void ResetIK()
    {
        currentIKWeight = 0f;
        twoBoneIK.weight = 0f;
        if (footTarget != null && footBone != null)
        {
            footTarget.position = footBone.position;
        }
    }
}