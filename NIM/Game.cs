using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NIM
{
    public enum GameState
    {
        Initiailizing = 0,

        ChoosePlayer = 1,
        RequestPlayerResponse = 2,
        ApplyMove = 3,
        CheckConditions = 4,

        GameOver = 5
    }

    public class Game
    {
        public GameState State { get; private set; }

        public Playground CurrentPlayground { get; private set; }

        public Move LastMove { get; private set; }

        public Player CurrentPlayer => _players[_currentPlayerIndex];

        private readonly ReadOnlyCollection<Player> _players;

        private int _currentPlayerIndex;

        private readonly Rules _rules;

        public Game(Rules rules, IEnumerable<Player> players)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));

            if (players is null)
                throw new ArgumentNullException(nameof(players));

            _players = new ReadOnlyCollection<Player>(players.ToList());

            if (!_players.Any())
                throw new ArgumentException("No players", nameof(players));

            HashSet<string> usedNames = new HashSet<string>();
            foreach (string name in _players.Select(p => p.Name))
            {
                if (usedNames.Contains(name))
                    throw new Exception($"Duplicate player name \"{name}\"");

                usedNames.Add(name);
            }

            _currentPlayerIndex = _players.Count - 1;

            CurrentPlayground = _rules.StartingField;

            LastMove = null;

            State = GameState.ChoosePlayer;
        }

        private Player ChooseNextPlayer()
        {
            _currentPlayerIndex = (++_currentPlayerIndex) % _players.Count;
            return CurrentPlayer;
        }

        private Move GetPlayerResponse()
        {
            LastMove = CurrentPlayer.DecideNextMove(_rules, CurrentPlayground);
            return LastMove;
        }

        private Playground ApplyMove()
        {
            if (!_rules.IsMoveValid(LastMove, CurrentPlayground))
                throw new Exception("Invalid move!");

            CurrentPlayground = CurrentPlayground.ApplyMove(LastMove);

            LastMove = LastMove;

            return CurrentPlayground;
        }

        private bool IsGameOver()
        {
            return !_rules.GetValidMoves(CurrentPlayground).Any();
        }

        public List<Player> GetWinningPlayers()
        {
            if (State != GameState.GameOver)
                return null;

            return _rules.LastMoveWins
                ? new List<Player> { CurrentPlayer }
                : _players.Where(p => !p.Equals(CurrentPlayer)).ToList();
        }

        public bool Step()
        {
            switch (State)
            {
                case GameState.ChoosePlayer:
                    ChooseNextPlayer();
                    State = GameState.RequestPlayerResponse;
                    break;
                case GameState.RequestPlayerResponse:
                    GetPlayerResponse();
                    State = GameState.ApplyMove;
                    break;
                case GameState.ApplyMove:
                    ApplyMove();
                    State = GameState.CheckConditions;
                    break;
                case GameState.CheckConditions:
                    State = IsGameOver()
                        ? GameState.GameOver
                        : GameState.ChoosePlayer;
                    break;
                default:
                    return false;
            }

            return true;
        }

        public List<Player> Loop()
        {
            while (Step()) { }

            return GetWinningPlayers();
        }
    }
}
