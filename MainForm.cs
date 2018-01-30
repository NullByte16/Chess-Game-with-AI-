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
        private Logic logic;

        public MainForm()
        {
            InitializeComponent();
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

        private void RessurectGrid()
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
            for(int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 4; j++)
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
                Cellclicked(i, j);
        }

        private LinkedList<Cell> ValidMoves = null;
        private GraphicCell lastChosen = null;
        public void Cellclicked(int i, int j)
        {
            if(lastChosen == null) // choosing source
            {
                ValidMoves = logic.GetValidMoves(i, j);
                // would return null if no piece 
                if (ValidMoves != null)
                {
                    foreach (Cell cell in ValidMoves)
                    {
                        grid[cell.I, cell.J].BackColor = System.Drawing.Color.GreenYellow;
                    }
                }

                lastChosen = grid[i, j];
            }

            else // choosing target
            {
                foreach (Cell cell in ValidMoves)
                {
                    grid[cell.I, cell.J].BackColor = grid[cell.I, cell.J].OriginalColor;
                }

                //var cell = this.logic.GetCell(i, j);
                if (!ValidMoves.Contains(this.logic.GetCell(i, j)))
                    return; // return if target is not possible to get to

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

                Image temp = lastChosen.Image;
                lastChosen.Image = null;
                grid[i, j].Image = temp;

                if (logic.MakeMove(i, j, lastChosen.I, lastChosen.J)) // means if checkmate
                {
                    this.InitGrid();
                    this.InitEaten();
                }

                lastChosen = null;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HelpGrid = null;
            HelpGrid = new HelpGraphicCell[8, 8];
            Cell[,] grid = logic.HelpForm();

            for(int i = 0; i < 8; i++)
            {
                for(int j = 0; j < 8; j++)
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
            RessurectGrid();
        }
    }
}
