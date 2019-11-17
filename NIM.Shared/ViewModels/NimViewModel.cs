using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NIM.Shared.ViewModels
{
    public class NimViewModel
    {
        public int[] CurrentMove { get; private set; }
        public List<Player> Players => gameState.Players;
        public int[] Field { get; private set; }
        public Dictionary<Player, int> SummaryTakes { get; private set; }
        public bool ExitOnInitialized { get => exitOnInitialized; private set { if (exitOnInitialized = value) Notify.Invoke(); } }
        public string Skin => settings.Skin;
        public string CurrentPlayerName => gameState?.Game?.CurrentPlayer?.Name ?? string.Empty;

        public event Func<Task> Notify;

        private readonly Models.GameState gameState;
        private readonly Models.Settings settings;
        private bool exitOnInitialized;

        public NimViewModel(Models.GameState gameState, Models.Settings settings)
        {
            this.gameState = gameState;
            this.settings = settings;
            // if game is not initialized leave
            ExitOnInitialized = !gameState.IsInitialized;
            SummaryTakes = gameState.Players.ToDictionary(p => p, p => 0);
            Field = gameState.Game.CurrentPlayground.Rows.ToArray();
            CurrentMove = new int[Field.Length];
        }

        public bool CanTake(int row)
        {
            int[] futureMove = CurrentMove.Clone() as int[];
            ++futureMove[row];

            return gameState.Rules
                .IsMoveValid(new Move(futureMove), gameState.Game.CurrentPlayground);
        }

        public bool CanAdd(int row)
        {
            int[] futureMove = CurrentMove.Clone() as int[];
            --futureMove[row];

            return gameState.Rules
                .IsMoveValid(new Move(futureMove), gameState.Game.CurrentPlayground)
                || (CurrentMove.Any(i => i > 0) && !futureMove.Any(i => i > 0));
        }

        public async Task TakeAsync(int row)
        {
            //if (!CanTake(row))
            //    return;
            --Field[row];
            if (gameState.Game.CurrentPlayer is Models.Human)
                ++CurrentMove[row];
            ++SummaryTakes[gameState.Game.CurrentPlayer];
            await Notify?.Invoke();
        }
        public async Task AddAsync(int row)
        {
            //if (!CanAdd(row))
            //    return;
            ++Field[row];
            --CurrentMove[row];
            --SummaryTakes[gameState.Game.CurrentPlayer];
            await Notify?.Invoke();
        }

        public bool CanEnd => gameState.Rules.IsMoveValid(new Move(CurrentMove), gameState.Game.CurrentPlayground);

        public async Task EndTurn()
        {

            if (gameState.Game.CurrentPlayer is Models.Human human)
            {
                if (!CanEnd) return;
                human.NextMove = CurrentMove;
            }
            // get move
            ExitOnInitialized = !gameState.Game.Step();
            // apply move
            ExitOnInitialized = !gameState.Game.Step();

            if (gameState.Game.CurrentPlayer is AiPlayerMinMax)
            {
                await ReplayAiMove();
                //SummaryTakes[gameState.Game.CurrentPlayer] += gameState.Game.LastMove.ChangesPerRow.Sum();
            }

            // check conditions and if it returns false game is over and navigate to winning page
            ExitOnInitialized = !gameState.Game.Step();

            CurrentMove = new int[gameState.Game.CurrentPlayground.Rows.Count];
            // choose player
            ExitOnInitialized = !gameState.Game.Step();

            await Reset();
        }

        async Task Reset()
        {

            if (ExitOnInitialized = gameState == null || gameState.Game == null || gameState.Game.State == GameState.GameOver)
            {
                await Notify.Invoke();
                return;
            }
            CurrentMove = new int[Field.Length];
            //Array.Copy(gameState.Game.CurrentPlayground.Rows, State);
            if (gameState.Game.CurrentPlayer is AiPlayerMinMax)
                await EndTurn();
        }

        async Task ReplayAiMove()
        {
            // was last player ai
            if (!(gameState.Game.CurrentPlayer is AiPlayerMinMax))
                return;
            for (int i = 0; i < gameState.Game.LastMove.ChangesPerRow.Count; ++i)
            {
                for (int j = 0; j < gameState.Game.LastMove.ChangesPerRow[i]; ++j)
                {
                    await TakeAsync(i);
                    await Task.Delay(1500);
                }
            }
        }
    }
}