using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

[System.Serializable]
public struct HorseFootPoseReaderData : IAnimationJobData   //JobData-数据容器
{
    [SerializeField] private Transform leftFront;
    [SerializeField] private Transform rightFront;
    [SerializeField] private Transform leftRearLeg1;
    [SerializeField] private Transform leftRearLeg2;
    [SerializeField] private Transform leftRear;
    [SerializeField] private Transform leftRearFoot;
    [SerializeField] private Transform leftRearIKTarget;
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

    public Transform LeftRearLeg1 => leftRearLeg1;
    public Transform LeftRearLeg2 => leftRearLeg2;
    public Transform LeftRearFoot => leftRearFoot;
    public Transform LeftRearIKTarget => leftRearIKTarget;
    public Transform LeftRearGroundProbe => leftRearGroundProbe;

    bool IAnimationJobData.IsValid() => leftFront && rightFront && leftRearLeg1 && leftRearLeg2 && leftRear &&
                                        leftRearFoot && leftRearIKTarget && rightRear && leftRearGroundProbe;
    void IAnimationJobData.SetDefaultValues()
    {
        leftFront = rightFront = leftRearLeg1 = leftRearLeg2 = leftRear = leftRearFoot =
            leftRearIKTarget = rightRear = null;
        leftRearGroundProbe = null;
    }
}

public struct HorseFootPoseDebugSnapshot
{
    public Vector3 beforePosition;
    public Quaternion beforeRotation;
    public Vector3 afterPosition;
    public Quaternion afterRotation;
    public int handleValid;
}

public struct HorseFootPoseReaderJob : IWeightedAnimationJob    //Job-读取动画流中的数据
{
    public TransformStreamHandle leftFront;
    public TransformStreamHandle rightFront;
    public TransformStreamHandle leftRearLeg1;
    public TransformStreamHandle leftRearLeg2;
    public TransformStreamHandle leftRear;
    public TransformStreamHandle leftRearFoot;
    public TransformStreamHandle leftRearIKTarget;
    public TransformStreamHandle rightRear;
    public TransformStreamHandle leftRearGroundProbe;
    public NativeArray<Vector3> positions;
    public NativeArray<HorseFootPoseDebugSnapshot> debugSnapshots;
    public NativeArray<int> processCallCount;
    public FloatProperty jobWeight { get; set; }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        processCallCount[0]++;
        positions[0] = leftFront.GetPosition(stream);
        positions[1] = rightFront.GetPosition(stream);
        positions[2] = leftRear.GetPosition(stream);
        positions[3] = rightRear.GetPosition(stream);
        positions[4] = leftRearGroundProbe.GetPosition(stream);

        // Update the goal in this animation evaluation, before the downstream Two Bone IK job.
        if (leftRearIKTarget.IsValid(stream))
        {
            leftRearIKTarget.SetPosition(stream, positions[2]);
            positions[5] = leftRearIKTarget.GetPosition(stream);
        }

        // Capture the animation pose before downstream IK constraints modify the chain.
        debugSnapshots[0] = ReadAnimatedPose(stream, leftRearLeg1);
        debugSnapshots[1] = ReadAnimatedPose(stream, leftRearLeg2);
        debugSnapshots[2] = WriteAnimatedPose(stream, leftRear);
        debugSnapshots[3] = ReadAnimatedPose(stream, leftRearFoot);
    }

    private static HorseFootPoseDebugSnapshot ReadAnimatedPose(AnimationStream stream, TransformStreamHandle bone)
    {
        if (!bone.IsValid(stream))
            return new HorseFootPoseDebugSnapshot { handleValid = 0 };

        var pose = new HorseFootPoseDebugSnapshot
        {
            beforePosition = bone.GetLocalPosition(stream),
            beforeRotation = bone.GetLocalRotation(stream),
            handleValid = 1
        };
        pose.afterPosition = pose.beforePosition;
        pose.afterRotation = pose.beforeRotation;
        return pose;
    }

    private static HorseFootPoseDebugSnapshot WriteAnimatedPose(AnimationStream stream, TransformStreamHandle bone)
    {
        if (!bone.IsValid(stream))
            return new HorseFootPoseDebugSnapshot { handleValid = 0 };

        var snapshot = new HorseFootPoseDebugSnapshot
        {
            beforePosition = bone.GetLocalPosition(stream),
            beforeRotation = bone.GetLocalRotation(stream),
            handleValid = 1
        };

        bone.SetLocalPosition(stream, snapshot.beforePosition);
        bone.SetLocalRotation(stream, snapshot.beforeRotation);
        snapshot.afterPosition = bone.GetLocalPosition(stream);
        snapshot.afterRotation = bone.GetLocalRotation(stream);
        return snapshot;
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
            leftRearLeg1 = animator.BindStreamTransform(data.LeftRearLeg1),
            leftRearLeg2 = animator.BindStreamTransform(data.LeftRearLeg2),
            leftRear = animator.BindStreamTransform(data.GetFoot(2)),
            leftRearFoot = animator.BindStreamTransform(data.LeftRearFoot),
            leftRearIKTarget = animator.BindStreamTransform(data.LeftRearIKTarget),
            rightRear = animator.BindStreamTransform(data.GetFoot(3)),
            leftRearGroundProbe = animator.BindStreamTransform(data.LeftRearGroundProbe),
            positions = reader.Output,
            debugSnapshots = reader.DebugSnapshots,
            processCallCount = reader.ProcessCallCount
        };
    }

    public override void Destroy(HorseFootPoseReaderJob job)
    {
        if (job.positions.IsCreated)
            job.positions.Dispose();
        if (job.debugSnapshots.IsCreated)
            job.debugSnapshots.Dispose();
        if (job.processCallCount.IsCreated)
            job.processCallCount.Dispose();
    }
}

[AddComponentMenu("Animation Rigging/Horse Foot Pose Reader")]
public class HorseFootPoseReader : RigConstraint<HorseFootPoseReaderJob, HorseFootPoseReaderData, HorseFootPoseReaderJobBinder> //约束组件
{
    [SerializeField] private bool turnOnDebug;
    [SerializeField] private bool showAnimationJobDebug = true;

    [SerializeField] private bool showDebugGizmos = true;
    private NativeArray<Vector3> output;
    private NativeArray<HorseFootPoseDebugSnapshot> debugSnapshots;
    private NativeArray<int> processCallCount;
    private readonly Vector3[] sampledPositions = new Vector3[5];
    private float nextGizmoDiagnosticTime;
    private float nextPoseDiagnosticTime;

    internal NativeArray<Vector3> Output => output;
    internal NativeArray<HorseFootPoseDebugSnapshot> DebugSnapshots => debugSnapshots;
    internal NativeArray<int> ProcessCallCount => processCallCount;

    public Vector3 GetSampledPosition(int footIndex) => sampledPositions[footIndex];
    public Vector3 GetSampledLeftRearGroundProbePosition() => sampledPositions[4];
    public Vector3 GetAnimationJobIKTargetPosition() => output.IsCreated ? output[5] : Vector3.zero;

    internal void InitializeOutput()
    {
        if (output.IsCreated && output.Length != 6)
            output.Dispose();
        if (!output.IsCreated)
            output = new NativeArray<Vector3>(6, Allocator.Persistent);
        if (!debugSnapshots.IsCreated)
            debugSnapshots = new NativeArray<HorseFootPoseDebugSnapshot>(4, Allocator.Persistent);
        if (!processCallCount.IsCreated)
            processCallCount = new NativeArray<int>(1, Allocator.Persistent);
    }

    private void OnEnable()
    {
        if (showAnimationJobDebug)
            Debug.Log($"[HorseFootPoseReader Job:{name}#{GetInstanceID()}] OnEnable, " +
                      $"active={isActiveAndEnabled}, dataValid={IsValid()}, " +
                      $"bindings=[frontLeft={(data.GetFoot(0) ? data.GetFoot(0).name : "null")}, " +
                      $"frontRight={(data.GetFoot(1) ? data.GetFoot(1).name : "null")}, " +
                      $"rear1={(data.LeftRearLeg1 ? data.LeftRearLeg1.name : "null")}, " +
                      $"rear2={(data.LeftRearLeg2 ? data.LeftRearLeg2.name : "null")}, " +
                      $"rear3={(data.GetFoot(2) ? data.GetFoot(2).name : "null")}, " +
                      $"foot={(data.LeftRearFoot ? data.LeftRearFoot.name : "null")}, " +
                      $"ikTarget={(data.LeftRearIKTarget ? data.LeftRearIKTarget.name : "null")}, " +
                      $"rearRight={(data.GetFoot(3) ? data.GetFoot(3).name : "null")}, " +
                      $"probe={(data.LeftRearGroundProbe ? data.LeftRearGroundProbe.name : "null")}]", this);
    }

    private void LateUpdate()
    {
        if (output.IsCreated)
        {
            for (int i = 0; i < sampledPositions.Length; i++)
                sampledPositions[i] = output[i];
        }

        if (!showAnimationJobDebug || Time.unscaledTime < nextPoseDiagnosticTime) return;
        nextPoseDiagnosticTime = Time.unscaledTime + 0.5f;

        int calls = processCallCount.IsCreated ? processCallCount[0] : -1;
        string poseInfo = debugSnapshots.IsCreated
            ? $"Leg1 {FormatPose(0, data.LeftRearLeg1)}, Leg2 {FormatPose(1, data.LeftRearLeg2)}, " +
              $"Leg3 {FormatPose(2, data.GetFoot(2))}, Foot {FormatPose(3, data.LeftRearFoot)}"
            : "poseSnapshots=<not initialized>";

        Debug.Log($"[HorseFootPoseReader Job:{name}#{GetInstanceID()}] LateUpdate=running, " +
                  $"active={isActiveAndEnabled}, outputCreated={output.IsCreated}, " +
                  $"dataValid={IsValid()}, rigBuilder={GetRigBuilderState()}, " +
                  $"debugSnapshotsCreated={debugSnapshots.IsCreated}, calls={calls}, " +
                  $"ikTargetStream={(output.IsCreated ? output[5].ToString("F3") : "n/a")}, " +
                  $"ikTargetScene={(data.LeftRearIKTarget ? data.LeftRearIKTarget.position.ToString("F3") : "missing")}, " +
                  $"{poseInfo}", this);
    }

    private string GetRigBuilderState()
    {
        Rig rig = GetComponent<Rig>();
        RigBuilder builder = GetComponentInParent<RigBuilder>();
        if (!rig) return "Rig=<missing>";
        if (!builder) return "RigBuilder=<missing>";

        foreach (RigLayer layer in builder.layers)
        {
            if (layer.rig != rig) continue;

            int constraintCount = layer.constraints != null ? layer.constraints.Length : 0;
            return $"graphValid={builder.graph.IsValid()}, layerFound=True, active={layer.active}, " +
                   $"initialized={layer.isInitialized}, constraints={constraintCount}";
        }

        return $"graphValid={builder.graph.IsValid()}, layerFound=False, layers={builder.layers.Count}";
    }

    private string FormatPose(int index, Transform bone)
    {
        HorseFootPoseDebugSnapshot pose = debugSnapshots[index];
        if (!bone) return "missing-transform";

        float positionDelta = Vector3.Distance(pose.afterPosition, bone.localPosition);
        float rotationDelta = Quaternion.Angle(pose.afterRotation, bone.localRotation);
        return $"valid={pose.handleValid}, anim=({pose.beforePosition:F3},{pose.beforeRotation.eulerAngles:F1}), " +
               $"job=({pose.afterPosition:F3},{pose.afterRotation.eulerAngles:F1}), " +
               $"scene=({bone.localPosition:F3},{bone.localRotation.eulerAngles:F1}), " +
               $"delta=({positionDelta:F3}m,{rotationDelta:F1}deg)";
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        if (Application.isPlaying && Time.unscaledTime >= nextGizmoDiagnosticTime)
        {
            nextGizmoDiagnosticTime = Time.unscaledTime + 1f;
            
            if (turnOnDebug) Debug.Log($"[HorseFootPoseReader Gizmo:{name}#{GetInstanceID()}] source=sampledPositions, outputCreated={output.IsCreated}, " +
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
