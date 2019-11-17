using NIM.Shared.Models;
using System.Collections.Generic;

namespace NIM.Shared.ViewModels
{
    public class SettingsViewModel
    {
        public int RowsCount
        {
            get => Rows.Count;
            set
            {
                if (value < 1)
                    return;
                while (value > Rows.Count)
                {
                    Rows.Add(Rows.Count + 1);
                }
                while (value < Rows.Count)
                {
                    Rows.RemoveAt(Rows.Count - 1);
                }
            }
        }
        public string TakeRange { get; set; }
        public List<int> Rows { get; set; }
        public int ChangesPerRow { get => changesPerRow; set => changesPerRow = value >= 1 ? value : changesPerRow; }
        public int MinChangesPerRow { get; set; } = 1;
        public bool LastMoveWins { get; set; } = true;
        private readonly Models.GameState state;
        private int changesPerRow = 3;
        private Models.GameState GameState { get; set; }
        private Settings Settings { get; set; }
        public string Skin { get; set; }
        public SettingsViewModel(Models.GameState gameState,Settings settings)
        {
            GameState = gameState;
            Settings = settings;
            state = gameState;
            Rows = settings.Rows;
            RowsCount = settings.Rows.Count;
            LastMoveWins = settings.LastMoveWins;
        }

        public void OnSave()
        {

            var b = Rules.Build(Rows);
            b = LastMoveWins ? b.LastMoveWins() : b.LastMoveLooses();
            if (!string.IsNullOrWhiteSpace(TakeRange))
            {
                b.ParseMoveRules(TakeRange);
            }
            else
            {
                b.AddSingleRowRules(1, ChangesPerRow);
            }
            state.RulesBuilder = b;
            Settings.Skin = Skin;
        }
    }
}
