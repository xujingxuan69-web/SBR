using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class HorseBodyController : MonoBehaviour
{
    [SerializeField] private Horse player;
    [SerializeField] private Transform modelRoot;
    [SerializeField, Min(0f)] private float groundRotationRatio = 5f;
    private void LateUpdate()
    {

        if (player == null || modelRoot == null) return;

        KinematicCharacterMotor motor = player.kcc;
        if (motor == null) return;

        Quaternion currentRotation = player.shareRotation;
        float pitchAngle = 0f;
        float pitchBlend = 1f - Mathf.Exp(-groundRotationRatio * Time.deltaTime);

        Vector3 currentForward = player.transform.forward;
        //四元数*向量-根据四元数旋转向量，这里Vector3.forward是人物头部的朝向
        //所以currentForward是用于获取马匹当前的前方在世界坐标下的朝向

        Vector3 flatForward = Vector3.ProjectOnPlane(currentForward, Vector3.up).normalized;
        //把currentForward投影到平面XZ上，去除Y轴分量，保存平面分量，保证接下来的计算不会影响马匹的平面分量

        Vector3 currentRight = Vector3.Cross(Vector3.up, flatForward).normalized;
        //根据当前两个轴，获得马匹的正右轴

        if (player.IsGrounded)
        {
            Vector3 groundNormal = motor.GroundingStatus.GroundNormal; //表面的朝外法线方向

            float normalUp = Vector3.Dot(groundNormal, Vector3.up);
            float normalForward = Vector3.Dot(groundNormal, flatForward);

            Vector3 slopeForward = (normalUp * flatForward - Vector3.up * normalForward).normalized;
            //计算沿马匹方向的斜坡向上方向，因为groundNormal是法线，所以是高才是实际上的长，而长的分量应当反转

            pitchAngle = Vector3.SignedAngle(currentForward, slopeForward, currentRight);
            //以currentRight为轴，获取马匹正前方向量和斜坡向上向量之间的夹角
            //注意 这里存在正负，所以能判定正在上坡还是下坡

        }
        else
        {
            pitchAngle = Vector3.SignedAngle(currentForward, flatForward, currentRight);
            //当马不在地面时，只要让马逐渐恢复原本的角度即可
        }

        currentRotation = Quaternion.AngleAxis(pitchBlend * pitchAngle, currentRight) * currentRotation;

        Quaternion targetLocalRotation = Quaternion.Inverse(player.transform.rotation) * currentRotation;

        modelRoot.localRotation = Quaternion.Slerp(modelRoot.localRotation, targetLocalRotation, pitchBlend);
    }
}
