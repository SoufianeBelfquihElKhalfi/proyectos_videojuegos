using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(SistemaVida))]
public class VidaEnemigo_UI : MonoBehaviour
{
    [Header("Sprites de corazón")]
    public Sprite heartFull;   // corazón lleno   ❤
    public Sprite heartHalf;   // medio corazón   🖤 (mitad)
    public Sprite heartEmpty;  // corazón vacío   ♡

    [Header("Posición del HUD")]
    [Tooltip("Desplazamiento en unidades de mundo desde el pivote del enemigo")]
    public Vector3 offset = new Vector3(0f, 1.5f, 0f);

    [Header("Apariencia")]
    public float iconSize = 0.325f;  // era 0.25f → +30%
    public float spacing = 0.05f;
    public Color colorLleno = Color.red;
    public Color colorMitad = new Color(1f, 0.5f, 0.5f);
    public Color colorVacio = new Color(0.3f, 0.3f, 0.3f, 0.7f);

    // ── internals ──────────────────────────────────────────────────────────────
    private SistemaVida _sistemaVida;
    private Canvas _canvas;
    private Image[] _iconos;
    private Camera _cam;

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    private void Awake()
    {
        _sistemaVida = GetComponent<SistemaVida>();
    }

    private void Start()
    {
        _cam = Camera.main;
        CrearCanvas();
        _sistemaVida.alCambiarVida.AddListener(ActualizarCorazones);
        ActualizarCorazones(_sistemaVida.VidaActual, _sistemaVida.VidaMaxima);
    }

    private void LateUpdate()
    {
        if (_canvas == null) return;
        _canvas.transform.position = transform.position + offset;

        // Billboard: el canvas siempre mira hacia la cámara
        if (_cam != null)
            _canvas.transform.rotation = _cam.transform.rotation;
    }

    private void OnDestroy()
    {
        if (_sistemaVida != null)
            _sistemaVida.alCambiarVida.RemoveListener(ActualizarCorazones);
        if (_canvas != null)
            Destroy(_canvas.gameObject);
    }

    // ── Canvas setup ───────────────────────────────────────────────────────────

    private void CrearCanvas()
    {
        GameObject go = new GameObject($"VidaUI_{gameObject.name}");
        _canvas = go.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;
        _canvas.worldCamera = _cam;

        RectTransform rt = _canvas.GetComponent<RectTransform>();
        rt.sizeDelta = Vector2.one;

        var scaler = go.GetComponent<CanvasScaler>();
        if (scaler) Destroy(scaler);

        _canvas.sortingLayerName = "Default";
        _canvas.sortingOrder = 10;

        int numCorazones = _sistemaVida.VidaMaxima / 2;
        _iconos = new Image[numCorazones];

        float paso = iconSize + spacing;
        float ancho = numCorazones * paso - spacing;
        float startX = -ancho / 2f + iconSize / 2f;

        for (int i = 0; i < numCorazones; i++)
        {
            GameObject iconGO = new GameObject($"Corazon_{i}");
            iconGO.transform.SetParent(go.transform, false);

            Image img = iconGO.AddComponent<Image>();
            img.sprite = heartEmpty ?? CrearSpriteCirculo(Color.gray);
            img.preserveAspect = true;

            RectTransform irt = img.GetComponent<RectTransform>();
            irt.sizeDelta = Vector2.one * iconSize;
            irt.anchoredPosition = new Vector2(startX + i * paso, 0f);

            _iconos[i] = img;
        }

        go.transform.position = transform.position + offset;
    }

    // ── Lógica de actualización ────────────────────────────────────────────────

    private void ActualizarCorazones(int vidaActual, int vidaMaxima)
    {
        if (_iconos == null) return;

        int numCorazones = vidaMaxima / 2;

        for (int i = 0; i < _iconos.Length; i++)
        {
            if (i >= numCorazones)
            {
                _iconos[i].gameObject.SetActive(false);
                continue;
            }
            _iconos[i].gameObject.SetActive(true);

            int mitadesEsteCorazon = vidaActual - i * 2;

            if (mitadesEsteCorazon >= 2)
            {
                _iconos[i].sprite = heartFull ?? CrearSpriteCirculo(colorLleno);
                _iconos[i].color = colorLleno;
            }
            else if (mitadesEsteCorazon == 1)
            {
                _iconos[i].sprite = heartHalf ?? CrearSpriteCirculo(colorMitad);
                _iconos[i].color = colorMitad;
            }
            else
            {
                _iconos[i].sprite = heartEmpty ?? CrearSpriteCirculo(colorVacio);
                _iconos[i].color = colorVacio;
            }
        }
    }

    // ── Fallback: círculo de color si no hay sprite asignado ──────────────────

    private static Sprite CrearSpriteCirculo(Color color)
    {
        int res = 32;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Vector2 centro = new Vector2(res / 2f, res / 2f);
        float radio = res / 2f - 1f;

        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), centro);
                tex.SetPixel(x, y, dist <= radio ? color : Color.clear);
            }
        tex.Apply();

        return Sprite.Create(tex,
            new Rect(0, 0, res, res),
            new Vector2(0.5f, 0.5f),
            res);
    }
}