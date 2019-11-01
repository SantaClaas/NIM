using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NIM
{
    public class AdvancedAiPlayer : Player
    {
        private readonly Random _random;

        private Node _gameTree;

        private readonly float _difficulty;

        /// <summary>
        /// Creates a new AI Player
        /// </summary>
        /// <param name="name">The name for this player. Also used as seed for the internal randomization</param>
        /// <param name="difficulty">The difficulty level of this AI player. May range anywhere from 1 to -1, where 1 results in the best, 0 in random and -1 in the worst possible choices</param>
        public AdvancedAiPlayer(string name, float difficulty) : base(name)
        {
            _random = new Random(name.GetHashCode());

            if (difficulty > 1f)
                _difficulty = 1f;
            else if (difficulty < -1f)
                _difficulty = -1f;
            else
                _difficulty = difficulty;
        }

        /// <summary>
        /// Creates a new AI Player
        /// </summary>
        /// <param name="name">The name for this player. Also used as seed for the internal randomization</param>
        /// <param name="difficulty">The difficulty level of this AI player. May range anywhere from 1 to -1, where 1 results in the best, 0 in random and -1 in the worst possible choices</param>
        /// <param name="rules">The rules to initialize the player with. Pre-loads the decision tree</param>
        public AdvancedAiPlayer(string name, float difficulty, Rules rules) : this(name, difficulty)
        {
            _gameTree = Node.CalcGameTree(rules);
        }

        /// <summary>
        /// Creates a new AI Player
        /// </summary>
        /// <param name="name">The name for this player. Also used as seed for the internal randomization</param>
        /// <param name="difficulty">The difficulty level of this AI player. May range anywhere from 1 to -1, where 1 results in the best, 0 in random and -1 in the worst possible choices</param>
        /// <param name="teacher">An existing AI Player to copy the decision tree from</param>
        public AdvancedAiPlayer(string name, float difficulty, AdvancedAiPlayer teacher) : this(name, difficulty)
        {
            _gameTree = teacher._gameTree;
        }

        public override string ToString()
        {
            return $"AI Player {Name}, Difficulty {Math.Round(_difficulty, 3)}";
        }

        /// <inheritdoc cref="Player.DecideNextMove"/>
        public override Move DecideNextMove(Rules rules, Playground playground)
        {
            if (_gameTree is null || rules != _gameTree.Rules)
                _gameTree = Node.CalcGameTree(rules);

            List<Node> entryPoints = _gameTree.Find(n => n.Playground.Equals(playground));

            Dictionary<Move, float> chances = new Dictionary<Move, float>();

            float weight = Math.Abs(_difficulty);

            foreach (Node node in entryPoints)
            {
                foreach (KeyValuePair<Move, Node> choice in node.Children)
                {
                    float chance = choice.Value.Evaluate(node.Player, _difficulty < 0);

                    chance = (float)_random.NextDouble() * (1f - weight) + chance * weight;

                    if (!chances.ContainsKey(choice.Key))
                    {
                        chances[choice.Key] = chance;
                        continue;
                    }

                    if (chances[choice.Key] < chance)
                        chances[choice.Key] = chance;
                }
            }

            List<KeyValuePair<Move, float>> keyValuePairs = chances.OrderBy(p => p.Value).ThenBy(p => p.Key.ChangesPerRow.Sum()).ToList();
            return keyValuePairs.Last().Key;
        }
    }

    internal class Node
    {
        public Rules Rules { get; }

        public Playground Playground { get; }

        public int Player { get; }

        public ReadOnlyDictionary<Move, Node> Children { get; }

        private readonly Dictionary<Move, Node> _children;

        private Node(Rules rules, Playground playground, int player)
        {
            Rules = rules;
            Playground = playground;
            Player = player;
            _children = new Dictionary<Move, Node>();
            Children = new ReadOnlyDictionary<Move, Node>(_children);
        }

        private void CalcNextGen()
        {
            foreach (Move move in Rules.GetValidMoves(Playground))
            {
                Node child = new Node(
                    Rules,
                    Playground.ApplyMove(move),
                    (Player + 1) % Rules.PlayerCount
                    );

                child.CalcNextGen();

                _children.Add(move, child);
            }
        }

        public static Node CalcGameTree(Rules rules)
        {
            Node root = new Node(
                rules,
                rules.StartingField,
                0
            );

            root.CalcNextGen();

            return root;
        }

        private void AddIfMatch(Func<Node, bool> filter, List<Node> matches)
        {
            if (filter(this))
                matches.Add(this);

            foreach (Node child in _children.Values)
                child.AddIfMatch(filter, matches);
        }

        public List<Node> Find(Func<Node, bool> filter)
        {
            List<Node> matches = new List<Node>();
            AddIfMatch(filter, matches);
            return matches;
        }

        public float Evaluate(int player, bool invertCondition)
        {
            if (_children.Count == 0)
                return (Rules.LastMoveWins ^ invertCondition) ^ (Player + (Rules.PlayerCount - 1)) % Rules.PlayerCount != player
                    ? 1f
                    : 0f;

            List<float> childEvaluations = _children.Values.Select(n => n.Evaluate(player, invertCondition)).ToList();

            if (Player == player && childEvaluations.Contains(1f))
                return 1f;

            return childEvaluations.Average();
        }
    }
}
