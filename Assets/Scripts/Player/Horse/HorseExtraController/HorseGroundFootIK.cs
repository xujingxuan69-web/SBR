using System;
using UnityEngine;

/// <summary>
/// Moves Two Bone IK targets to the ground. The IK constraints do the actual
/// leg solve; this component never writes to animated bones.
/// </summary>
[DefaultExecutionOrder(10000)]
public sealed class HorseGroundFootIK : MonoBehaviour
{
    [Serializable]
    private struct Foot
    {
        // The transform used as the Two Bone IK tip.
        public Transform animatedFoot;
        // The animation-space point used for ground detection (e.g. the heel).
        public Transform groundProbe;
        public Transform ikTarget;
    }

    [SerializeField] private Foot[] feet = new Foot[4];
    [SerializeField] private HorseFootPoseReader poseReader;
    [SerializeField, Range(0, 3)] private int poseIndex = 2;
    [SerializeField] private LayerMask groundMask = 1;
    [SerializeField, Min(0.01f)] private float rayStartHeight = 0.5f;
    [SerializeField, Min(0.01f)] private float rayLength = 1.5f;
    [SerializeField, Min(0.01f)] private float maxGroundDistance = 0.1f;
    [SerializeField] private float footHeight = 0.02f;
    [SerializeField, Range(0f, 1f)] private float weight = 1f;
    [SerializeField] private bool drawRays = true;
    private float nextDiagnosticTime;

    private void LateUpdate()
    {
        if (feet == null) return;

        for (int i = 0; i < feet.Length; i++)
        {
            Foot foot = feet[i];
            if (!foot.animatedFoot || !foot.ikTarget)
            {
                LogDiagnostic($"foot[{i}] missing reference: animatedFoot={foot.animatedFoot}, target={foot.ikTarget}");
                continue;
            }

            // Read the animation pose before Two Bone IK. Using animatedFoot.position
            // here can read the previous IK result and create a feedback loop.
            Vector3 fallback = poseReader
                ? poseReader.GetSampledPosition(poseIndex)
                : foot.animatedFoot.position;
            if (poseReader && fallback == Vector3.zero && foot.animatedFoot.position != Vector3.zero)
                fallback = foot.animatedFoot.position;
            Vector3 probePosition = poseReader && i == 0
                ? poseReader.GetSampledLeftRearGroundProbePosition()
                : foot.groundProbe ? foot.groundProbe.position : fallback;
            if (probePosition == Vector3.zero && foot.groundProbe && foot.groundProbe.position != Vector3.zero)
                probePosition = foot.groundProbe.position;
            Vector3 origin = probePosition + transform.up * rayStartHeight;

            bool hitGround = Physics.Raycast(origin, -transform.up, out RaycastHit hit, rayLength, groundMask,
                QueryTriggerInteraction.Ignore);
            float groundDistance = hitGround ? Vector3.Distance(probePosition, hit.point) : float.PositiveInfinity;
            Vector3 targetAtGround = fallback;
            string groundDecision = "miss";
            if (hitGround)
            {
                targetAtGround = hit.point + hit.normal * footHeight + (fallback - probePosition);
                groundDecision = groundDistance <= maxGroundDistance ? "apply" : "too-far";
            }

            if (hitGround && groundDistance <= maxGroundDistance)
            {
                foot.ikTarget.position = Vector3.Lerp(fallback, targetAtGround, weight);
                foot.ikTarget.rotation = Quaternion.FromToRotation(transform.up, hit.normal)
                                         * foot.animatedFoot.rotation;
            }
            else
            {
                foot.ikTarget.position = fallback;
                foot.ikTarget.rotation = foot.animatedFoot.rotation;
            }

            if (i == 0)
            {
                Vector3 cyanPosition = poseReader ? poseReader.GetSampledPosition(poseIndex) : Vector3.zero;
                LogDiagnostic($"reader={(poseReader ? $"{poseReader.name}#{poseReader.GetInstanceID()}" : "<null>")}, index={poseIndex}, " +
                              $"cyan={cyanPosition:F3}, animatedFoot={foot.animatedFoot.position:F3}, sample={fallback:F3}, " +
                              $"probe={probePosition:F3}, rayOrigin={origin:F3}, rayDir={-transform.up:F3}, " +
                              $"hit={hitGround}, hitPoint={(hitGround ? hit.point.ToString("F3") : "n/a")}, " +
                              $"normal={(hitGround ? hit.normal.ToString("F3") : "n/a")}, distance={groundDistance:F3}, " +
                              $"threshold={maxGroundDistance:F3}, decision={groundDecision}, " +
                              $"groundTarget={targetAtGround:F3}, target={foot.ikTarget.position:F3}, " +
                              $"correction={(foot.ikTarget.position - fallback):F3}, weight={weight:F2}, mask={groundMask.value}");
            }
        }
    }

    private void LogDiagnostic(string message)
    {
        if (Time.unscaledTime < nextDiagnosticTime) return;
        nextDiagnosticTime = Time.unscaledTime + 1f;
        Debug.Log($"[HorseGroundFootIK:{name}] enabled={enabled}, active={gameObject.activeInHierarchy}, {message}", this);
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawRays || feet == null) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < feet.Length; i++)
        {
            if (!feet[i].animatedFoot) continue;

            Vector3 position = poseReader
                ? poseReader.GetSampledPosition(poseIndex)
                : feet[i].animatedFoot.position;
            if (poseReader && position == Vector3.zero && feet[i].animatedFoot.position != Vector3.zero)
                position = feet[i].animatedFoot.position;
            Vector3 probePosition = poseReader && i == 0
                ? poseReader.GetSampledLeftRearGroundProbePosition()
                : feet[i].groundProbe ? feet[i].groundProbe.position : position;
            if (probePosition == Vector3.zero && feet[i].groundProbe && feet[i].groundProbe.position != Vector3.zero)
                probePosition = feet[i].groundProbe.position;
            Vector3 origin = probePosition + transform.up * rayStartHeight;
            Gizmos.DrawLine(origin, origin - transform.up * rayLength);
        }
    }
}
