using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NIM.Components.Models
{
    public class GameState
    {
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
