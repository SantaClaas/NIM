using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NIM
{
    public class Rules
    {
        public static Rules Default { get; }

        static Rules()
        {
            Default = new Rules(
                new ReadOnlyCollection<Move>(new[]
                {
                    new Move(new []{1,0,0,0}),
                    new Move(new []{2,0,0,0}),
                    new Move(new []{3,0,0,0}),
                    new Move(new []{0,1,0,0}),
                    new Move(new []{0,2,0,0}),
                    new Move(new []{0,3,0,0}),
                    new Move(new []{0,0,1,0}),
                    new Move(new []{0,0,2,0}),
                    new Move(new []{0,0,3,0}),
                    new Move(new []{0,0,0,1}),
                    new Move(new []{0,0,0,2}),
                    new Move(new []{0,0,0,3})
                }),
                 new Playground(new[] { 1, 2, 3, 4 }),
                true,
                2
                );
        }

        public ReadOnlyCollection<Move> ValidMoves { get; }

        public Playground StartingField { get; }

        public bool LastMoveWins { get; }

        public int PlayerCount { get; }

        private Rules(ReadOnlyCollection<Move> moves, Playground field, bool? win, int? players)
        {
            ValidMoves = moves ?? Default.ValidMoves;
            StartingField = field ?? Default.StartingField;
            LastMoveWins = win ?? Default.LastMoveWins;
            PlayerCount = players ?? Default.PlayerCount;
        }

        public static Builder Build(IEnumerable<int> rows)
        {
            return new Builder(rows);
        }

        public bool IsMoveValid(Move move, Playground playground)
        {
            if (!ValidMoves.Contains(move))
                return false;

            return !playground.Rows.Where((s, i) => s < move.ChangesPerRow[i]).Any();
        }

        public List<Move> GetValidMoves(Playground playground)
        {
            return ValidMoves.Where(m => IsMoveValid(m, playground)).ToList();
        }

        public class Builder
        {
            private List<Move> _validMoves;

            private readonly Playground _startingField;

            private bool? _lastMoveWins;

            private int? _playerCount;

            internal Builder(IEnumerable<int> rows)
            {
                _startingField = new Playground(rows);
                _validMoves = null;
                _lastMoveWins = null;
                _playerCount = null;
            }

            public Rules Create()
            {
                return new Rules(_validMoves is null ? null : new ReadOnlyCollection<Move>(_validMoves.Distinct().ToList()), new Playground(_startingField), _lastMoveWins, _playerCount);
            }

            public Builder AddRules(params Move[] moves)
            {
                if (_validMoves is null)
                    _validMoves = new List<Move>();

                if (moves.Any(m => m.ChangesPerRow.Count != _startingField.Rows.Count || m.ChangesPerRow.Any(a => a < 0)))
                    throw new Exception("Invalid move");

                _validMoves.AddRange(moves);
                return this;
            }

            public Builder AddSingleRowRules(int min, int max)
            {
                for (int row = 0; row < _startingField.Rows.Count; ++row)
                {
                    int[] rows = new int[_startingField.Rows.Count];
                    for (int i = min; i <= max; ++i)
                    {
                        rows[row] = i;
                        AddRules(new Move(rows));
                    }
                }

                return this;
            }

            public Builder AddMultiRowRules(params int[] amountPerRow)
            {
                if (amountPerRow.Length > _startingField.Rows.Count)
                    throw new Exception("Too many rows");

                int[] rows = new int[_startingField.Rows.Count];
                Array.Copy(amountPerRow, rows, amountPerRow.Length);

                foreach (IEnumerable<int> permutation in Permutations(rows))
                    AddRules(new Move(permutation));

                return this;
            }

            public Builder LastMoveWins()
            {
                _lastMoveWins = true;
                return this;
            }

            public Builder LastMoveLooses()
            {
                _lastMoveWins = false;
                return this;
            }

            public Builder Players(int count)
            {
                _playerCount = count;
                return this;
            }

            private IEnumerable<IEnumerable<T>> Permutations<T>(IEnumerable<T> items)
            {
                List<T> list = items.ToList();
                if (list.Count < 2)
                    yield return list;
                else
                {
                    foreach (T item in list)
                    {
                        foreach (IEnumerable<T> permutation in Permutations(list.Where(i => !i.Equals(item))))
                            yield return permutation.Prepend(item);
                    }
                }
            }
        }
    }
}
