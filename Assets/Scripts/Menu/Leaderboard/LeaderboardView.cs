using System;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class LeaderboardView : IUIScreen
{
    public VisualTreeAsset rowTemplate;

    private VisualElement root;
    private ScrollView listContainer;

    private void LoadLeaderboard()
    {
        var scores = LeaderboardManager.main.userScores;

        foreach (LeaderboardEntry entry in scores)
        {
            AddRow(entry);
        }
    }
    private void AddRow(LeaderboardEntry entry)
    {
        var row = rowTemplate.Instantiate();

        var rankLabel = row.Q<Label>("Label_Rank");
        var trophy = row.Q<VisualElement>("Icon_Trophy");
        var nameLabel = row.Q<Label>("Label_Username");
        var scoreLabel = row.Q<Label>("Label_Score");

        rankLabel.text = (entry.Rank + 1).ToString();
        nameLabel.text = entry.PlayerName;
        scoreLabel.text = Utility.FormatTime((float)entry.Score);

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

    public void Initialize(VisualElement root)
    {
        this.root = root;
        rowTemplate = Resources.Load<VisualTreeAsset>("UI/LeaderboardRow");

        listContainer = root.Q<ScrollView>("ScrollView_Leaderboard");
        listContainer.Clear();

        LoadLeaderboard();
    }
}
