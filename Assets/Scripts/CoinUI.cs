using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour
{
    [Tooltip("Created by GameObject > Platformer > Create Game UI. If empty, a label is created at runtime.")]
    [SerializeField] private Text label;
    [SerializeField] private string format = "Coins: {0} / {1}";
    [SerializeField] private int fontSize = 48;
    [SerializeField] private Color textColor = Color.yellow;

    private GameManager manager;

    public bool HasUI => label != null;

    private void Awake()
    {
        if (label == null)
            BuildUI();
    }

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

    public GameObject BuildUI()
    {
        Canvas canvas = UIFactory.CreateCanvas("CoinCanvas", transform);
        label = UIFactory.CreateText("CoinLabel", canvas.transform, new Vector2(0f, 1f), new Vector2(40f, -30f),
            new Vector2(600f, 80f), fontSize, textColor, TextAnchor.UpperLeft);
        label.text = string.Format(format, 0, 0);
        return canvas.gameObject;
    }
}
