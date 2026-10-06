using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private Color healthColor = new Color(0.9f, 0.2f, 0.2f);
    [SerializeField] private Color healthBackgroundColor = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private bool showTimer = true;
    [SerializeField] private string completeTitle = "LEVEL COMPLETE!";

    [Header("UI References")]
    [Tooltip("Created by GameObject > Platformer > Create Game UI. If empty, the UI is created at runtime.")]
    [SerializeField] private GameObject healthBar;
    [SerializeField] private RectTransform healthFill;
    [SerializeField] private Text healthLabel;
    [SerializeField] private Text timerLabel;
    [SerializeField] private GameObject completePanel;
    [SerializeField] private Text completeText;

    private GameManager manager;
    private PlayerHealth playerHealth;
    private InputAction restartAction;

    public bool HasUI => healthBar != null && healthFill != null && healthLabel != null &&
                         timerLabel != null && completePanel != null && completeText != null;

    private void Awake()
    {
        if (!HasUI)
            BuildUI();

        completePanel.SetActive(false);
        timerLabel.gameObject.SetActive(showTimer);

        restartAction = new InputAction("Restart", InputActionType.Button);
        restartAction.AddBinding("<Keyboard>/enter");
        restartAction.AddBinding("<Keyboard>/numpadEnter");
        restartAction.AddBinding("<Gamepad>/start");
    }

    private void Start()
    {
        manager = GameManager.Instance;
        manager.LevelCompleted += ShowLevelComplete;

        playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.HealthChanged += UpdateHealth;
            UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
        else
        {
            healthBar.SetActive(false);
        }
    }

    private void OnEnable() => restartAction.Enable();
    private void OnDisable() => restartAction.Disable();

    private void OnDestroy()
    {
        restartAction.Dispose();
        if (manager != null)
            manager.LevelCompleted -= ShowLevelComplete;
        if (playerHealth != null)
            playerHealth.HealthChanged -= UpdateHealth;
    }

    private void Update()
    {
        if (manager == null)
            return;

        if (showTimer)
            timerLabel.text = FormatTime(manager.LevelTime);

        if (manager.IsLevelComplete && restartAction.WasPressedThisFrame())
            manager.RestartLevel();
    }

    private void UpdateHealth(int current, int max)
    {
        float fraction = max > 0 ? (float)current / max : 0f;
        healthFill.anchorMax = new Vector2(fraction, 1f);
        healthLabel.text = $"{current} / {max}";
    }

    private void ShowLevelComplete()
    {
        completeText.text =
            $"{completeTitle}\n\n" +
            $"Coins: {manager.CoinsCollected} / {manager.TotalCoins}\n" +
            $"Time: {FormatTime(manager.LevelTime)}\n\n" +
            "<size=36>Press Enter to play again</size>";
        completePanel.SetActive(true);
    }

    private static string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60f);
        return $"{minutes:00}:{seconds % 60f:00.00}";
    }

    public GameObject BuildUI()
    {
        Canvas canvas = UIFactory.CreateCanvas("HUDCanvas", transform, 10);
        Transform root = canvas.transform;
        var topLeft = new Vector2(0f, 1f);

        Image background = UIFactory.CreateImage("HealthBar", root, topLeft, new Vector2(40f, -110f),
            new Vector2(320f, 36f), healthBackgroundColor);
        healthBar = background.gameObject;

        Image fill = UIFactory.CreateImage("Fill", background.transform, Vector2.zero, Vector2.zero, Vector2.zero, healthColor);
        healthFill = fill.rectTransform;
        UIFactory.Stretch(healthFill);

        healthLabel = UIFactory.CreateText("HealthLabel", background.transform, new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(320f, 36f), 26, Color.white, TextAnchor.MiddleCenter);
        healthLabel.text = "5 / 5";

        timerLabel = UIFactory.CreateText("Timer", root, new Vector2(1f, 1f), new Vector2(-40f, -30f),
            new Vector2(400f, 80f), 48, Color.white, TextAnchor.UpperRight);
        timerLabel.text = FormatTime(0f);

        Image panel = UIFactory.CreateImage("LevelCompletePanel", root, Vector2.zero, Vector2.zero, Vector2.zero,
            new Color(0f, 0f, 0f, 0.7f));
        UIFactory.Stretch(panel.rectTransform);
        completePanel = panel.gameObject;

        completeText = UIFactory.CreateText("LevelCompleteText", panel.transform, new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(1200f, 600f), 72, Color.yellow, TextAnchor.MiddleCenter);
        completeText.text = completeTitle;
        completePanel.SetActive(false);

        return canvas.gameObject;
    }
}
