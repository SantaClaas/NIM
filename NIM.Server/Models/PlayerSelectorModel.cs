using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NIM.Server.Models
{
    public class PlayerSelectorModel
    {
        public int Number { get; set; }
        public bool IsAiPlayer { get; set; }
        public string Name { get; set; }
        public Player SelectedAiPlayer { get; set; }
        public float DifficultyAdvancedAi { get; set; }
        public List<Player> ValidAiPlayers { get; set; }

    }
    public class AiPlayerChangeArgs
    {
        public ChangeEventArgs ChangeEventArgs { get; set; }
        public int ModelNumber { get; set; }
    }
}
