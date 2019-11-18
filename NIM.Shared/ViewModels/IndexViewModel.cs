using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using NIM.Shared.Models;

namespace NIM.Shared.ViewModels
{
    public class IndexViewModel
    {
        public List<PlayerSelectorModel> PossiblePlayers { get; set; }
        public string GetValidationCss(PlayerSelectorModel model) => IsNameUnique(model.Name) ? "was-validated" : string.Empty;
        public bool CanSubmit { get; set; } = true;

        private Models.GameState gameState;
        private NavigationManager navigationManager;

        public IndexViewModel(Models.GameState gameState, NavigationManager navigationManager)
        {
            this.gameState = gameState;
            this.navigationManager = navigationManager;
            PossiblePlayers = Enumerable.Range(1, 2).Select(i => new PlayerSelectorModel
            {
                IsAiPlayer = false,
                DifficultyAdvancedAi = 0f,
                IsNameValid = true,
            })
            .ToList();
        }
        bool IsNameUnique(string name) => PossiblePlayers.Count(p => p.Name == name) == 1;

        public void AddNew()
        {

            PossiblePlayers.Add(new PlayerSelectorModel
            {
                IsAiPlayer = false,
                DifficultyAdvancedAi = 0f,
            });
        }

        public void RemoveLatest()
        {
            if (PossiblePlayers.Count == 2) return;
            PossiblePlayers.RemoveAt(PossiblePlayers.Count - 1);
        }

        public void OnInput(ChangeEventArgs e, int index)
        {
            PossiblePlayers[index].Name = e.Value as string;
            PossiblePlayers[index].IsNameValid = IsNameUnique(e.Value as string);
            CanSubmit = !PossiblePlayers.Any(p => !p.IsNameValid);
        }

        public void OnSubmit() 
        {
            gameState.Players = PossiblePlayers.Select((p, i) => p.IsAiPlayer ? new AiPlayerMinMax(p.Name ?? $"Player {i + 1} (Computer)", p.DifficultyAdvancedAi) : new Human(p.Name ?? $"Player {i + 1}") as Player).ToList();
            Rules rules = gameState.RulesBuilder?.Players(gameState.Players.Count).Create() ?? Rules.Default;
            gameState.Game = new Game(rules, gameState.Players);
            navigationManager.NavigateTo("nim");
            // get move
            gameState.Game.Step();
        }
    }
}
