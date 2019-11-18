using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NIM
{
    public class AiPlayerMinMax : Player
    {
        private readonly Random _random;

        private GamePlan _gamePlan;

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
            _gamePlan = new GamePlan(rules);
            _gamePlan.Generate();
        }

        /// <summary>
        /// Creates a new AI Player
        /// </summary>
        /// <param name="name">The name for this player. Also used as seed for the internal randomization</param>
        /// <param name="difficulty">The difficulty level of this AI player. May range anywhere from 1 to -1, where 1 results in the best, 0 in random and -1 in the worst possible choices</param>
        /// <param name="teacher">An existing AI Player to copy the decision tree from</param>
        public AiPlayerMinMax(string name, float difficulty, AiPlayerMinMax teacher) : this(name, difficulty)
        {
            _gamePlan = teacher._gamePlan;
        }

        public override string ToString()
        {
            return $"AI Player {Name}, Difficulty {Math.Round(_difficulty, 3)}";
        }

        /// <inheritdoc cref="Player.DecideNextMove"/>
        public override Move DecideNextMove(Rules rules, Playground playground)
        {
            if (_gamePlan is null || rules != _gamePlan.Rules)
            {
                _gamePlan = new GamePlan(rules);
                _gamePlan.Generate();
            }

            Dictionary<Move, float> chances = new Dictionary<Move, float>();

            List<Tuple<Move, float, float>> tuples = _gamePlan.GetChances(playground);

            float weight = Math.Abs(_difficulty);
            foreach ((Move move, float winChance, float looseChance) in tuples)
                chances[move] = (float)_random.NextDouble() * (1f - weight) + (_difficulty < 0 ? looseChance : winChance) * weight;

            List<KeyValuePair<Move, float>> keyValuePairs = chances.OrderBy(p => p.Value).ThenBy(p => p.Key.ChangesPerRow.Sum()).ToList();
            return keyValuePairs.Last().Key;
        }
    }

    public class GamePlan
    {
        public Rules Rules { get; }

        private readonly Dictionary<Playground, MoveChancesPlayground> _chances;

        public GamePlan(Rules rules)
        {
            if (rules.PlayerCount < 2)
                throw new ArgumentException("Cannot create GamePlan for less than two players");

            Rules = rules;
            _chances = new Dictionary<Playground, MoveChancesPlayground>();
        }

        public List<Tuple<Move, float, float>> GetChances(Playground current)
        {
            List<Tuple<Move, float, float>> chances = new List<Tuple<Move, float, float>>();

            if (_chances.TryGetValue(current, out MoveChancesPlayground moveChances))
            {
                for (int i = 0; i < Rules.ValidMoves.Count; ++i)
                {
                    if (moveChances.WinChances[i] < 0f)
                        continue;

                    chances.Add(new Tuple<Move, float, float>(Rules.ValidMoves[i], moveChances.WinChances[i], moveChances.LooseChances[i]));
                }
            }

            return chances;
        }

        public void Generate()
        {
            List<Node> leafs = GenerateTree(Rules);

            Dictionary<Playground, MoveChancesPlayerTimeline> chances = new Dictionary<Playground, MoveChancesPlayerTimeline>();
            Queue<Node> nodeQueue = new Queue<Node>();
            HashSet<Node> inQueue = new HashSet<Node>();
            foreach (Node leaf in leafs)
            {
                nodeQueue.Enqueue(leaf);
                inQueue.Add(leaf);
            }

            MoveChancesPlayerTimeline none = new MoveChancesPlayerTimeline(0);

            Node currentNode;
            MoveChancesPlayerTimeline[] childChances = new MoveChancesPlayerTimeline[Rules.ValidMoves.Count];
            Node node;
            bool canEvaluate;
            int childPlayerIndex;
            MoveChancesPlayerTimeline currentChances;
            int childCount;
            float tempChance;
            bool guaranteedWin;
            bool guaranteedLoss;
            MoveChancesPlayground moveChancesPlayground;
            while (nodeQueue.Count > 0)
            {
                currentNode = nodeQueue.Dequeue();

                childCount = 0;
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
                    else
                    {
                        ++childCount;
                    }
                }

                if (!canEvaluate)
                {
                    nodeQueue.Enqueue(currentNode);
                    continue;
                }

                currentChances = new MoveChancesPlayerTimeline(Rules.PlayerCount);

                if (childCount > 0)
                {
                    guaranteedWin = false;
                    guaranteedLoss = false;
                    moveChancesPlayground = new MoveChancesPlayground(Rules.ValidMoves.Count);

                    for (int c = 0; c < childChances.Length; ++c)
                    {
                        if (childChances[c].Equals(none))
                        {
                            moveChancesPlayground.WinChances[c] = float.MinValue;
                            moveChancesPlayground.LooseChances[c] = float.MinValue;
                            continue;
                        }

                        tempChance = childChances[c].WinChances[1];
                        if (tempChance >= 1f)
                            guaranteedWin = true;
                        currentChances.WinChances[0] += tempChance;
                        moveChancesPlayground.WinChances[c] = tempChance;

                        tempChance = childChances[c].LooseChances[1];
                        if (tempChance >= 1f)
                            guaranteedLoss = true;
                        currentChances.LooseChances[0] += tempChance;
                        moveChancesPlayground.LooseChances[c] = tempChance;

                        for (int p = 1; p < Rules.PlayerCount; ++p)
                        {
                            childPlayerIndex = (p + 1) % Rules.PlayerCount;
                            currentChances.WinChances[p] += childChances[c].WinChances[childPlayerIndex];
                            currentChances.LooseChances[p] += childChances[c].LooseChances[childPlayerIndex];
                        }
                    }

                    if (guaranteedWin)
                        currentChances.WinChances[0] = 1f;
                    else
                        currentChances.WinChances[0] /= childCount;

                    if (guaranteedLoss)
                        currentChances.LooseChances[0] = 1f;
                    else
                        currentChances.LooseChances[0] /= childCount;

                    for (int p = 1; p < Rules.PlayerCount; ++p)
                    {
                        currentChances.WinChances[p] /= childCount;
                        currentChances.LooseChances[p] /= childCount;
                    }

                    _chances[currentNode.Current] = moveChancesPlayground;
                }
                else
                {
                    for (int p = 0; p < Rules.PlayerCount; ++p)
                    {
                        currentChances.WinChances[p] = Rules.LastMoveWins ^ (p != 1) ? 1f : 0f;
                        currentChances.LooseChances[p] = Rules.LastMoveWins ^ (p != 1) ? 0f : 1f;
                    }
                }

                chances[currentNode.Current] = currentChances;

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

        private static List<Node> GenerateTree(Rules rules)
        {
            List<Node> leafs = new List<Node>();

            Dictionary<Playground, Node> tree = new Dictionary<Playground, Node>();
            Stack<Node> generations = new Stack<Node>();

            generations.Push(new Node(rules.StartingField, rules.ValidMoves.Count));

            Node none = new Node(null, rules.ValidMoves.Count);

            Node current;
            int childrenCount;
            while (generations.Count > 0)
            {
                current = generations.Pop();

                childrenCount = 0;
                for (int i = 0; i < current.Children.Length; ++i)
                {

                    if (current.Current.TryApplyValidMove(rules.ValidMoves[i], out Playground next))
                    {
                        ++childrenCount;
                        if (tree.TryGetValue(next, out Node nextGeneration))
                        {
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

            return leafs;
        }

        private struct Node
        {
            public readonly Node[] Parents;
            public readonly Playground Current;
            public readonly Node[] Children;

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

        private struct MoveChancesPlayerTimeline
        {
            public readonly float[] WinChances;
            public readonly float[] LooseChances;

            public MoveChancesPlayerTimeline(int playerCount)
            {
                WinChances = new float[playerCount];
                LooseChances = new float[playerCount];
            }

            public override string ToString()
            {
                return $"Win: {string.Join("|", WinChances.Select(f => f < 0 ? "-" : Math.Round(f, 2).ToString(CultureInfo.InvariantCulture)))}; Loose: {string.Join("|", LooseChances.Select(f => f < 0 ? "-" : Math.Round(f, 2).ToString(CultureInfo.InvariantCulture)))}";
            }
        }

        private struct MoveChancesPlayground
        {
            public readonly float[] WinChances;
            public readonly float[] LooseChances;

            public MoveChancesPlayground(int validMoves)
            {
                WinChances = new float[validMoves];
                LooseChances = new float[validMoves];
            }

            public override string ToString()
            {
                return $"Win: {string.Join("|", WinChances.Select(f => f < 0 ? "-" : Math.Round(f, 2).ToString(CultureInfo.InvariantCulture)))}; Loose: {string.Join("|", LooseChances.Select(f => f < 0 ? "-" : Math.Round(f, 2).ToString(CultureInfo.InvariantCulture)))}";
            }
        }
    }
}
