using System.Collections.Generic;
using UnityEngine;

public class GhostFollower : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private Transform playerCenter;

    [Header("Follow")]
    [SerializeField] private float smoothTime = 0.24f;
    [SerializeField] private float maxSpeed = 12f;

    [Header("Snap")]
    [SerializeField] private float maxSnapDistance = 5f;

    [Header("Horizontal Delay")]
    [SerializeField] private float horizontalDelay = 0.18f;

    [Header("Avoid Crossing Player")]
    [SerializeField] private float wrapAngleThreshold = 110f;
    [SerializeField] private float aroundPlayerSpeed = 140f;
    [SerializeField] private float minDistanceFromPlayer = 1.1f;

    private Vector3 velocity;

    private struct PositionSample
    {
        public Vector3 position;
        public float time;

        public PositionSample(Vector3 position, float time)
        {
            this.position = position;
            this.time = time;
        }
    }

    private Queue<PositionSample> positionHistory = new Queue<PositionSample>();

    void Start()
    {
        if (followTarget == null) return;

        transform.position = followTarget.position;
        positionHistory.Clear();
        positionHistory.Enqueue(new PositionSample(followTarget.position, Time.time));
        velocity = Vector3.zero;
    }

    void LateUpdate()
    {
        if (followTarget == null) return;

        positionHistory.Enqueue(new PositionSample(followTarget.position, Time.time));

        while (positionHistory.Count > 1 && Time.time - positionHistory.Peek().time > horizontalDelay)
        {
            positionHistory.Dequeue();
        }

        Vector3 delayedPosition = positionHistory.Peek().position;

        Vector3 rawTargetPosition = new Vector3(
            delayedPosition.x,
            followTarget.position.y,
            delayedPosition.z
        );

        bool isWrapping = false;
        Vector3 targetPosition = rawTargetPosition;

        if (playerCenter != null)
        {
            targetPosition = GetSafeTargetPosition(rawTargetPosition, out isWrapping);
        }

        float distance = Vector3.Distance(transform.position, targetPosition);

        if (!isWrapping && distance > maxSnapDistance)
        {
            transform.position = targetPosition;
            velocity = Vector3.zero;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                smoothTime,
                maxSpeed
            );
        }
    }

    Vector3 GetSafeTargetPosition(Vector3 rawTargetPosition, out bool isWrapping)
    {
        isWrapping = false;

        if (playerCenter == null)
            return rawTargetPosition;

        Vector3 playerPos = playerCenter.position;

        Vector3 currentFromPlayer = transform.position - playerPos;
        Vector3 targetFromPlayer = rawTargetPosition - playerPos;

        currentFromPlayer.y = 0f;
        targetFromPlayer.y = 0f;

        if (targetFromPlayer.sqrMagnitude < 0.0001f)
            return rawTargetPosition;

        float targetRadius = Mathf.Max(targetFromPlayer.magnitude, minDistanceFromPlayer);

        if (currentFromPlayer.sqrMagnitude < 0.0001f)
        {
            currentFromPlayer = targetFromPlayer.normalized * targetRadius;
        }

        float angle = Vector3.Angle(currentFromPlayer, targetFromPlayer);

        if (angle < wrapAngleThreshold)
        {
            Vector3 safeOffset = targetFromPlayer.normalized * targetRadius;
            Vector3 safePosition = playerPos + safeOffset;
            safePosition.y = rawTargetPosition.y;
            return safePosition;
        }

        isWrapping = true;

        Vector3 rotatedDirection = Vector3.RotateTowards(
            currentFromPlayer.normalized,
            targetFromPlayer.normalized,
            aroundPlayerSpeed * Mathf.Deg2Rad * Time.deltaTime,
            0f
        );

        Vector3 wrappedPosition = playerPos + rotatedDirection * targetRadius;
        wrappedPosition.y = rawTargetPosition.y;

        return wrappedPosition;
    }
}