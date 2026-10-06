using UnityEditor;
using UnityEngine;

public static class PlatformerUIMenu
{
    [MenuItem("GameObject/Platformer/Create Game UI", false, 10)]
    private static void CreateGameUI()
    {
        CoinUI coinUI = Object.FindAnyObjectByType<CoinUI>();
        GameHUD hud = Object.FindAnyObjectByType<GameHUD>();

        GameObject root;
        if (coinUI != null)
            root = coinUI.gameObject;
        else if (hud != null)
            root = hud.gameObject;
        else
        {
            root = new GameObject("UI");
            Undo.RegisterCreatedObjectUndo(root, "Create Game UI");
        }

        if (coinUI == null)
            coinUI = Undo.AddComponent<CoinUI>(root);
        if (hud == null)
            hud = Undo.AddComponent<GameHUD>(root);

        if (!coinUI.HasUI)
        {
            Undo.RecordObject(coinUI, "Create Game UI");
            Undo.RegisterCreatedObjectUndo(coinUI.BuildUI(), "Create Game UI");
            EditorUtility.SetDirty(coinUI);
        }

        if (!hud.HasUI)
        {
            Undo.RecordObject(hud, "Create Game UI");
            Undo.RegisterCreatedObjectUndo(hud.BuildUI(), "Create Game UI");
            EditorUtility.SetDirty(hud);
        }

        Selection.activeGameObject = root;
    }

    [MenuItem("GameObject/Platformer/Create Game UI", true)]
    private static bool CanCreateGameUI() => !EditorApplication.isPlaying;
}
