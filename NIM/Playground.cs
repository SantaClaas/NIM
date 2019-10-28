using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NIM
{
    public class Playground
    {
        public ReadOnlyCollection<int> Rows { get; }

        public int this[int index] => Rows[index];

        public Playground(Playground playground)
        {
            Rows = Array.AsReadOnly(playground.Rows.ToArray());
        }

        public Playground(IEnumerable<int> rows)
        {
            Rows = Array.AsReadOnly(rows.ToArray());
        }

        public Playground ApplyMove(Move move)
        {
            return new Playground(Rows.Select((s, i) => s - move.ChangesPerRow[i]));
        }
    }
}
