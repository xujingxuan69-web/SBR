using UnityEngine;

public class HorseTailController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Horse player;
    [SerializeField] private Transform[] tailBones;

    [Header("Swing Settings")]
    [SerializeField] private float verticalSpeedSwingAmplitude = 30f;
    [SerializeField] private float maxUpAngle = 30f;
    [SerializeField] private float maxDownAngle = -20f;
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private float speedThreshold = 0.1f;

    [Header("Speed to Zero")]
    [SerializeField] private float zeroSpeed = 6f;
    [SerializeField] private float zeroSmoothSpeed = 5f;

    [Header("Flutter Settings")]
    [SerializeField] private float flutterSpeed = 6f;             
    [SerializeField] private float maxFlutterAmplitude = 15f;
    [SerializeField] private float minFlutterAmplitude = 0f;

    private float[] boneOffsets;
    private Quaternion[] initialRotations;
    private float[] initialAngles;

    private float currentVerticalAngle = 0f;
    private float targetVerticalAngle = 0f;
    private float flutterTime = 0f;

    private float[] boneZeroAngles;

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
        initialAngles = new float[count];
        boneOffsets = new float[count];
        boneZeroAngles = new float[count];

        for (int i = 0; i < count; i++)
        {
            if (tailBones[i] != null)
            {
                initialRotations[i] = tailBones[i].localRotation;

                Vector3 euler = initialRotations[i].eulerAngles;
                initialAngles[i] = euler.x;
                if (initialAngles[i] > 180f) initialAngles[i] -= 360f;
            }
            boneOffsets[i] = 1f - (float)i / count * 0.6f;
            boneZeroAngles[i] = initialAngles[i];
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

        UpdateVerticalAngle();
        ApplySmoothVerticalAngle();
        ApplyTailRotation();
    }

    private void UpdateVerticalAngle()
    {
        targetVerticalAngle = CalculateVerticalAngle();
    }

    private void ApplySmoothVerticalAngle()
    {
        float speed = Mathf.Abs(player.horizontalSpeed);
        if (speed < speedThreshold && Mathf.Abs(player.verticalSpeed) < speedThreshold)
        {
            targetVerticalAngle = Mathf.Lerp(targetVerticalAngle, 0f, Time.deltaTime * smoothSpeed * 0.5f);
        }

        currentVerticalAngle = Mathf.Lerp(currentVerticalAngle, targetVerticalAngle, Time.deltaTime * smoothSpeed);
    }

    private void ApplyTailRotation()
    {
        float speed = Mathf.Abs(player.horizontalSpeed);
        float speedRatio = Mathf.Clamp01(speed / player.maxForwardSpeed);

        float zeroFactor = Mathf.Clamp01(speed / zeroSpeed);

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

        int verticalInfluenceCount = Mathf.Min(2, tailBones.Length);

        for (int i = 0; i < tailBones.Length; i++)
        {
            if (tailBones[i] == null) continue;

            float boneOffset = boneOffsets[i];

            float targetZero = Mathf.Lerp(initialAngles[i], 0f, zeroFactor);
            boneZeroAngles[i] = Mathf.Lerp(boneZeroAngles[i], targetZero, Time.deltaTime * zeroSmoothSpeed);

            float verticalAngle = 0f;
            if (i < verticalInfluenceCount)
            {
                float influenceWeight = (i == 0) ? 1f : 0.5f;
                verticalAngle = currentVerticalAngle * boneOffset * influenceWeight;
            }

            float phaseOffset = (float)i / tailBones.Length * Mathf.PI * 0.8f;
            float flutterOffset = Mathf.Sin(flutterTime + phaseOffset) * flutterAmplitude * boneOffset;

            float finalAngle = boneZeroAngles[i] + verticalAngle + flutterOffset;
            finalAngle = Mathf.Clamp(finalAngle, maxDownAngle, maxUpAngle);

            tailBones[i].localRotation = Quaternion.Euler(finalAngle, 0f, 0f);
        }
    }

    private float CalculateVerticalAngle()
    {
        if (player == null) return 0f;

        float verticalAngle = 0f;
        if (player.verticalSpeed < 0)
        {
            float fallRatio = Mathf.Clamp01(Mathf.Abs(player.verticalSpeed) / 15f);
            verticalAngle = fallRatio * verticalSpeedSwingAmplitude;
        }
        else if (player.verticalSpeed > 0)
        {
            float riseRatio = Mathf.Clamp01(player.verticalSpeed / 15f);
            verticalAngle = -riseRatio * verticalSpeedSwingAmplitude * 0.5f;
        }

        return Mathf.Clamp(verticalAngle, maxDownAngle, maxUpAngle);
    }

    public void ResetTail()
    {
        currentVerticalAngle = 0f;
        targetVerticalAngle = 0f;
        flutterTime = 0f;

        for (int i = 0; i < tailBones.Length; i++)
        {
            if (tailBones[i] != null && i < initialRotations.Length)
            {
                tailBones[i].localRotation = initialRotations[i];
                boneZeroAngles[i] = initialAngles[i];
            }
        }
    }
}