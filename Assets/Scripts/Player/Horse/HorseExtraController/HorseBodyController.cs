using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class HorseBodyController : MonoBehaviour
{
    [SerializeField] private Horse player;
    [SerializeField] private Transform modelRoot;
    [SerializeField, Min(0f)] private float groundRotationRatio = 5f;

    private KinematicCharacterMotor motor;
    
    private float pitchAngle = 0f;
    private Vector3 lastGroundNormal = Vector3.up;
    private bool hasGroundNormal;

    private void Start()
    {
        motor = player.kcc;
    }

    private void LateUpdate()
    {
        if (player == null || modelRoot == null || motor == null) return;

        Quaternion currentRotation = player.shareRotation;
        float pitchBlend = 1f - Mathf.Exp(-groundRotationRatio * Time.deltaTime);

        Vector3 currentForward = player.transform.forward;
        //四元数*向量-根据四元数旋转向量，这里Vector3.forward是人物头部的朝向
        //所以currentForward是用于获取马匹当前的前方在世界坐标下的朝向

        Vector3 flatForward = Vector3.ProjectOnPlane(currentForward, Vector3.up).normalized;
        //把currentForward投影到平面XZ上，去除Y轴分量，保存平面分量

        Vector3 currentRight = Vector3.Cross(Vector3.up, flatForward).normalized;
        //根据当前两个轴，获得马匹的正右轴

        float targetPitchAngle;

        var grounding = motor.GroundingStatus;
        bool foundGroundNormal = grounding.FoundAnyGround && grounding.GroundNormal.sqrMagnitude > 0.0001f;
        if (foundGroundNormal)
        {
            lastGroundNormal = grounding.GroundNormal.normalized;
            hasGroundNormal = true;
        }

        bool keepGroundPitch = foundGroundNormal ||
            ((player.HasGroundingGrace || player.IsBlockedBySteepSlope) && hasGroundNormal);
        if (keepGroundPitch)
        {
            Vector3 groundNormal = foundGroundNormal ? grounding.GroundNormal.normalized : lastGroundNormal;
            float normalUp = Vector3.Dot(groundNormal, Vector3.up);
            float normalForward = Vector3.Dot(groundNormal, flatForward);
            Vector3 slopeForward = (normalUp * flatForward - Vector3.up * normalForward).normalized;
            targetPitchAngle = Vector3.SignedAngle(currentForward, slopeForward, currentRight);
        }
        else
        {
            targetPitchAngle = Vector3.SignedAngle(currentForward, flatForward, currentRight);
        }
        pitchAngle = Mathf.LerpAngle(pitchAngle, targetPitchAngle, pitchBlend);

        currentRotation = Quaternion.AngleAxis(pitchAngle, currentRight) * currentRotation;

        player.SetBodyRotation(currentRotation);
        modelRoot.localRotation = Quaternion.Inverse(player.transform.rotation) * currentRotation;
    }
}
