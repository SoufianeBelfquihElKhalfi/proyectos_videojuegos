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

    [Header("Rotación orgánica")]
    [SerializeField] private float pitchAmount = 3f;
    [SerializeField] private float rollAmount = 5f;
    [SerializeField] private float yawAmount = 2f;
    [SerializeField] private float rotationFrequency = 0.8f;

    private Vector3 posicionInicialLocal;
    private Vector3 escalaInicialLocal;

    private float hoverPhase;
    private float swayPhase;
    private float breathePhase;

    private Quaternion rotacionInicialLocal;
    private float tiltPhase;
    private float rollPhase;
    private float yawPhase;

    private void Awake()
    {
        posicionInicialLocal = transform.localPosition;
        escalaInicialLocal = transform.localScale;

        hoverPhase = Random.Range(0f, 100f);
        swayPhase = Random.Range(0f, 100f);
        breathePhase = Random.Range(0f, 100f);

        rotacionInicialLocal = transform.localRotation;

        tiltPhase = Random.Range(0f, 100f);
        rollPhase = Random.Range(0f, 100f);
        yawPhase = Random.Range(0f, 100f);
    }

    private void LateUpdate()
    {
        float verticalOffset = Mathf.Sin((Time.time + hoverPhase) * hoverFrequency) * hoverAmplitude;
        float lateralOffset = Mathf.Sin((Time.time + swayPhase) * swayFrequency) * swayAmount;
        float breatheOffset = Mathf.Sin((Time.time + breathePhase) * breatheFrequency) * breatheAmount;

        transform.localPosition = posicionInicialLocal + new Vector3(lateralOffset, verticalOffset, 0f);
        transform.localScale = escalaInicialLocal * (1f + breatheOffset);

        float pitch = Mathf.Sin((Time.time + tiltPhase) * rotationFrequency) * pitchAmount;
        float roll = Mathf.Sin((Time.time + rollPhase) * rotationFrequency * 0.85f) * rollAmount;
        float yaw = Mathf.Sin((Time.time + yawPhase) * rotationFrequency * 0.6f) * yawAmount;

        transform.localRotation = rotacionInicialLocal * Quaternion.Euler(pitch, yaw, roll);
    }
}