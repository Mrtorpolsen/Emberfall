using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

public class LeaderboardView : IUIScreenView
{
    public VisualTreeAsset rowTemplate;

    private ScrollView listContainer;

    private Button btnEasy;
    private Button btnMedium;
    private Button btnHard;
    private Dictionary<DifficultyLevel, Button> difficultyButtons;

    private const string LEADERBOARD_LEADERBOARDROW_ADDRESSABLE = "UI/LeaderboardRow";
    private const string SCROLLVIEW_LEADERBOARD = "ScrollView_Leaderboard";
    public const string BTN_EASY = "Btn_Easy_Tab";
    public const string BTN_MEDIUM = "Btn_Medium_Tab";
    public const string BTN_HARD = "Btn_Hard_Tab";

    public async Task InitializeAsync(VisualElement root)
    {
        rowTemplate = await Addressables.LoadAssetAsync<VisualTreeAsset>(LEADERBOARD_LEADERBOARDROW_ADDRESSABLE).Task;

        if (rowTemplate == null)
        {
            Debug.LogError("Leaderboard row template not loaded!");
            return;
        }

        listContainer = UtilityUIBinding.QRequired<ScrollView>(root, SCROLLVIEW_LEADERBOARD);
        btnEasy = UtilityUIBinding.QRequired<Button>(root, BTN_EASY);
        btnMedium = UtilityUIBinding.QRequired<Button>(root, BTN_MEDIUM);
        btnHard = UtilityUIBinding.QRequired<Button>(root, BTN_HARD);

        difficultyButtons = new Dictionary<DifficultyLevel, Button>
        {
            { DifficultyLevel.Easy, btnEasy },
            { DifficultyLevel.Medium, btnMedium },
            { DifficultyLevel.Hard, btnHard }
        };
    }

    public void RenderLeaderboard(List<LeaderboardEntry> scores)
    {
        foreach (LeaderboardEntry entry in scores)
        {
            RenderRow(entry);
        }
    }

    private void RenderRow(LeaderboardEntry entry)
    {
        var row = rowTemplate.Instantiate();

        var rankLabel = UtilityUIBinding.QRequired<Label>(row, "Label_Rank");
        var trophy = UtilityUIBinding.QRequired<VisualElement>(row, "Icon_Trophy");
        var nameLabel = UtilityUIBinding.QRequired<Label>(row, "Label_Username");
        var scoreLabel = UtilityUIBinding.QRequired<Label>(row, "Label_Score");

        rankLabel.text = (entry.Rank + 1).ToString();
        nameLabel.text = entry.PlayerName;
        scoreLabel.text = TimeFormatter.FormatTimeMiliseconds((float)entry.Score);

        //add trophy
        switch (entry.Rank)
        {
            case 0: trophy.AddToClassList("gold"); break;
            case 1: trophy.AddToClassList("silver"); break;
            case 2: trophy.AddToClassList("bronze"); break;
            default: break;
        }

        listContainer.Add(row);
    }

    public void ClearLeaderboard()
    {
        listContainer.Clear();
    }

    public void SetActiveLeaderboardBtn(DifficultyLevel difficulty)
    {
        foreach (var kvp in difficultyButtons)
        {
            if (kvp.Key == difficulty)
            {
                kvp.Value.AddToClassList("active");
            }
            else
            {
                kvp.Value.RemoveFromClassList("active");
            }
        }
    }
}
