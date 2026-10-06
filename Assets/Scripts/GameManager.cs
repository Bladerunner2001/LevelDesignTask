using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<GameManager>();
                if (instance == null)
                    instance = new GameObject("GameManager").AddComponent<GameManager>();
            }
            return instance;
        }
    }

    public event Action<int, int> CoinsChanged;
    public event Action LevelCompleted;

    public int CoinsCollected { get; private set; }
    public int TotalCoins { get; private set; }
    public Checkpoint CurrentCheckpoint { get; private set; }
    public bool IsLevelComplete { get; private set; }
    public float LevelTime { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private void Update()
    {
        if (!IsLevelComplete)
            LevelTime += Time.deltaTime;
    }

    public void RegisterCoin()
    {
        TotalCoins++;
        CoinsChanged?.Invoke(CoinsCollected, TotalCoins);
    }

    public void CollectCoin(int value)
    {
        CoinsCollected += value;
        CoinsChanged?.Invoke(CoinsCollected, TotalCoins);
    }

    public void SetCheckpoint(Checkpoint checkpoint)
    {
        if (CurrentCheckpoint == checkpoint)
            return;

        if (CurrentCheckpoint != null)
            CurrentCheckpoint.SetActive(false);

        CurrentCheckpoint = checkpoint;
        CurrentCheckpoint.SetActive(true);
    }

    public void CompleteLevel()
    {
        if (IsLevelComplete)
            return;

        IsLevelComplete = true;
        LevelCompleted?.Invoke();
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
