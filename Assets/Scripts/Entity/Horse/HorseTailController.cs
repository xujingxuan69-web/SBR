using UnityEngine;

public class HorseTailController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Horse player;
    [SerializeField] private Transform[] tailBones;

    [Header("Flutter Settings")]
    [SerializeField] private float flutterSpeed = 6f;
    [SerializeField] private float maxFlutterAmplitude = 15f;
    [SerializeField] private float minFlutterAmplitude = 0f;

    private float[] boneOffsets;
    private Quaternion[] initialRotations;
    private float flutterTime = 0f;

    private void Awake()
    {
        if (tailBones == null || tailBones.Length == 0)
        {
            Debug.LogWarning("None of TailBones");
            return;
        }

        InitBoneData();
    }

    private void InitBoneData()
    {
        int count = tailBones.Length;
        initialRotations = new Quaternion[count];
        boneOffsets = new float[count];

        for (int i = 0; i < count; i++)
        {
            if (tailBones[i] != null)
            {
                initialRotations[i] = tailBones[i].localRotation;
            }
            boneOffsets[i] = 1f - (float)i / count * 0.6f;
        }
    }

    private void OnEnable()
    {
        ResetTail();
    }

    private void OnDisable()
    {
        ResetTail();
    }

    private void LateUpdate()
    {
        if (player == null || tailBones == null || tailBones.Length == 0) return;

        float speed = Mathf.Abs(player.horizontalSpeed);
        float speedRatio = Mathf.Clamp01(speed / player.maxForwardSpeed);

        // 平方映射，让低速时几乎不抖动
        float speedRatioSquared = speedRatio * speedRatio;
        float currentFlutterSpeed = Mathf.Lerp(0f, flutterSpeed, speedRatioSquared);
        float flutterAmplitude = Mathf.Lerp(minFlutterAmplitude, maxFlutterAmplitude, speedRatioSquared);

        if (player.IsMoving)
        {
            flutterTime += Time.deltaTime * currentFlutterSpeed;
        }
        else
        {
            flutterTime = 0f;
        }

        for (int i = 0; i < tailBones.Length; i++)
        {
            if (tailBones[i] == null) continue;

            float boneOffset = boneOffsets[i];

            // 只保留抖动效果，叠加到初始旋转上
            float phaseOffset = (float)i / tailBones.Length * Mathf.PI * 0.8f;
            float flutterOffset = Mathf.Sin(flutterTime + phaseOffset) * flutterAmplitude * boneOffset;

            tailBones[i].localRotation = initialRotations[i] * Quaternion.Euler(flutterOffset, 0f, 0f);
        }
    }

    public void ResetTail()
    {
        flutterTime = 0f;

        for (int i = 0; i < tailBones.Length; i++)
        {
            if (tailBones[i] != null && i < initialRotations.Length)
            {
                tailBones[i].localRotation = initialRotations[i];
            }
        }
    }
}