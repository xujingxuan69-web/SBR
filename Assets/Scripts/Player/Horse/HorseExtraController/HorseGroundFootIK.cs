using System;
using UnityEngine;

/// <summary>
/// Moves Two Bone IK targets to the ground. The IK constraints do the actual
/// leg solve; this component never writes to animated bones.
/// </summary>
public sealed class HorseGroundFootIK : MonoBehaviour
{
    [Serializable]
    private struct Foot
    {
        public Transform animatedFoot;
        public Transform ikTarget;
    }

    [SerializeField] private Foot[] feet = new Foot[4];
    [SerializeField] private HorseFootPoseReader poseReader;
    [SerializeField, Range(0, 3)] private int poseIndex = 2;
    [SerializeField] private LayerMask groundMask = 1;
    [SerializeField, Min(0.01f)] private float rayStartHeight = 0.5f;
    [SerializeField, Min(0.01f)] private float rayLength = 1.5f;
    [SerializeField] private float footHeight = 0.02f;
    [SerializeField, Range(0f, 1f)] private float weight = 1f;
    [SerializeField] private bool drawRays = true;

    private void LateUpdate()
    {
        if (feet == null) return;

        for (int i = 0; i < feet.Length; i++)
        {
            Foot foot = feet[i];
            if (!foot.animatedFoot || !foot.ikTarget) continue;

            // Read the animation pose before Two Bone IK. Using animatedFoot.position
            // here can read the previous IK result and create a feedback loop.
            Vector3 fallback = poseReader
                ? poseReader.GetSampledPosition(poseIndex)
                : foot.animatedFoot.position;
            Vector3 origin = fallback + transform.up * rayStartHeight;

            if (Physics.Raycast(origin, -transform.up, out RaycastHit hit, rayLength, groundMask,
                    QueryTriggerInteraction.Ignore))
            {
                Vector3 grounded = hit.point + hit.normal * footHeight;
                foot.ikTarget.position = Vector3.Lerp(fallback, grounded, weight);
                foot.ikTarget.rotation = Quaternion.FromToRotation(transform.up, hit.normal)
                                         * foot.animatedFoot.rotation;
            }
            else
            {
                foot.ikTarget.position = fallback;
                foot.ikTarget.rotation = foot.animatedFoot.rotation;
            }
        }
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
            Vector3 origin = position + transform.up * rayStartHeight;
            Gizmos.DrawLine(origin, origin - transform.up * rayLength);
        }
    }
}
