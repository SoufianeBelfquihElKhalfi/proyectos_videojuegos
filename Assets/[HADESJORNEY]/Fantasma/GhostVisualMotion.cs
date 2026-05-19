using UnityEngine;

public class GhostVisualMotion : MonoBehaviour
{
    [Header("Flotacion")]
    [SerializeField] private float hoverAmplitude = 0.12f;
    [SerializeField] private float hoverFrequency = 2.2f;

    [Header("Balanceo lateral")]
    [SerializeField] private float swayAmount = 0.12f;
    [SerializeField] private float swayFrequency = 1.2f;

    [Header("Respiracion")]
    [SerializeField] private float breatheAmount = 0.025f;
    [SerializeField] private float breatheFrequency = 1.6f;

    private Vector3 posicionInicialLocal;
    private Vector3 escalaInicialLocal;

    private void Awake()
    {
        posicionInicialLocal = transform.localPosition;
        escalaInicialLocal = transform.localScale;
    }

    private void LateUpdate()
    {
        float verticalOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        float lateralOffset = Mathf.Sin(Time.time * swayFrequency) * swayAmount;
        float breatheOffset = Mathf.Sin(Time.time * breatheFrequency) * breatheAmount;

        transform.localPosition = posicionInicialLocal + new Vector3(lateralOffset, verticalOffset, 0f);
        transform.localScale = escalaInicialLocal * (1f + breatheOffset);
    }
}