using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class HorseHeadController : MonoBehaviour
{
    [SerializeField] private GameObject headTrackingTarget;
    [SerializeField] private Horse player;
    [SerializeField] private float turnMoveDistance;
    [SerializeField] private float maxMoveDistance;
    [SerializeField] private float turnSmoothSpeed = 5f;

    private float targetTurnOffset = 0f;
    private CancellationTokenSource cts;

    private void OnEnable()
    {
        if (player == null) return;

        player.onObstacleInFront += SetHeadByMove;
        player.onObstacleClear += OnObstacleClear;
        player.onTurn += OnTurn;
    }

    private void OnDisable()
    {
        if (player == null) return;

        player.onObstacleInFront -= SetHeadByMove;
        player.onObstacleClear -= OnObstacleClear;
        player.onTurn -= OnTurn;

        cts?.Cancel();
        cts?.Dispose();
    }

    private void SetHeadByMove()
    {
        if (headTrackingTarget == null || player == null) return;

        cts?.Cancel();
        cts = new CancellationTokenSource();

        int count = player.GetObstacleInFront();
        if (count == 0)
        {
            SetHeadDefault();
            return;
        }

        Collider[] buffer = player.GetObstacleHitBuffer();
        Vector3 sphereCenter = player.GetObstacleDetectCenter();

        Collider nearest = GetNearestObstacle(buffer, count, sphereCenter);
        if (nearest == null)
        {
            SetHeadDefault();
            return;
        }

        Vector3 closestPoint = nearest.ClosestPoint(sphereCenter);
        float distance = Vector3.Distance(sphereCenter, closestPoint);

        float maxCheckDistance = player.checkSphereRadius;
        float t = 1f - Mathf.Clamp01(distance / maxCheckDistance);
        float offsetMagnitude = t * maxMoveDistance;

        Vector3 direction = (closestPoint - sphereCenter).normalized;
        float dot = Vector3.Dot(direction, player.transform.right);

        float side = -Mathf.Sign(dot);
        if (Mathf.Abs(dot) < 0.01f)
        {
            side = 1;
        }

        float targetX = Mathf.Clamp(side * offsetMagnitude, -maxMoveDistance, maxMoveDistance);

        ApplyHeadPosition(targetX);
        targetTurnOffset = 0f;
    }

    private void OnObstacleClear()
    {
        if (headTrackingTarget == null || player == null) return;

        if (Mathf.Abs(targetTurnOffset) > 0.01f)
        {
            return;
        }

        float currentX = headTrackingTarget.transform.localPosition.x;
        if (Mathf.Abs(currentX) > 0.01f)
        {
            SetHeadDefault();
        }
        else
        {
            ApplyHeadPosition(0f);
        }
    }

    private void OnTurn(float turnDirection)
    {
        if (headTrackingTarget == null || player == null) return;

        if (player.IsObstacleInFront())
        {
            return;
        }

        targetTurnOffset = Mathf.Clamp(turnDirection * turnMoveDistance, -maxMoveDistance, maxMoveDistance);

        cts?.Cancel();
        cts = new CancellationTokenSource();
        SmoothTurnTo(targetTurnOffset, cts.Token).Forget();
    }

    private async UniTask SmoothTurnTo(float targetValue, CancellationToken token)
    {
        float currentX = headTrackingTarget.transform.localPosition.x;

        while (Mathf.Abs(currentX - targetValue) > 0.01f)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            currentX = Mathf.Lerp(currentX, targetValue, Time.deltaTime * turnSmoothSpeed);
            ApplyHeadPosition(currentX);

            await UniTask.Yield();
        }

        ApplyHeadPosition(targetValue);
    }

    private void ApplyHeadPosition(float offset)
    {
        if (headTrackingTarget == null) return;

        Vector3 localPos = headTrackingTarget.transform.localPosition;
        localPos.x = offset;
        headTrackingTarget.transform.localPosition = localPos;
    }

    private void SetHeadDefault()
    {
        if (headTrackingTarget == null) return;

        cts?.Cancel();
        cts = new CancellationTokenSource();

        targetTurnOffset = 0f;
        SmoothTurnTo(0f, cts.Token).Forget();
    }

    private Collider GetNearestObstacle(Collider[] buffer, int count, Vector3 referencePos)
    {
        Collider nearest = null;
        float nearestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            if (buffer[i] == null) continue;

            Vector3 closest = buffer[i].ClosestPoint(referencePos);
            float dist = Vector3.Distance(referencePos, closest);

            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = buffer[i];
            }
        }

        return nearest;
    }
}