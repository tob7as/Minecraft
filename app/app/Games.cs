using System;
using System.Drawing;
using System.Windows.Forms;

namespace CapriDesktop
{
    // The little game windows she opens with play_game. She talks through each result with the line she's handed.
    internal abstract class GameWindow : Form
    {
        protected readonly Action<string> Say;
        protected readonly Random Luck = new Random();
        protected readonly Label Status;

        protected GameWindow(string title, Size size, Action<string> say)
        {
            Say = say;
            AutoScaleDimensions = new SizeF(96f, 96f);
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            BackColor = Look.Panel;
            ForeColor = Look.Ink;
            Font = Look.Mono(10f, FontStyle.Regular, GraphicsUnit.Point);
            ClientSize = size;
            Status = new Label { Bounds = new Rectangle(16, size.Height - 84, size.Width - 32, 36), ForeColor = Look.Dim, TextAlign = ContentAlignment.MiddleCenter };
            Controls.Add(Status);
        }

        protected Button Pad(string text, Rectangle bounds, Color accent)
        {
            Button button = new Button { Text = text, Bounds = bounds, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(17, 21, 28), ForeColor = Look.Ink, UseVisualStyleBackColor = false, TabStop = false };
            button.FlatAppearance.BorderColor = accent;
            Controls.Add(button);
            return button;
        }

        protected string Pick(params string[] lines) { return lines[Luck.Next(lines.Length)]; }
    }

    // Noughts and crosses: they're X and go first, she's O. She plays well but slips now and then, so it can be won.
    internal sealed class NoughtsAndCrosses : GameWindow
    {
        private readonly Button[] cells = new Button[9];
        private readonly char[] board = new char[9];
        private bool over;
        private int wins, losses, draws;
        private static readonly int[,] Lines = { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 }, { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 }, { 0, 4, 8 }, { 2, 4, 6 } };

        internal NoughtsAndCrosses(Action<string> say) : base("Noughts and crosses · Capri", new Size(300, 380), say)
        {
            for (int i = 0; i < 9; i++)
            {
                int cell = i;
                cells[i] = Pad("", new Rectangle(30 + i % 3 * 82, 20 + i / 3 * 82, 76, 76), Look.Neon);
                cells[i].Font = Look.Mono(28f, FontStyle.Bold, GraphicsUnit.Point);
                cells[i].Click += delegate { Played(cell); };
            }
            Pad("New game", new Rectangle(30, 332, 240, 32), Look.Cyan).Click += delegate { Reset(); };
            Reset();
        }

        private void Reset()
        {
            for (int i = 0; i < 9; i++) { board[i] = ' '; cells[i].Text = ""; cells[i].Enabled = true; }
            over = false;
            Show("Your move. You're X.");
        }

        private void Show(string text) { Status.Text = text + "\nYou " + wins + "  ·  Capri " + losses + "  ·  Draws " + draws; }

        private void Played(int cell)
        {
            if (over || board[cell] != ' ') return;
            Mark(cell, 'X');
            if (Finished()) return;
            Mark(Choose(), 'O');
            if (!Finished()) Show("Your move.");
        }

        private void Mark(int cell, char who)
        {
            board[cell] = who;
            cells[cell].Text = who.ToString();
            cells[cell].ForeColor = who == 'X' ? Look.Cyan : Look.Pink;
        }

        private bool Finished()
        {
            char winner = Winner(board);
            bool full = Array.IndexOf(board, ' ') < 0;
            if (winner == ' ' && !full) return false;
            over = true;
            foreach (Button cell in cells) cell.Enabled = false;
            if (winner == 'X') { wins++; Show("You win!"); Say(Pick("No way. You actually beat me. Rematch, choom.", "Okay, okay, you got me. Don't let it go to your head.", "Glitch in my targeting. That's my story and I'm sticking to it.")); }
            else if (winner == 'O') { losses++; Show("Capri wins."); Say(Pick("Three in a row. Too easy, choom.", "Netrunner reflexes. Wanna go again?", "Flatlined. Better luck next round.")); }
            else { draws++; Show("Draw."); Say(Pick("A draw. We're too evenly matched, choom.", "Stalemate. Run it back?", "Nobody wins. Again?")); }
            return true;
        }

        private static char Winner(char[] cells)
        {
            for (int i = 0; i < 8; i++)
                if (cells[Lines[i, 0]] != ' ' && cells[Lines[i, 0]] == cells[Lines[i, 1]] && cells[Lines[i, 1]] == cells[Lines[i, 2]]) return cells[Lines[i, 0]];
            return ' ';
        }

        // The best move by minimax, except one time in five she takes a random free square.
        private int Choose()
        {
            System.Collections.Generic.List<int> free = new System.Collections.Generic.List<int>();
            for (int i = 0; i < 9; i++) if (board[i] == ' ') free.Add(i);
            if (Luck.Next(5) == 0) return free[Luck.Next(free.Count)];
            int best = free[0], score = int.MinValue;
            foreach (int cell in free)
            {
                board[cell] = 'O';
                int value = Score(false, 0);
                board[cell] = ' ';
                if (value > score) { score = value; best = cell; }
            }
            return best;
        }

        private int Score(bool hers, int depth)
        {
            char winner = Winner(board);
            if (winner == 'O') return 10 - depth;
            if (winner == 'X') return depth - 10;
            if (Array.IndexOf(board, ' ') < 0) return 0;
            int best = hers ? int.MinValue : int.MaxValue;
            for (int i = 0; i < 9; i++)
            {
                if (board[i] != ' ') continue;
                board[i] = hers ? 'O' : 'X';
                int value = Score(!hers, depth + 1);
                board[i] = ' ';
                best = hers ? Math.Max(best, value) : Math.Min(best, value);
            }
            return best;
        }
    }

    // Rock paper scissors, first to three. She picks at random.
    internal sealed class RockPaperScissors : GameWindow
    {
        private static readonly string[] Moves = { "Rock", "Paper", "Scissors" };
        private readonly Label shown;
        private int yours, hers;

        internal RockPaperScissors(Action<string> say) : base("Rock paper scissors · Capri", new Size(360, 300), say)
        {
            shown = new Label { Bounds = new Rectangle(16, 20, 328, 70), ForeColor = Look.Ink, TextAlign = ContentAlignment.MiddleCenter, Font = Look.Mono(12f, FontStyle.Bold, GraphicsUnit.Point), Text = "First to three.\nPick one." };
            Controls.Add(shown);
            for (int i = 0; i < 3; i++)
            {
                int move = i;
                Pad(Moves[i], new Rectangle(16 + i * 112, 104, 104, 64), Look.Neon).Click += delegate { Played(move); };
            }
            Pad("New game", new Rectangle(16, 252, 328, 32), Look.Cyan).Click += delegate { yours = hers = 0; shown.Text = "First to three.\nPick one."; Score(); };
            Score();
        }

        private void Score() { Status.Text = "You " + yours + "  ·  Capri " + hers; }

        private void Played(int move)
        {
            if (yours >= 3 || hers >= 3) { yours = hers = 0; }
            int pick = Luck.Next(3);
            shown.Text = "You: " + Moves[move] + "\nCapri: " + Moves[pick];
            int result = (move - pick + 3) % 3;
            if (result == 1) yours++;
            else if (result == 2) hers++;
            Score();
            if (yours == 3) { Say(Pick("You win the set. Lucky streak, choom.", "Three rounds to you. I'll get you next time.", "Fine. You win. Rematch?")); return; }
            if (hers == 3) { Say(Pick("Set's mine. Predictable, choom.", "Three-zip energy. Wanna go again?", "Read you like open source code.")); return; }
            if (result == 0) Say(Pick("Same pick. Great minds.", "Tie. Again.", "Mirror match."));
            else if (result == 1) Say(Pick(Moves[move] + " beats " + Moves[pick].ToLowerInvariant() + ". Your point.", "Point to you. Don't get cocky.", "Okay, that one's yours."));
            else Say(Pick(Moves[pick] + " beats " + Moves[move].ToLowerInvariant() + ". Mine.", "Point to me, choom.", "Called it."));
        }
    }
}
