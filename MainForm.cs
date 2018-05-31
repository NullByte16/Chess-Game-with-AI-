using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chess
{
    public partial class MainForm : Form
    {
        private GraphicCell[,] grid = new GraphicCell[8, 8];  // corresponding to the original matrix. to show whats there
        private HelpGraphicCell[,] HelpGrid = null;
        private EatenPieceHolder[,] EatenBlack = new EatenPieceHolder[4, 4];
        private EatenPieceHolder[,] EatenWhite = new EatenPieceHolder[4, 4];
        private EatenPieceHolder nextBlack;
        private EatenPieceHolder nextWhite;
        private Image KingW = Properties.Resources.KingW, KingB = Properties.Resources.KingB;
        private Logic logic;
        private AI ai = null; // null indicates its a human playing

        public MainForm()
        {
            InitializeComponent();
        }

        private void init()
        {
            logic = new Chess.Logic();
            InitGrid();
            InitEaten();
        }

        private void InitGrid()
        {
            var images = new Dictionary<Color, Dictionary<Type, Bitmap>>();
            #region ugly_code
            var whites = new Dictionary<Type, Bitmap>();
            var blacks = new Dictionary<Type, Bitmap>();
            images[Color.White] = whites;
            images[Color.Black] = blacks;

            whites[Type.Bishop] = Properties.Resources.BishopW;
            whites[Type.Pawn] = Properties.Resources.PawnW;
            whites[Type.Knight] = Properties.Resources.KnightW;
            whites[Type.Queen] = Properties.Resources.QueenW;
            whites[Type.King] = Properties.Resources.KingW;
            whites[Type.Rook] = Properties.Resources.RookW;

            blacks[Type.Bishop] = Properties.Resources.BishopB;
            blacks[Type.Pawn] = Properties.Resources.PawnB;
            blacks[Type.Knight] = Properties.Resources.KnightB;
            blacks[Type.Queen] = Properties.Resources.QueenB;
            blacks[Type.King] = Properties.Resources.KingB;
            blacks[Type.Rook] = Properties.Resources.RookB;
            #endregion

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    grid[i, j] = new GraphicCell(i, j, this, null);

                    Piece piece = logic.getCellContent(i, j);
                    if (piece != null)
                        grid[i, j].Image = images[piece.GetColor()][piece.GetType()];
                }
            }
        }

        private void RessurrectGrid()
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    this.Controls.Remove(grid[i, j]);
                    grid[i, j] = null;
                }
            logic.InitGrid();
            InitGrid();
        }

        public void InitEaten()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    EatenWhite[i, j] = new EatenPieceHolder(i, j, this, null, Color.White);
                    EatenBlack[i, j] = new EatenPieceHolder(i, j, this, null, Color.Black);
                }
            }

            nextWhite = EatenWhite[0, 0];
            nextBlack = EatenBlack[0, 0];
        }

        public void CheckMyColor(int i, int j)
        {
            // todo names that mean something
            if (lastChosen != null || logic.IsThisColorToPlay(i, j))
                CellClicked(i, j);
        }

        private LinkedList<Cell> ValidMoves = null;
        private GraphicCell lastChosen = null;
        public void CellClicked(int i, int j) // todo there's a bug - after choosing a target once, then choosing another target, it won't show valid moves
        {
            if (lastChosen == null) // choosing source
            {
                ValidMoves = logic.GetValidMoves(i, j);
                // would return null if no piece 
                if (ValidMoves != null)
                {
                    foreach (Cell cell in ValidMoves)
                    {
                        if (cell.piece == null && logic.grid[i, j].piece.GetType() == Type.King && Math.Abs(j - cell.J) == 2)
                            grid[cell.I, cell.J].BackColor = System.Drawing.Color.Aqua;

                        else if (cell.piece == null)
                            grid[cell.I, cell.J].BackColor = System.Drawing.Color.GreenYellow;

                        else
                            grid[cell.I, cell.J].BackColor = System.Drawing.Color.Red;
                    }

                    lastChosen = grid[i, j];
                }
            }

            else // choosing target
            {
                foreach (Cell cell in ValidMoves)
                {
                    grid[cell.I, cell.J].BackColor = grid[cell.I, cell.J].OriginalColor;
                }

                //var cell = this.logic.GetCell(i, j);
                if (!ValidMoves.Contains(this.logic.GetCell(i, j)))
                {
                    lastChosen = null;
                    return;
                }
                // return if target is not possible to get to

                /*if (logic.getCellContent(i, j) != null && !logic.IsThisColorToPlay(i, j)) // target is about get eaten!
                {
                    if (logic.GetTurn() == Color.White)
                    {
                        nextBlack.PlaceEaten(grid[i, j].Image);
                        if (nextBlack.J == 3) // todo its a magic number. at least explain
                            nextBlack = EatenBlack[nextBlack.I + 1, 0];
                        else nextBlack = EatenBlack[nextBlack.I, nextBlack.J + 1];
                    }
                    else
                    {
                        nextWhite.PlaceEaten(grid[i, j].Image);
                        if (nextWhite.J == 3)
                            nextWhite = EatenWhite[nextWhite.I + 1, 0];
                        else nextWhite = EatenWhite[nextWhite.I, nextWhite.J + 1];
                    }
                }*/

                showEaten(i, j);

                graphicallyMovePiece(lastChosen, grid[i, j]); //BREAKPOINT

                if (logic.MakeMove(i, j, lastChosen.I, lastChosen.J)) // means if checkmate 
                {
                    this.InitGrid(); // todo if checkmate, leave the board!
                    this.InitEaten();
                }

                lastChosen = null;

                // todo
                // if move was successfully done, and not checkmate... blah blah if's
                if (ai != null)
                {
                    letAIplay();
                }
            }
        }

        private void showEaten(int i, int j) //RECENT CODE
        {
            if (logic.getCellContent(i, j) != null && !logic.IsThisColorToPlay(i, j)) // target is about get eaten!
            {
                if (logic.GetTurn() == Color.White)
                {
                    nextBlack.PlaceEaten(grid[i, j].Image);
                    if (nextBlack.J == 3) // todo its a magic number. at least explain
                        nextBlack = EatenBlack[nextBlack.I + 1, 0];
                    else nextBlack = EatenBlack[nextBlack.I, nextBlack.J + 1];
                }
                else
                {
                    nextWhite.PlaceEaten(grid[i, j].Image);
                    if (nextWhite.J == 3)
                        nextWhite = EatenWhite[nextWhite.I + 1, 0];
                    else nextWhite = EatenWhite[nextWhite.I, nextWhite.J + 1];
                }
            }
        }

        private void graphicallyMovePiece(GraphicCell source, GraphicCell target)
        {
            int i, j;

            Image temp = source.Image;
            source.Image = null;
            target.Image = temp;

            if (logic.grid[source.I, source.J].piece != null)
            {
                i = source.I;
                j = source.J;
            }

            else
            {
                i = target.I;
                j = target.J;
            }
            if (logic.getCellContent(i, j).GetType() == Type.King && Math.Abs(target.J - source.J) == 2)
            {
                if (target.J < 4)
                {

                    temp = grid[target.I, 0].Image;
                    grid[target.I, 0].Image = null;

                    grid[target.I, 3].Image = temp;
                }

                if (target.J > 4)
                {

                    temp = grid[target.I, 7].Image;
                    grid[target.I, 7].Image = null;

                    grid[target.I, 5].Image = temp;
                }
            }

            if (logic.getCellContent(i, j).GetType() == Type.Pawn && (target.I == 0 || target.I == 7))
            {
                string promotion = Promote(target.I);
                switch (promotion)
                {
                    case "Knight":
                        logic.Promotion(source.I, source.J, Type.Knight);

                        if (target.I == 0)
                            grid[target.I, target.J].Image = Properties.Resources.KnightW;

                        else
                            grid[target.I, target.J].Image = Properties.Resources.KnightB;
                        break;

                    case "Rook":
                        logic.Promotion(source.I, source.J, Type.Rook);

                        if (target.I == 0)
                            grid[target.I, target.J].Image = Properties.Resources.RookW;

                        else
                            grid[target.I, target.J].Image = Properties.Resources.RookB;
                        break;

                    case "Bishop":
                        logic.Promotion(source.I, source.J, Type.Bishop);

                        if (target.I == 0)
                            grid[target.I, target.J].Image = Properties.Resources.BishopW;

                        else
                            grid[target.I, target.J].Image = Properties.Resources.BishopB;
                        break;

                    case "Queen":
                        logic.Promotion(source.I, source.J, Type.Queen);

                        if (target.I == 0)
                            grid[target.I, target.J].Image = Properties.Resources.QueenW;

                        else
                            grid[target.I, target.J].Image = Properties.Resources.QueenB;
                        break;
                }
            }
        }

        private string Promote(int i)
        {
            var form1 = new ChooseDubbedPieceWhite();
            var form2 = new ChooseDubbedPieceBlack();
            string chosen;

            if (i == 0)
            {
                form1.Location = this.Location;
                form1.ShowDialog();
                chosen = form1.chosen;
            }

            else
            {
                form2.Location = this.Location;
                form2.ShowDialog();
                chosen = form2.chosen;
            }
            // code will freeze here until that form is closed

            MessageBox.Show("you chose " + chosen);
            return chosen;
        }

        const int AI_DELAY_SECONDS = 2;
        private void letAIplay()
        {
            this.Update();
            long start = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            Move move = ai.ChooseMove();
            if (move != null)
            {
                showEaten(move.to.I, move.to.J);
                logic.MakeMove(move.to.I, move.to.J, move.from.I, move.from.J);

                graphicallyMovePiece(grid[move.from.I, move.from.J], grid[move.to.I, move.to.J]);

                long end = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                int sleep_time = 1000 * AI_DELAY_SECONDS - (int)(end - start);
                if (sleep_time > 0)
                    System.Threading.Thread.Sleep(sleep_time);
            }

            else
                EndGame();
        }

        public void EndGame()
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            HelpGrid = null;
            HelpGrid = new HelpGraphicCell[8, 8];
            Cell[,] grid = logic.HelpForm();

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    HelpGrid[i, j] = new HelpGraphicCell(i, j, this, null);

                    if (grid[i, j].piece != null && grid[i, j].piece.GetType() == Type.Pawn)
                    {
                        if (grid[i, j].piece.GetColor() == Color.Black) HelpGrid[i, j].Image = Properties.Resources.PawnB;
                        else HelpGrid[i, j].Image = Properties.Resources.PawnW;
                    }

                    if (grid[i, j].piece != null && grid[i, j].piece.GetType() == Type.Knight)
                    {
                        if (grid[i, j].piece.GetColor() == Color.Black) HelpGrid[i, j].Image = Properties.Resources.KnightB;
                        else HelpGrid[i, j].Image = Properties.Resources.KnightW;
                    }

                    if (grid[i, j].piece != null && grid[i, j].piece.GetType() == Type.Rook)
                    {
                        if (grid[i, j].piece.GetColor() == Color.Black) HelpGrid[i, j].Image = Properties.Resources.RookB;
                        else HelpGrid[i, j].Image = Properties.Resources.RookW;
                    }

                    if (grid[i, j].piece != null && grid[i, j].piece.GetType() == Type.Bishop)
                    {
                        if (grid[i, j].piece.GetColor() == Color.Black) HelpGrid[i, j].Image = Properties.Resources.BishopB;
                        else HelpGrid[i, j].Image = Properties.Resources.BishopW;
                    }

                    if (grid[i, j].piece != null && grid[i, j].piece.GetType() == Type.Queen)
                    {
                        if (grid[i, j].piece.GetColor() == Color.Black) HelpGrid[i, j].Image = Properties.Resources.QueenB;
                        else HelpGrid[i, j].Image = Properties.Resources.QueenW;
                    }

                    if (grid[i, j].piece != null && grid[i, j].piece.GetType() == Type.King)
                    {
                        if (grid[i, j].piece.GetColor() == Color.Black) HelpGrid[i, j].Image = Properties.Resources.KingB;
                        else HelpGrid[i, j].Image = Properties.Resources.KingW;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            help.Visible = false;
        }

        private void help_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Console.WriteLine("DEBUG");
            var game = this.logic;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            RessurrectGrid();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            init();
            ai = new AI(Color.Black, logic);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
            init();
            ai = null;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var form = new ChooseDubbedPieceWhite();
            form.Location = this.Location;
            form.ShowDialog();
            // code will freeze here until that form is closed

            string chosen = form.chosen;
            MessageBox.Show("you chose " + chosen);
        }
    }
}
