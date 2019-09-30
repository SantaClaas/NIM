using System;
using System.Collections.Generic;
using System.Text;

namespace NIM.Core
{
    /// <summary>
    /// Name WIP
    /// </summary>
    class Rule
    {
        public bool IsLastMoveWin { get; private set; }
        public PlayGround StartingGround { get; private set; }

        List<Move> moves;

        bool ValidateMove(Move move, PlayGround playGround)
        {
            throw new NotImplementedException();
        }

        IEnumerable<Move> GetValidMoves(PlayGround playGround)
        {
            throw new NotImplementedException();
        }

    }
}
