using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NIM
{
    public class AiPlayerMinMax : Player
    {
        private readonly Random _random;

        private PlayerNode _gameTree;

        private readonly float _difficulty;

        /// <summary>
        /// Creates a new AI Player
        /// </summary>
        /// <param name="name">The name for this player. Also used as seed for the internal randomization</param>
        /// <param name="difficulty">The difficulty level of this AI player. May range anywhere from 1 to -1, where 1 results in the best, 0 in random and -1 in the worst possible choices</param>
        public AiPlayerMinMax(string name, float difficulty) : base(name)
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
        public AiPlayerMinMax(string name, float difficulty, Rules rules) : this(name, difficulty)
        {
            _gameTree = PlayerNode.CalcGameTree(rules);
        }

        /// <summary>
        /// Creates a new AI Player
        /// </summary>
        /// <param name="name">The name for this player. Also used as seed for the internal randomization</param>
        /// <param name="difficulty">The difficulty level of this AI player. May range anywhere from 1 to -1, where 1 results in the best, 0 in random and -1 in the worst possible choices</param>
        /// <param name="teacher">An existing AI Player to copy the decision tree from</param>
        public AiPlayerMinMax(string name, float difficulty, AiPlayerMinMax teacher) : this(name, difficulty)
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
            if (_gameTree is null || rules != _gameTree.MoveNode.Rules)
                _gameTree = PlayerNode.CalcGameTree(rules);

            List<PlayerNode> entryPoints = _gameTree.Find(n => n.MoveNode.Playground.Equals(playground));

            Dictionary<Move, float> chances = new Dictionary<Move, float>();

            foreach (PlayerNode node in entryPoints)
            {
                foreach (KeyValuePair<Move, PlayerNode> choice in node.Children)
                {
                    float chance = choice.Value.Evaluate(node.Player, _difficulty < 0);

                    if (!chances.ContainsKey(choice.Key))
                    {
                        chances[choice.Key] = chance;
                        continue;
                    }

                    if (chances[choice.Key] < chance)
                        chances[choice.Key] = chance;
                }
            }

            float weight = Math.Abs(_difficulty);
            foreach (Move move in chances.Keys.ToList())
                chances[move] = (float)_random.NextDouble() * (1f - weight) + chances[move] * weight;

            List<KeyValuePair<Move, float>> keyValuePairs = chances.OrderBy(p => p.Value).ThenBy(p => p.Key.ChangesPerRow.Sum()).ToList();
            return keyValuePairs.Last().Key;
        }
    }

    internal class PlayerNode
    {
        public int Player { get; }

        public MoveNode MoveNode { get; }

        public ReadOnlyDictionary<Move, PlayerNode> Children { get; }

        private readonly Dictionary<Move, PlayerNode> _children;

        private PlayerNode(int player, MoveNode moveNode)
        {
            Player = player;
            MoveNode = moveNode;
            _children = new Dictionary<Move, PlayerNode>();
            Children = new ReadOnlyDictionary<Move, PlayerNode>(_children);
        }

        private void CalcNextGen()
        {
            foreach (KeyValuePair<Move, MoveNode> movePair in MoveNode.Children)
            {
                PlayerNode child = new PlayerNode(
                    (Player + 1) % MoveNode.Rules.PlayerCount,
                    movePair.Value
                    );

                _children.Add(movePair.Key, child);

                child.CalcNextGen();
            }
        }

        public static PlayerNode CalcGameTree(Rules rules)
        {
            MoveNode moveTree = MoveNode.CalcGameTree(rules);

            PlayerNode root = new PlayerNode(
                0,
                moveTree
            );

            root.CalcNextGen();

            return root;
        }

        private void AddIfMatch(Func<PlayerNode, bool> filter, List<PlayerNode> matches)
        {
            if (filter(this))
                matches.Add(this);

            foreach (PlayerNode child in _children.Values)
                child.AddIfMatch(filter, matches);
        }

        public List<PlayerNode> Find(Func<PlayerNode, bool> filter)
        {
            List<PlayerNode> matches = new List<PlayerNode>();
            AddIfMatch(filter, matches);
            return matches;
        }

        public float Evaluate(int player, bool invertCondition)
        {
            if (_children.Count == 0)
                return (MoveNode.Rules.LastMoveWins ^ invertCondition) ^ (Player + (MoveNode.Rules.PlayerCount - 1)) % MoveNode.Rules.PlayerCount != player
                    ? 1f
                    : 0f;

            List<float> childEvaluations = _children.Values.Select(n => n.Evaluate(player, invertCondition)).ToList();

            if (Player == player && childEvaluations.Contains(1f))
                return 1f;

            return childEvaluations.Average();
        }
    }

    internal class MoveNode
    {
        public Rules Rules { get; }

        public Playground Playground { get; }

        public ReadOnlyDictionary<Move, MoveNode> Children { get; }

        private readonly Dictionary<Move, MoveNode> _children;

        private MoveNode(Rules rules, Playground playground)
        {
            Rules = rules;
            Playground = playground;
            _children = new Dictionary<Move, MoveNode>();
            Children = new ReadOnlyDictionary<Move, MoveNode>(_children);
        }

        private void CalcNextGen(Dictionary<Playground, MoveNode> existingNodes)
        {
            foreach (Move move in Rules.GetValidMoves(Playground))
            {
                Playground nextPlayground = Playground.ApplyMove(move);

                if (existingNodes.ContainsKey(nextPlayground))
                {
                    _children.Add(move, existingNodes[nextPlayground]);
                    continue;
                }


                MoveNode child = new MoveNode(Rules, nextPlayground);
                _children.Add(move, child);
                existingNodes.Add(nextPlayground, child);
                child.CalcNextGen(existingNodes);
            }
        }

        public static MoveNode CalcGameTree(Rules rules)
        {
            MoveNode root = new MoveNode(
                rules,
                rules.StartingField
            );

            Dictionary<Playground, MoveNode> existingNodes = new Dictionary<Playground, MoveNode>
            {
                {root.Playground, root}
            };


            root.CalcNextGen(existingNodes);

            return root;
        }
    }

    public class GamePlan
    {
        public Rules Rules { get; }

        private readonly Dictionary<Playground, List<Tuple<int, float, float>>> _chances;

        public GamePlan(Rules rules)
        {
            Rules = rules;
            _chances = new Dictionary<Playground, List<Tuple<int, float, float>>>();
        }

        public void Generate()
        {
            Dictionary<Playground, Generation> generateTree = GenerateTree(Rules);
        }

        private static Dictionary<Playground, Generation> GenerateTree(Rules rules)
        {
            Dictionary<Playground, Generation> tree = new Dictionary<Playground, Generation>();
            Stack<Generation> generations = new Stack<Generation>();

            generations.Push(new Generation
            {
                Age = 0,
                Parent = rules.StartingField
            });

            Generation current;
            Playground next;
            Generation nextGeneration;
            while (generations.Count > 0)
            {
                current = generations.Pop();

                current.Children = new Playground[rules.ValidMoves.Count];
                for (int i = 0; i < current.Children.Length; ++i)
                {
                    next = current.Parent.ApplyMove(rules.ValidMoves[i]);

                    if (tree.TryGetValue(next, out nextGeneration))
                        current.Children[i] = nextGeneration.Parent;
                    else if (next.Rows.Any(r => r < 0))
                        current.Children[i] = null;
                    else
                    {
                        current.Children[i] = next;
                        generations.Push(new Generation
                        {
                            Age = current.Age + 1,
                            Parent = next
                        });
                    }
                }

                tree[current.Parent] = current;
            }

            return tree;
        }

        private struct Generation
        {
            public int Age;
            public Playground Parent;
            public Playground[] Children;

            public override string ToString()
            {
                return $"{Parent}->{string.Join(", ", Children.Select(c => c is null ? "X" : c.ToString()))}";
            }
        }

        private struct MoveChances
        {
            public float[] WinChances;
            public float[] LooseChances;

            public override string ToString()
            {
                return $"Win: {string.Join("|", WinChances)}; Loose: {string.Join("|", LooseChances)}";
            }
        }
    }
}
