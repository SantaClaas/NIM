using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NIM
{
    public class AiPlayerRandom : Player
    {
        private readonly Random _random;

        public AiPlayerRandom(string name) : base(name)
        {
            _random = new Random(name.GetHashCode());
        }

        public override Move DecideNextMove(Rules rules, Playground playground)
        {
            List<Move> validMoves = rules.GetValidMoves(playground);

            return validMoves[_random.Next(validMoves.Count)];
        }
    }

    public class AiPlayerFirst : Player
    {
        public AiPlayerFirst(string name) : base(name)
        {
        }

        public override Move DecideNextMove(Rules rules, Playground playground)
        {
            return rules.GetValidMoves(playground)[0];
        }
    }

    public class AiPlayerSimple : Player
    {
        public AiPlayerSimple(string name) : base(name)
        {
        }

        public override Move DecideNextMove(Rules rules, Playground playground)
        {
            if (!rules.LastMoveWins || rules.ValidMoves.Any(m => m.ChangesPerRow.Count(c => c > 0) != 1))
                throw new Exception("Incompatible ruleset");

            List<Move> validMoves = rules.GetValidMoves(playground);
            return validMoves.FirstOrDefault(m =>
                                                playground.ApplyMove(m).Rows.Aggregate((a, c) => a ^ c) == 0
                                            )
                   ?? validMoves.First();
        }
    }

    public class AiPlayerHeavy : Player
    {
        private Rules _rules;

        public AiPlayerHeavy(string name) : base(name)
        {
        }

        public override Move DecideNextMove(Rules rules, Playground playground)
        {
            CalcStrategyMap(rules);

            throw new NotImplementedException();
        }

        private void CalcStrategyMap(Rules rules)
        {
            if (_rules == rules)
                return;

            _rules = rules;

            #region Calculate last points

            Dictionary<int[], int> moveOverlap = new Dictionary<int[], int>();

            foreach (ReadOnlyCollection<int> point in InField(_rules.StartingField.Rows, true))
            {
                foreach (Move move in _rules.ValidMoves)
                {
                    if (!move.ChangesPerRow.Where((v, i) => point[i] < v).Any())
                        continue;

                    int[] coordinate = point.ToArray();

                    if (moveOverlap.ContainsKey(coordinate))
                        ++moveOverlap[coordinate];
                    else
                        moveOverlap[coordinate] = 1;
                }
            }

            IEnumerable<int[]> lastMoveTargetCoordinates = moveOverlap.Where(p => p.Value == _rules.ValidMoves.Count).Select(p => p.Key);

            #endregion //Calculate last points

            #region Calculate enemy moveset

            List<Move> enemyMoves = new List<Move>(_rules.PlayerCount * _rules.ValidMoves.Count);

            List<int> enemyMoveAdditionIndices = Enumerable.Repeat(_rules.ValidMoves.Count - 1, _rules.PlayerCount).ToList();
            foreach (ReadOnlyCollection<int> indices in InField(enemyMoveAdditionIndices, false))
            {
                Move move = _rules.ValidMoves[indices[0]];
                move = indices.Skip(1).Aggregate(move, (m, i) => m.Add(_rules.ValidMoves[i]));
                enemyMoves.Add(move);
            }
            
            #endregion //Calculate enemy moveset


        }

        private IEnumerable<ReadOnlyCollection<int>> InField(IList<int> point, bool excludeSelf)
        {
            List<int> counter = new List<int>(point.Count);
            ReadOnlyCollection<int> iterationObject = new ReadOnlyCollection<int>(counter);

            int index;
            do
            {
                index = 0;

                do
                {
                    ++counter[index];
                    if (counter[index] > point[index])
                    {
                        counter[index] = 0;
                        ++index;
                    }
                    else
                    {
                        break;
                    }
                } while (index < point.Count);

                if (!excludeSelf || index < point.Count)
                    yield return iterationObject;
            }
            while (index < point.Count);
        }
    }
}
