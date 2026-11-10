using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

[System.Serializable]
public struct HorseFootPoseReaderData : IAnimationJobData
{
    [SerializeField] private Transform leg1;
    [SerializeField] private Transform leg2;
    [SerializeField] private Transform leg3;
    [SerializeField] private Transform foot;
    [SerializeField] private Transform ikTarget;

    public Transform Leg1 => leg1;
    public Transform Leg2 => leg2;
    public Transform Leg3 => leg3;
    public Transform Foot => foot;
    public Transform IKTarget => ikTarget;

    bool IAnimationJobData.IsValid() => leg1 && leg2 && leg3 && foot && ikTarget;

    void IAnimationJobData.SetDefaultValues()
    {
        leg1 = leg2 = leg3 = foot = ikTarget = null;
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

public struct HorseFootPoseReaderJob : IWeightedAnimationJob
{
    public TransformStreamHandle leg1;
    public TransformStreamHandle leg2;
    public TransformStreamHandle leg3;
    public TransformStreamHandle foot;
    public TransformStreamHandle ikTarget;
    public NativeArray<Vector3> positions;
    public NativeArray<HorseFootPoseDebugSnapshot> debugSnapshots;
    public NativeArray<int> processCallCount;
    public FloatProperty jobWeight { get; set; }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        processCallCount[0]++;

        positions[0] = leg3.GetPosition(stream);
        positions[1] = foot.GetPosition(stream);

        if (ikTarget.IsValid(stream))
        {
            float weight = jobWeight.Get(stream);
            Vector3 targetPosition = ikTarget.GetPosition(stream);
            ikTarget.SetPosition(stream, Vector3.Lerp(targetPosition, positions[0], weight));
            positions[2] = ikTarget.GetPosition(stream);
        }

        debugSnapshots[0] = ReadAnimatedPose(stream, leg1);
        debugSnapshots[1] = ReadAnimatedPose(stream, leg2);
        debugSnapshots[2] = ReadAnimatedPose(stream, leg3);
        debugSnapshots[3] = ReadAnimatedPose(stream, foot);
    }

    private static HorseFootPoseDebugSnapshot ReadAnimatedPose(AnimationStream stream, TransformStreamHandle bone)
    {
        if (!bone.IsValid(stream))
            return new HorseFootPoseDebugSnapshot { handleValid = 0 };

        Vector3 position = bone.GetLocalPosition(stream);
        Quaternion rotation = bone.GetLocalRotation(stream);
        return new HorseFootPoseDebugSnapshot
        {
            beforePosition = position,
            beforeRotation = rotation,
            afterPosition = position,
            afterRotation = rotation,
            handleValid = 1
        };
    }
}

public class HorseFootPoseReaderJobBinder : AnimationJobBinder<HorseFootPoseReaderJob, HorseFootPoseReaderData>
{
    public override HorseFootPoseReaderJob Create(Animator animator, ref HorseFootPoseReaderData data, Component component)
    {
        var reader = (HorseFootPoseReader)component;
        reader.InitializeOutput();

        return new HorseFootPoseReaderJob
        {
            leg1 = animator.BindStreamTransform(data.Leg1),
            leg2 = animator.BindStreamTransform(data.Leg2),
            leg3 = animator.BindStreamTransform(data.Leg3),
            foot = animator.BindStreamTransform(data.Foot),
            ikTarget = animator.BindStreamTransform(data.IKTarget),
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
public class HorseFootPoseReader : RigConstraint<HorseFootPoseReaderJob, HorseFootPoseReaderData, HorseFootPoseReaderJobBinder>
{
    private NativeArray<Vector3> output;
    private NativeArray<HorseFootPoseDebugSnapshot> debugSnapshots;
    private NativeArray<int> processCallCount;
    private readonly Vector3[] sampledPositions = new Vector3[2];
    private bool hasSampledPose;
    private float nextSampleDiagnosticTime;
    private float nextPoseDiagnosticTime;
    private HorseGroundFootIK debugController;

    internal NativeArray<Vector3> Output => output;
    internal NativeArray<HorseFootPoseDebugSnapshot> DebugSnapshots => debugSnapshots;
    internal NativeArray<int> ProcessCallCount => processCallCount;

    public Transform Leg3Transform => data.Leg3;
    public Transform FootTransform => data.Foot;
    public Transform IKTarget => data.IKTarget;
    public bool HasSampledPose => hasSampledPose;
    public Vector3 SampledLeg3Position => sampledPositions[0];
    public Vector3 SampledFootPosition => sampledPositions[1];
    public Vector3 AnimationJobIKTargetPosition => output.IsCreated ? output[2] : Vector3.zero;

    private HorseGroundFootIK DebugController
    {
        get
        {
            if (!debugController)
                debugController = GetComponentInParent<HorseGroundFootIK>();
            return debugController;
        }
    }

    private bool DebugOnPoseReader => DebugController && DebugController.DebugOnPoseReader;
    private bool GizmosOnPoseReader => DebugController && DebugController.GizmosOnPoseReader;

    internal void InitializeOutput()
    {
        if (output.IsCreated && output.Length != 3)
            output.Dispose();
        if (!output.IsCreated)
            output = new NativeArray<Vector3>(3, Allocator.Persistent);
        if (!debugSnapshots.IsCreated)
            debugSnapshots = new NativeArray<HorseFootPoseDebugSnapshot>(4, Allocator.Persistent);
        if (!processCallCount.IsCreated)
            processCallCount = new NativeArray<int>(1, Allocator.Persistent);
    }

    private void OnEnable()
    {
        if (DebugOnPoseReader)
            Debug.Log($"[HorseFootPoseReader:{name}#{GetInstanceID()}] OnEnable, active={isActiveAndEnabled}, " +
                      $"dataValid={IsValid()}, bindings=[leg1={(data.Leg1 ? data.Leg1.name : "null")}, " +
                      $"leg2={(data.Leg2 ? data.Leg2.name : "null")}, leg3={(data.Leg3 ? data.Leg3.name : "null")}, " +
                      $"foot={(data.Foot ? data.Foot.name : "null")}, " +
                      $"ikTarget={(data.IKTarget ? data.IKTarget.name : "null")}]", this);
    }

    private void LateUpdate()
    {
        if (output.IsCreated)
        {
            sampledPositions[0] = output[0];
            sampledPositions[1] = output[1];
            hasSampledPose = true;
        }

        if (DebugOnPoseReader && Time.unscaledTime >= nextSampleDiagnosticTime)
        {
            nextSampleDiagnosticTime = Time.unscaledTime + 1f;
            Debug.Log($"[HorseFootPoseReader:{name}#{GetInstanceID()}] samples: " +
                      $"leg3={sampledPositions[0]:F3}, foot={sampledPositions[1]:F3}, " +
                      $"ikTarget={(output.IsCreated ? output[2].ToString("F3") : "not initialized")}", this);
        }

        if (!DebugOnPoseReader || Time.unscaledTime < nextPoseDiagnosticTime) return;
        nextPoseDiagnosticTime = Time.unscaledTime + 0.5f;

        int calls = processCallCount.IsCreated ? processCallCount[0] : -1;
        string poseInfo = debugSnapshots.IsCreated
            ? $"Leg1 {FormatPose(0, data.Leg1)}, Leg2 {FormatPose(1, data.Leg2)}, " +
              $"Leg3 {FormatPose(2, data.Leg3)}, Foot {FormatPose(3, data.Foot)}"
            : "poseSnapshots=<not initialized>";

        Debug.Log($"[HorseFootPoseReader:{name}#{GetInstanceID()}] LateUpdate=running, " +
                  $"active={isActiveAndEnabled}, outputCreated={output.IsCreated}, dataValid={IsValid()}, " +
                  $"rigBuilder={GetRigBuilderState()}, calls={calls}, " +
                  $"ikTargetStream={(output.IsCreated ? output[2].ToString("F3") : "n/a")}, " +
                  $"ikTargetScene={(data.IKTarget ? data.IKTarget.position.ToString("F3") : "missing")}, " +
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
        if (!GizmosOnPoseReader) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(Application.isPlaying ? sampledPositions[0] : data.Leg3 ? data.Leg3.position : Vector3.zero, 0.035f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(Application.isPlaying ? sampledPositions[1] : data.Foot ? data.Foot.position : Vector3.zero, 0.035f);
    }
}
