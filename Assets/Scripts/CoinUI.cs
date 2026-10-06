using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour
{
    [Tooltip("Optional. If empty, a simple top-left label is created at runtime.")]
    [SerializeField] private Text label;
    [SerializeField] private string format = "Coins: {0} / {1}";
    [SerializeField] private int fontSize = 48;
    [SerializeField] private Color textColor = Color.yellow;

    private void Awake()
    {
        if (label == null)
            label = CreateLabel();
    }

    private GameManager manager;

    private void OnEnable()
    {
        manager = GameManager.Instance;
        manager.CoinsChanged += UpdateLabel;
        UpdateLabel(manager.CoinsCollected, manager.TotalCoins);
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.CoinsChanged -= UpdateLabel;
    }

    private void UpdateLabel(int collected, int total)
    {
        label.text = string.Format(format, collected, total);
    }

    private Text CreateLabel()
    {
        var canvasObject = new GameObject("CoinCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        var textObject = new GameObject("CoinLabel", typeof(RectTransform), typeof(Text), typeof(Outline));
        textObject.transform.SetParent(canvasObject.transform, false);

        var rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(40f, -30f);
        rect.sizeDelta = new Vector2(600f, 80f);

        var text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.color = textColor;
        text.alignment = TextAnchor.UpperLeft;
        text.raycastTarget = false;

        textObject.GetComponent<Outline>().effectDistance = new Vector2(2f, -2f);
        return text;
    }
}
