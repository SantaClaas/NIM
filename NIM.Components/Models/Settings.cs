using System.Collections.Generic;

namespace NIM.Components.Models
{
    public class Settings
    {
        public List<int> Rows { get; set; }
        public int ChangesPerRow { get; set; }
        public bool LastMoveWins { get; set; }
    }
}
