using System;
using System.Collections.Generic;
using System.Linq;

namespace NIM.Server.ViewModels
{
    public class SettingsViewModel
    {
        private int rowsCount;
        public int RowsCount
        {
            get => rowsCount;
            set
            {
                if (value < 1)
                    return;
                while (value > Rows.Count)
                {
                    Rows.Add(Rows.Count + 1);
                }
                while (value < Rows.Count)
                {
                    Rows.RemoveAt(Rows.Count - 1);
                }
                rowsCount = value;
            }
        }
        public int RowsMin { get; set; } = 1;
        public List<int> Rows { get; set; }
        public int ChangesPerRow { get; set; } = 3;
        public int MaxChangesPerRow { get; set; } = 3;
        public int MinChangesPerRow { get; set; } = 1;
        public bool LastMoveWins { get; set; } = true;

        public SettingsViewModel()
        {
            Rows = new List<int>(0);
            RowsCount = 4;
        }
    }
}
