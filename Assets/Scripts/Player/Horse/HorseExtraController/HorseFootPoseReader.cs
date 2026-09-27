using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

[System.Serializable]
public struct HorseFootPoseReaderData : IAnimationJobData   //JobData-数据容器
{
    [SerializeField] private Transform leftFront;
    [SerializeField] private Transform rightFront;
    [SerializeField] private Transform leftRear;
    [SerializeField] private Transform rightRear;

    public Transform GetFoot(int index) => index switch
    {
        0 => leftFront,
        1 => rightFront,
        2 => leftRear,
        3 => rightRear,
        _ => null
    };

    bool IAnimationJobData.IsValid() => leftFront && rightFront && leftRear && rightRear;
    void IAnimationJobData.SetDefaultValues()
    {
        leftFront = rightFront = leftRear = rightRear = null;
    }
}

public struct HorseFootPoseReaderJob : IWeightedAnimationJob    //Job-读取动画流中的数据
{
    public TransformStreamHandle leftFront;
    public TransformStreamHandle rightFront;
    public TransformStreamHandle leftRear;
    public TransformStreamHandle rightRear;
    public NativeArray<Vector3> positions;
    public FloatProperty jobWeight { get; set; }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        positions[0] = leftFront.GetPosition(stream);
        positions[1] = rightFront.GetPosition(stream);
        positions[2] = leftRear.GetPosition(stream);
        positions[3] = rightRear.GetPosition(stream);
    }
}

public class HorseFootPoseReaderJobBinder : AnimationJobBinder<HorseFootPoseReaderJob, HorseFootPoseReaderData> //JobBinder-把JobData和Job连接起来，同时管理Job的生命周期
{
    public override HorseFootPoseReaderJob Create(Animator animator, ref HorseFootPoseReaderData data, Component component)
    {
        var reader = (HorseFootPoseReader)component;
        reader.InitializeOutput();

        return new HorseFootPoseReaderJob
        {
            leftFront = animator.BindStreamTransform(data.GetFoot(0)),
            rightFront = animator.BindStreamTransform(data.GetFoot(1)),
            leftRear = animator.BindStreamTransform(data.GetFoot(2)),
            rightRear = animator.BindStreamTransform(data.GetFoot(3)),
            positions = reader.Output
        };
    }

    public override void Destroy(HorseFootPoseReaderJob job)
    {
        if (job.positions.IsCreated)
            job.positions.Dispose();
    }
}

[AddComponentMenu("Animation Rigging/Horse Foot Pose Reader")]
public class HorseFootPoseReader : RigConstraint<HorseFootPoseReaderJob, HorseFootPoseReaderData, HorseFootPoseReaderJobBinder> //约束组件
{
    [SerializeField] private bool showDebugGizmos = true;
    private NativeArray<Vector3> output;
    private readonly Vector3[] sampledPositions = new Vector3[4];

    internal NativeArray<Vector3> Output => output;

    public Vector3 GetSampledPosition(int footIndex) => sampledPositions[footIndex];

    internal void InitializeOutput()
    {
        if (!output.IsCreated)
            output = new NativeArray<Vector3>(4, Allocator.Persistent);
    }

    private void LateUpdate()
    {
        if (!output.IsCreated) return;

        for (int i = 0; i < sampledPositions.Length; i++)
            sampledPositions[i] = output[i];
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < sampledPositions.Length; i++)
            Gizmos.DrawSphere(sampledPositions[i], 0.035f);
    }
}
