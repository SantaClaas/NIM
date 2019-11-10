using System;

namespace NIM.Components.Models
{
    public class Human : Player
    {
        public Human(string name) : base(name)
        {
            
        }
        public int[] NextMove { get; set; }

        public override Move DecideNextMove(Rules rules, Playground playground)
        {
            return new Move(NextMove);
        }

      
    }
}
