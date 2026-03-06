using UnityEngine;

public class GhostVisualMotion : MonoBehaviour
{
    [Header("Float")]
    [SerializeField] private float hoverAmplitude = 0.12f;
    [SerializeField] private float hoverFrequency = 2.2f;

    [Header("Lateral Sway")]
    [SerializeField] private float swayAmount = 0.18f;
    [SerializeField] private float swayFrequency = 1.2f;

    [Header("Base Local Offset")]
    [SerializeField] private Vector3 baseLocalPosition = Vector3.zero;

    void LateUpdate()
    {
        float verticalOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        float lateralOffset = Mathf.Sin(Time.time * swayFrequency) * swayAmount;

        transform.localPosition = baseLocalPosition + new Vector3(lateralOffset, verticalOffset, 0f);
    }
}