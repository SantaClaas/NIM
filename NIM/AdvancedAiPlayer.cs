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

        public AdvancedAiPlayer(string name) : base(name)
        {
            _random = new Random(name.GetHashCode());
        }

        public AdvancedAiPlayer(string name, Rules rules) : this(name)
        {
            _gameTree = Node.CalcGameTree(rules);
        }

        public AdvancedAiPlayer(string name, AdvancedAiPlayer teacher) : this(name)
        {
            _gameTree = teacher._gameTree;
        }

        public override Move DecideNextMove(Rules rules, Playground playground)
        {
            if (_gameTree is null || rules != _gameTree.Rules)
                _gameTree = Node.CalcGameTree(rules);

            List<Node> entryPoints = _gameTree.Find(n => n.Playground.Equals(playground));

            List<Move> moves = new List<Move>();

            foreach (Node node in entryPoints)
                foreach (KeyValuePair<Move, Node> choice in node.Children)
                    if (choice.Value.IsWinningBranch(node.Player))
                        moves.Add(choice.Key);

            if (moves.Count == 0)
                moves = rules.GetValidMoves(playground);

            return moves[_random.Next(moves.Count)];
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

        public bool IsWinningBranch(int player)
        {
            if (_children.Count == 0)
                return Rules.LastMoveWins ^ Player != player;

            return Player == player
                ? _children.Values.Any(n => n.IsWinningBranch(player))
                : _children.Values.All(n => n.IsWinningBranch(player));
        }
    }
}
