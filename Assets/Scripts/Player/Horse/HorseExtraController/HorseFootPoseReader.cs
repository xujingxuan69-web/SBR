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
    [SerializeField] private Transform leftRearGroundProbe;

    public Transform GetFoot(int index) => index switch
    {
        0 => leftFront,
        1 => rightFront,
        2 => leftRear,
        3 => rightRear,
        _ => null
    };

    public Transform LeftRearGroundProbe => leftRearGroundProbe;

    bool IAnimationJobData.IsValid() => leftFront && rightFront && leftRear && rightRear && leftRearGroundProbe;
    void IAnimationJobData.SetDefaultValues()
    {
        leftFront = rightFront = leftRear = rightRear = null;
        leftRearGroundProbe = null;
    }
}

public struct HorseFootPoseReaderJob : IWeightedAnimationJob    //Job-读取动画流中的数据
{
    public TransformStreamHandle leftFront;
    public TransformStreamHandle rightFront;
    public TransformStreamHandle leftRear;
    public TransformStreamHandle rightRear;
    public TransformStreamHandle leftRearGroundProbe;
    public NativeArray<Vector3> positions;
    public FloatProperty jobWeight { get; set; }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        positions[0] = leftFront.GetPosition(stream);
        positions[1] = rightFront.GetPosition(stream);
        positions[2] = leftRear.GetPosition(stream);
        positions[3] = rightRear.GetPosition(stream);
        positions[4] = leftRearGroundProbe.GetPosition(stream);
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
            leftRearGroundProbe = animator.BindStreamTransform(data.LeftRearGroundProbe),
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
    private readonly Vector3[] sampledPositions = new Vector3[5];
    private float nextGizmoDiagnosticTime;

    internal NativeArray<Vector3> Output => output;

    public Vector3 GetSampledPosition(int footIndex) => sampledPositions[footIndex];
    public Vector3 GetSampledLeftRearGroundProbePosition() => sampledPositions[4];

    internal void InitializeOutput()
    {
        if (!output.IsCreated)
            output = new NativeArray<Vector3>(5, Allocator.Persistent);
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

        if (Application.isPlaying && Time.unscaledTime >= nextGizmoDiagnosticTime)
        {
            nextGizmoDiagnosticTime = Time.unscaledTime + 1f;
            Debug.Log($"[HorseFootPoseReader Gizmo:{name}#{GetInstanceID()}] source=sampledPositions, outputCreated={output.IsCreated}, " +
                      $"leftFront={sampledPositions[0]:F3}, rightFront={sampledPositions[1]:F3}, " +
                      $"leftRear={sampledPositions[2]:F3}, rightRear={sampledPositions[3]:F3}, " +
                      $"leftRearBinding={(data.GetFoot(2) ? data.GetFoot(2).position.ToString("F3") : "<null>")}", this);
        }

        Gizmos.color = Color.cyan;
        for (int i = 0; i < 4; i++)
        {
            Transform foot = data.GetFoot(i);
            if (Application.isPlaying)
                Gizmos.DrawSphere(sampledPositions[i], 0.035f);
            else if (foot)
                Gizmos.DrawSphere(foot.position, 0.035f);
        }

        Gizmos.color = Color.magenta;
        if (Application.isPlaying)
            Gizmos.DrawSphere(sampledPositions[4], 0.035f);
        else if (data.LeftRearGroundProbe)
            Gizmos.DrawSphere(data.LeftRearGroundProbe.position, 0.035f);
    }
}
