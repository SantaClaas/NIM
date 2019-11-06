using Microsoft.AspNetCore.Components;

namespace NIM.Server.ViewModels
{
    public class SettingsViewModel
    {
        public uint RowsCount { get; set; }
        public uint RowsMin { get; set; } = 0;
        public uint RowsMax { get; set; } = 0;
        public int ChangesPerRow { get; set; } = 3;
        public int MaxChangesPerRow { get; set; } = 3;
        public int MinChangesPerRow { get; set; } = 1;
        public bool LastMoveWins { get; set; } = true;
    }
}
