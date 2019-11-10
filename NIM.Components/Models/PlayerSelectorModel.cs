using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NIM.Components.Models
{
    public class PlayerSelectorModel
    {
        public bool IsAiPlayer { get; set; }
        public string Name { get; set; }
        public float DifficultyAdvancedAi { get => difficultyAdvancedAi; set { difficultyAdvancedAi = value >= -1 || value <= 1 ? value : difficultyAdvancedAi; } }
        private float difficultyAdvancedAi;
        public bool IsNameValid { get; set; } = true;

    }
}
