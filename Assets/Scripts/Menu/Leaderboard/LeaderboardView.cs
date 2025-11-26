using System;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class LeaderboardView : MonoBehaviour, IUIScreen
{
    public UIDocument uiDocument;
    public VisualTreeAsset rowTemplate;

    private ScrollView listContainer;

    private void LoadLeaderboard()
    {
        // Example data
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

        listContainer.Add(row);
    }

    public void Initialize()
    {
        listContainer = uiDocument.rootVisualElement.Q<ScrollView>("ScrollView_Leaderboard");
        listContainer.Clear();

        LoadLeaderboard();
    }
}
