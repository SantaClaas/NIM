using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NIM
{
    public class Playground:IEquatable<Playground>
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

        public override string ToString()
        {
            return $"Field: {string.Join(", ",Rows)}";
        }

        public bool Equals(Playground other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Rows.Count != other.Rows.Count)
                return false;

            return !Rows.Where((c, i) => c != other.Rows[i]).Any();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Playground)obj);
        }

        public override int GetHashCode()
        {
            if (Rows is null)
                return 0;

            unchecked
            {
                int hash = 7103;
                foreach (int changePerRow in Rows)
                    hash += changePerRow * 4597;

                return hash;
            }
        }
    }
}
