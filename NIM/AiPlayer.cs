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
}
