using System;

namespace NIM.Server.Models
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
