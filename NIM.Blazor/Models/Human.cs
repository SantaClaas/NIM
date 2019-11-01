using System;

namespace NIM.Blazor.Models
{
    public class Human : Player
    {
        public Human(string name) : base(name)
        {
        }

        public override Move DecideNextMove(Rules rules, Playground playground)
        {
            throw new NotImplementedException();
        }
    }
}
