using System;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class LeaderboardView : IUIScreenView
{
    public VisualTreeAsset rowTemplate;

    private VisualElement root;
    private ScrollView listContainer;

    private const string LEADERBOARD_LEADERBOARDROW_ADDRESSABLE = "UI/LeaderboardRow";

    private void LoadLeaderboard()
    {
        var scores = LeaderboardService.Instance.userScores;

        foreach (LeaderboardEntry entry in scores)
        {
            GenerateRow(entry);
        }
    }
    private void GenerateRow(LeaderboardEntry entry)
    {
        var row = rowTemplate.Instantiate();

        var rankLabel = row.Q<Label>("Label_Rank");
        var trophy = row.Q<VisualElement>("Icon_Trophy");
        var nameLabel = row.Q<Label>("Label_Username");
        var scoreLabel = row.Q<Label>("Label_Score");

        rankLabel.text = (entry.Rank + 1).ToString();
        nameLabel.text = entry.PlayerName;
        scoreLabel.text = TimeFormatter.FormatTime((float)entry.Score);

        //add trophy
        switch(entry.Rank)
        {
            case 0: trophy.AddToClassList("gold"); break;
            case 1: trophy.AddToClassList("silver"); break;
            case 2: trophy.AddToClassList("bronze"); break;
            default: break;
        }

        listContainer.Add(row);
    }

    public async void Initialize(VisualElement root)
    {
        LoadingSpinner.Instance.ShowSpinner();
        try
        {
            this.root = root;
            rowTemplate = await Addressables.LoadAssetAsync<VisualTreeAsset>(LEADERBOARD_LEADERBOARDROW_ADDRESSABLE).Task;

            if (rowTemplate == null)
            {
                Debug.LogError("Leaderboard row template not loaded!");
                return;
            }

            listContainer = root.Q<ScrollView>("ScrollView_Leaderboard");
            listContainer.Clear();

            await LeaderboardService.Instance.GetScores();
            LoadLeaderboard();
        }
        finally
        {
            LoadingSpinner.Instance.HideSpinner();
        }
    }
}
