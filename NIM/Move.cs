using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NIM
{
    public class Move : IEquatable<Move>
    {
        public ReadOnlyCollection<int> ChangesPerRow { get; }

        public int this[int index] => ChangesPerRow[index];

        public Move(IEnumerable<int> changesPerRow)
        {
            ChangesPerRow = Array.AsReadOnly(changesPerRow.ToArray());
        }

        public override string ToString()
        {
            return $"Changes: {string.Join(", ", ChangesPerRow)}";
        }

        public bool Equals(Move other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (ChangesPerRow.Count != other.ChangesPerRow.Count)
                return false;

            return !ChangesPerRow.Where((c, i) => c != other.ChangesPerRow[i]).Any();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Move)obj);
        }

        public override int GetHashCode()
        {
            if (ChangesPerRow is null)
                return 0;

            unchecked
            {
                int hash = 7103;
                foreach (int changePerRow in ChangesPerRow)
                    hash += changePerRow * 4591;

                return hash;
            }
        }

        public Move Add(Move other)
        {
            if (ChangesPerRow.Count != other.ChangesPerRow.Count)
                throw new ArgumentException("Dimensions do not match");

            return new Move(ChangesPerRow.Select((c, i) => c + other.ChangesPerRow[i]));
        }
    }
}
