using UnityEngine;

[DefaultExecutionOrder(10000)]  //延后执行，保证不出错
public sealed class HorseGroundFootIK : MonoBehaviour
{
    [SerializeField] private HorseFootPoseReader[] feet = new HorseFootPoseReader[4];
    [SerializeField] private LayerMask groundMask = 1;
    [SerializeField, Min(0.01f)] private float rayStartHeight = 0.5f;
    [SerializeField, Min(0.01f)] private float rayLength = 1.5f;
    [SerializeField, Min(0.01f)] private float maxGroundDistance = 0.1f;
    [SerializeField] private float footHeight = 0.02f;
    [SerializeField, Range(0f, 1f)] private float weight = 1f;
    [SerializeField] private bool debugOnPoseReader;
    [SerializeField] private bool debugOnFootIK;
    [SerializeField] private bool debugOnFootIKTarget = true;
    [SerializeField] private bool gizmosOnPoseReader = true;
    [SerializeField] private bool gizmosOnFootIK = true;

    public bool DebugOnPoseReader => debugOnPoseReader;
    public bool GizmosOnPoseReader => gizmosOnPoseReader;

    private float nextDiagnosticTime;
    private float nextTargetDiagnosticTime;

    private void LateUpdate()
    {
        if (feet == null) return;

        for (int i = 0; i < feet.Length; i++)
        {
            HorseFootPoseReader foot = feet[i];
            if (!foot || !foot.Leg3Transform || !foot.FootTransform || !foot.IKTarget)
            {
                LogDiagnostic($"foot[{i}] reader or bone reference is missing");
                continue;
            }

            Vector3 fallback = foot.HasSampledPose
                ? foot.SampledLeg3Position
                : foot.Leg3Transform.position;
            Vector3 probePosition = foot.HasSampledPose
                ? foot.SampledFootPosition
                : foot.FootTransform.position;
            Vector3 origin = probePosition + transform.up * rayStartHeight;

            bool hitGround = Physics.Raycast(origin, -transform.up, out RaycastHit hit, rayLength, groundMask,
                QueryTriggerInteraction.Ignore);
            float groundDistance = hitGround ? Vector3.Distance(probePosition, hit.point) : float.PositiveInfinity;
            Vector3 targetAtGround = fallback;
            string groundDecision = "miss";
            if (hitGround)
            {
                targetAtGround = hit.point + hit.normal * footHeight + (fallback - probePosition);
                groundDecision = groundDistance <= maxGroundDistance ? "would-apply-disabled" : "too-far";
            }

            // Ground correction remains disabled during the current isolation test.
            /*
            if (hitGround && groundDistance <= maxGroundDistance)
            {
                foot.IKTarget.position = Vector3.Lerp(fallback, targetAtGround, weight);
                foot.IKTarget.rotation = Quaternion.FromToRotation(transform.up, hit.normal)
                                         * foot.Leg3Transform.rotation;
            }
            else
            {
                foot.IKTarget.position = fallback;
                foot.IKTarget.rotation = foot.Leg3Transform.rotation;
            }
            */

            if (i == 0 && debugOnFootIKTarget && Time.unscaledTime >= nextTargetDiagnosticTime)
            {
                nextTargetDiagnosticTime = Time.unscaledTime + 0.5f;
                Debug.Log($"[HorseGroundFootIK Target:{name}] foot={foot.name}, sample={fallback:F3}, " +
                          $"jobTarget={foot.AnimationJobIKTargetPosition:F3}, target={foot.IKTarget.position:F3}, " +
                          $"hit={hitGround}, correction=disabled", this);
            }

            LogDiagnostic($"foot[{i}] reader={foot.name}, sample={fallback:F3}, probe={probePosition:F3}, " +
                          $"rayOrigin={origin:F3}, rayDir={-transform.up:F3}, hit={hitGround}, " +
                          $"hitPoint={(hitGround ? hit.point.ToString("F3") : "n/a")}, " +
                          $"distance={groundDistance:F3}, threshold={maxGroundDistance:F3}, " +
                          $"decision={groundDecision}, target={foot.IKTarget.position:F3}, " +
                          $"groundTarget={targetAtGround:F3}, weight={weight:F2}, mask={groundMask.value}");
        }
    }

    private void LogDiagnostic(string message)
    {
        if (!debugOnFootIK || Time.unscaledTime < nextDiagnosticTime) return;
        nextDiagnosticTime = Time.unscaledTime + 1f;
        Debug.Log($"[HorseGroundFootIK:{name}] enabled={enabled}, active={gameObject.activeInHierarchy}, {message}", this);
    }

    private void OnDrawGizmosSelected()
    {
        if (!gizmosOnFootIK || feet == null) return;

        Gizmos.color = Color.yellow;
        foreach (HorseFootPoseReader foot in feet)
        {
            if (!foot || !foot.FootTransform) continue;

            Vector3 probePosition = Application.isPlaying && foot.HasSampledPose
                ? foot.SampledFootPosition
                : foot.FootTransform.position;
            Vector3 origin = probePosition + transform.up * rayStartHeight;
            Gizmos.DrawLine(origin, origin - transform.up * rayLength);
        }
    }
}
