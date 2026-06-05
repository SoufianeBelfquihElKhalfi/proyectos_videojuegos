using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class emberSpawner : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Sprite emberSprite;
    [SerializeField] private RectTransform spawnArea;

    [Header("Emisión")]
    [SerializeField] private float spawnInterval = 0.15f;
    [SerializeField] private int maxEmbers = 40;

    [Header("Movimiento")]
    [SerializeField] private float riseSpeedMin = 80f;
    [SerializeField] private float riseSpeedMax = 160f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float swayAmplitude = 25f;
    [SerializeField] private float swayFrequency = 1.5f;

    [Header("Aspecto")]
    [SerializeField] private float sizeMin = 6f;
    [SerializeField] private float sizeMax = 18f;
    [SerializeField] private Color colorA = new Color(1f, 0.6f, 0.1f);
    [SerializeField] private Color colorB = new Color(1f, 0.85f, 0.3f);

    private int alive;

    private void OnEnable()
    {
        StartCoroutine(spawnLoop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();   // para el spawnLoop y todas las animaciones de brasas

        // limpia las brasas que quedaron (hijos creados por el spawner)
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        alive = 0;
    }

    private IEnumerator spawnLoop()
    {
        while (true)
        {
            if (alive < maxEmbers)
                spawnEmber();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void spawnEmber()
    {
        var go = new GameObject("ember", typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)go.transform;
        rect.SetParent(transform, false);

        var img = go.GetComponent<Image>();
        img.sprite = emberSprite;
        img.raycastTarget = false;
        img.color = Color.Lerp(colorA, colorB, Random.value);

        float size = Random.Range(sizeMin, sizeMax);
        rect.sizeDelta = new Vector2(size, size);

        float halfW = spawnArea != null ? spawnArea.rect.width * 0.5f : 400f;
        float x = Random.Range(-halfW, halfW);
        rect.anchoredPosition = new Vector2(x, 0f);

        StartCoroutine(animateEmber(rect, img));
    }

    private IEnumerator animateEmber(RectTransform rect, Image img)
    {
        alive++;
        float speed = Random.Range(riseSpeedMin, riseSpeedMax);
        float startX = rect.anchoredPosition.x;
        float phase = Random.Range(0f, Mathf.PI * 2f);
        Color baseColor = img.color;
        float t = 0f;
        while (t < lifetime)
        {
            t += Time.deltaTime;
            float n = t / lifetime;
            var p = rect.anchoredPosition;
            p.y += speed * Time.deltaTime;
            p.x = startX + Mathf.Sin(t * swayFrequency + phase) * swayAmplitude;
            rect.anchoredPosition = p;
            var c = baseColor;
            c.a = 1f - n;
            img.color = c;
            yield return null;
        }
        alive--;
        Destroy(rect.gameObject);
    }
}