using System;
using System.Collections.Generic;

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

   
}
