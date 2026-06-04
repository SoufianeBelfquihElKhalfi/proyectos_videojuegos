using UnityEngine;

public class parallaxLayer : MonoBehaviour
{
    [SerializeField] private float strength = 20f;   // cuántos píxeles se mueve como máximo
    [SerializeField] private float smooth = 5f;      // suavidad del seguimiento

    private Vector2 basePos;
    private RectTransform rect;

    private void Awake()
    {
        rect = (RectTransform)transform;
        basePos = rect.anchoredPosition;
    }

    private void Update()
    {
        // ratón normalizado a rango -1..1 respecto al centro de la pantalla
        Vector2 mouse = Input.mousePosition;
        float nx = (mouse.x / Screen.width - 0.5f) * 2f;
        float ny = (mouse.y / Screen.height - 0.5f) * 2f;

        // sentido contrario al ratón (parallax)
        Vector2 target = basePos + new Vector2(-nx, -ny) * strength;

        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, target, Time.deltaTime * smooth);
    }
}