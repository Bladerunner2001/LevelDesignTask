using System;
using UnityEngine;

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

    public int CoinsCollected { get; private set; }
    public int TotalCoins { get; private set; }
    public Checkpoint CurrentCheckpoint { get; private set; }

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
}
