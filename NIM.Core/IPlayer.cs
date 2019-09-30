using System;
using System.Collections.Generic;
using System.Text;

namespace NIM.Core
{
    interface IPlayer
    {
        string Name { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="rule">The rule object to check validate move</param>
        /// <param name="playGround">The current playground to create all possible moves</param>
        /// <returns></returns>
        Move MakeMove(Rule rule, PlayGround playGround);
        

    }
}
