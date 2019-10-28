namespace NIM.Blazor.ViewModels
{
    public class NimViewModel
    {
        public string[] Players { get; set; } = new string[] { "Player 1", "Player 2" };
        public string CurrentPlayer => Players[ActivePlayer];
        public int ActivePlayer { get; set; } = 0;
        public int[] Rows { get; set; } = new int[] { 7, 8 };

        public void NextPlayer()
        {
            if (ActivePlayer < Players.Length)
                ActivePlayer++;
            else
                ActivePlayer = 0;
        }
    }
}
