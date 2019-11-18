using System.Collections.Generic;

namespace NIM.Shared.Models
{
    public class Settings
    {
        private string skin;

        public List<int> Rows { get; set; }
        public int ChangesPerRow { get; set; }
        public bool LastMoveWins { get; set; }
        public string Skin { get => skin ?? (skin = "Torch.png"); set => skin = value; }
    }
}
