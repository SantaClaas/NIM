﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
            Dictionary<Playground, Node> tree = GenerateTree(Rules, out List<Node> leafs);

            Dictionary<Playground, MoveChances> chances = new Dictionary<Playground, MoveChances>();
            Queue<Node> nodeQueue = new Queue<Node>();
            HashSet<Node> inQueue = new HashSet<Node>();
            foreach (Node leaf in leafs)
            {
                nodeQueue.Enqueue(leaf);
                inQueue.Add(leaf);
            }

            MoveChances none = new MoveChances(Rules.PlayerCount);

            Node currentNode;
            MoveChances[] childChances = new MoveChances[Rules.ValidMoves.Count];
            Node node;
            bool canEvaluate;
            while (nodeQueue.Count > 0)
            {
                currentNode = nodeQueue.Dequeue();

                canEvaluate = true;
                for (int i = 0; i < currentNode.Children.Length; ++i)
                {
                    node = currentNode.Children[i];

                    if (node.Current is null)
                        childChances[i] = none;
                    else if (!chances.TryGetValue(node.Current, out childChances[i]))
                    {
                        canEvaluate = false;
                        break;
                    }
                }

                if (!canEvaluate)
                {
                    nodeQueue.Enqueue(currentNode);
                    continue;
                }

                chances[currentNode.Current] = none;

                for (int i = 0; i < currentNode.Parents.Length; ++i)
                {
                    node = currentNode.Parents[i];

                    if (node.Current is null)
                        continue;

                    if (inQueue.Contains(node))
                        continue;

                    nodeQueue.Enqueue(node);
                    inQueue.Add(node);
                }
            }
        }

        private static Dictionary<Playground, Node> GenerateTree(Rules rules, out List<Node> leafs)
        {
            leafs = new List<Node>();

            Dictionary<Playground, Node> tree = new Dictionary<Playground, Node>();
            Stack<Node> generations = new Stack<Node>();

            generations.Push(new Node(rules.StartingField, rules.ValidMoves.Count));

            Node none = new Node(null, rules.ValidMoves.Count);

            Node current;
            Playground next;
            Node nextGeneration;
            int childrenCount;
            while (generations.Count > 0)
            {
                current = generations.Pop();

                childrenCount = 0;
                for (int i = 0; i < current.Children.Length; ++i)
                {

                    if (current.Current.TryApplyValidMove(rules.ValidMoves[i], out next))
                    {
                        ++childrenCount;
                        if (tree.TryGetValue(next, out nextGeneration))
                        {
                            next = nextGeneration.Current;
                            nextGeneration.Parents[i] = current;
                            current.Children[i] = nextGeneration;
                        }
                        else
                        {
                            current.Children[i] = new Node(next, rules.ValidMoves.Count) { Parents = { [i] = current } };
                            generations.Push(current.Children[i]);
                        }
                    }
                    else
                    {
                        current.Children[i] = none;
                    }
                }
                if (childrenCount == 0)
                    leafs.Add(current);

                tree[current.Current] = current;
            }

            return tree;
        }

        private struct Node
        {
            public Node[] Parents;
            public Playground Current;
            public Node[] Children;

            public Node(Playground playground, int moveCount)
            {
                Current = playground;
                Parents = new Node[moveCount];
                Children = new Node[moveCount];
            }

            public override string ToString()
            {
                return $"{Current}, {Parents.Count(p => !(p.Current is null))} Parent(s), {Children.Count(c => !(c.Current is null))} Child(ren)";
            }
        }

        private struct MoveChances
        {
            public readonly float[] WinChances;
            public readonly float[] LooseChances;

            public MoveChances(int playerCount)
            {
                WinChances = new float[playerCount];
                LooseChances = new float[playerCount];
            }

            public override string ToString()
            {
                return $"Win: {string.Join("|", WinChances)}; Loose: {string.Join("|", LooseChances)}";
            }
        }
    }
}
