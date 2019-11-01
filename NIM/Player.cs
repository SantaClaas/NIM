using System;

namespace NIM
{
    public abstract class Player : IEquatable<Player>
    {
        public string Name { get; }

        protected Player(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public abstract Move DecideNextMove(Rules rules, Playground playground);

        public bool Equals(Player other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Player)obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}
