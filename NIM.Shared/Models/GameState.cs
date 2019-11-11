using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NIM.Shared.Models
{
    public class GameState
    {

        public string Skin { get; set; } = "Torch.png"; //"https://cdn.pixabay.com/photo/2016/10/05/19/08/match-1717377_960_720.png";
        public int[] Field { get; set; }
        public int[] CurrentMove { get; set; }
        public bool IsInitialized => Game != null;

        public Settings Settings { get; set; }
        public Game Game { get; set; }
        public Rules Rules
        {
            get => rules ?? RulesBuilder?.Create() ?? Rules.Default;
            set => rules = value;

        }
        public List<Player> Players { get; set; }
        public Rules.Builder RulesBuilder { get; set; }
        private Rules rules;
        public Dictionary<Player, int> SummaryTakes { get; set; }
    }
}
