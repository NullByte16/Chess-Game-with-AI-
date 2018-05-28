﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    public class Logic
    {
        public Cell[,] grid = new Cell[8, 8]; // todo private
        private Color CurrentTurn = Color.White;
        private bool[] castlingHasMoved = new bool[6];
        private Cell[] castlingPieces = new Cell[6];

        public Logic()
        {
            // todo init all cells. some of the cell shall include a piece
            for(int i = 0; i < this.castlingHasMoved.Length; i++)
            {
                castlingHasMoved[i] = false;
            }

            castlingPieces[0] = grid[7, 3];
            castlingPieces[1] = grid[0, 3];
            castlingPieces[2] = grid[7, 0];
            castlingPieces[3] = grid[7, 7];
            castlingPieces[4] = grid[0, 0];
            castlingPieces[5] = grid[0, 7];

            InitGrid();
        }

        public Piece getCellContent(int i, int j)
        {
            return this.grid[i, j].piece;
        }

        public void InitGrid()
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    grid[i, j] = new Cell(i, j, null);

            for (int j = 0; j < 8; j++)
            {
                //Init Pawns
                grid[1, j].piece = new Piece(Type.Pawn, Color.Black);
                grid[6, j].piece = new Piece(Type.Pawn, Color.White);
            }

            //Init Rooks
            grid[0, 0].piece = new Piece(Type.Rook, Color.Black);
            grid[0, 7].piece = new Piece(Type.Rook, Color.Black);
            grid[7, 0].piece = new Piece(Type.Rook, Color.White);
            grid[7, 7].piece = new Piece(Type.Rook, Color.White);

            //Init Knights
            grid[0, 1].piece = new Piece(Type.Knight, Color.Black);
            grid[0, 6].piece = new Piece(Type.Knight, Color.Black);
            grid[7, 1].piece = new Piece(Type.Knight, Color.White);
            grid[7, 6].piece = new Piece(Type.Knight, Color.White);

            //Init Bishops
            grid[0, 2].piece = new Piece(Type.Bishop, Color.Black);
            grid[0, 5].piece = new Piece(Type.Bishop, Color.Black);
            grid[7, 2].piece = new Piece(Type.Bishop, Color.White);
            grid[7, 5].piece = new Piece(Type.Bishop, Color.White);

            //Init Queens
            grid[0, 4].piece = new Piece(Type.Queen, Color.Black);
            grid[7, 4].piece = new Piece(Type.Queen, Color.White);

            //Init Kings
            grid[0, 3].piece = new Piece(Type.King, Color.Black);
            grid[7, 3].piece = new Piece(Type.King, Color.White);
        }

        /// <summary>
        /// Returns the current color who's turn it is to play.
        /// </summary>
        public Color GetTurn()
        {
            return CurrentTurn;
        }

        /// <summary>
        /// Switches the turn over to the opposite color.
        /// </summary>
        public void NextTurn()
        {
            if (GetTurn() == Color.Black) this.CurrentTurn = Color.White;
            else this.CurrentTurn = Color.Black;
        }

        /// <summary>
        /// Returns true if color of piece in cell at index [i, j] is the same as the color who's turn it currently is.
        /// </summary>
        public bool IsThisColorToPlay(int i, int j)
        {
            if (grid[i, j].piece != null && grid[i, j].piece.GetColor() == GetTurn())
                return true;
            return false;
        }

        /// <summary>
        /// Moves the piece at index [fromI, fromJ] to the cell at index [toI, toJ]. Returns true if there is checkmate,
        /// else returns false. * If checkmate exists, will present appropriate MessageBox and initialize the grid. *
        /// </summary>
        public bool MakeMove(int toI, int toJ, int fromI, int fromJ)
        {
            // Also switch turns if success      
            if (grid[fromI, fromJ].piece == null)
                throw new Exception("attempt to move a null piece");

            if (grid[fromI, fromJ].piece.GetType() == Type.King && Math.Abs(toJ - fromJ) == 2)
            {
                if(toI > 4)
                {
                    if (toJ < fromJ)
                        MakeMove(7, 2, 7, 0);
                    else
                        MakeMove(7, 4, 7, 7);
                }
                else
                {
                    if (toJ < fromJ)
                        MakeMove(0, 2, 0, 0);
                    else
                        MakeMove(0, 4, 0, 7);
                }
            }

            grid[toI, toJ].piece = grid[fromI, fromJ].piece;
            grid[fromI, fromJ].piece = null;

            NextTurn();
            if (InCheck(GetTurn())) {
                if (InCheckMate(GetTurn())) {
                    NextTurn();
                    System.Windows.Forms.MessageBox.Show("Checkmate!!! " + GetTurn() + " is the winner!");
                    InitGrid();
                    return true;
                }
            }

            return false;
        }

        public Cell GetCell(int i, int j)
        {
            return grid[i, j];
        }

        public LinkedList<Cell> GetValidMoves(Cell cell)
        {
            return GetValidMoves(cell.I, cell.J);
        }

        public override string ToString()
        {
            String st = "";
            for (int i = 0; i < 8; i++)
            {
                for(int j = 0; j < 8; j++)
                {
                    string piece = "X";
                    if (grid[i, j].piece == null)
                        piece = " ";
                    else if (grid[i, j].piece.GetType() == Type.Bishop)
                        piece = "b";

                    else if (grid[i, j].piece.GetType() == Type.King)
                        piece = "k";
                    else if (grid[i, j].piece.GetType() == Type.Knight)
                        piece = "n";
                    else if (grid[i, j].piece.GetType() == Type.Pawn)
                        piece = "p";
                    else if (grid[i, j].piece.GetType() == Type.Queen)
                        piece = "q";
                    else if (grid[i, j].piece.GetType() == Type.Rook)
                        piece = "r";

                    if(grid[i, j].piece != null)
                    {
                        if (grid[i, j].piece.GetColor() == Color.Black)
                            piece = piece.ToUpper();
                    }
                    st += piece;
                }
                st += "\n";
            }
            return st;
        }

        /// <summary>
        /// Returns a LinkedList with all Cells to which the piece in the cell at index [IndexerCell.I, IndexerCell.J]
        /// is allowed to move.
        /// </summary>
        public LinkedList<Cell> GetValidMoves(int i, int j)
        {
            Cell cell = grid[i, j];

            if (cell.piece == null)
                return null;
            LinkedList<Cell> result = new LinkedList<Cell>();

            // if not check, dont inclucde moves that lead to check
            // todo
            
            switch (cell.piece.GetType())
            {
                case Type.Pawn:
                    CheckMovesPawn(cell, result); // its a void function. it fills up our list.
                    break;

                case Type.Knight:
                    CheckMovesKnight(cell, result);
                    break;

                case Type.Rook:
                    CheckMovesRook(cell, result);
                    break;

                case Type.Bishop:
                    CheckMovesBishop(cell, result);
                    break;

                case Type.Queen:
                    CheckMovesQueen(cell, result);
                    break;

                case Type.King:
                    CheckMovesKing(cell, result);
                    break;
            }
            #region move_simulation
            LinkedList<Cell> remover = new LinkedList<Cell>();
            Piece tempPiece = cell.piece;
            cell.piece = null;
            Piece occupied_piece = null;// To save any existing pieces in the current move of the current piece, from being deleted.
            foreach(var target_cell in result)
            {
                occupied_piece = target_cell.piece;
                target_cell.piece = tempPiece;
                if(InCheck(target_cell.piece.GetColor()))
                {
                    remover.AddLast(target_cell);
                }
                target_cell.piece = occupied_piece;
            }
            cell.piece = tempPiece;
            #endregion

            foreach (Cell remove in remover)
            {
                result.Remove(remove);
            }

            return result;
        }

        #region MoveCheckers
        //Checkers. Different assisstive methods for determining valid moves for each piece type.

        /// <summary>
        /// Updates the LinkedList result to contain all Cells to which the pawn in cell is allowed to move.
        /// </summary>
        private void CheckMovesPawn(Cell cell, LinkedList<Cell> result)
        {
            if (cell.piece.GetColor() == Color.Black && cell.I + 1 < 8)
            {
                if (cell.I == 1 && grid[cell.I + 2, cell.J].piece == null)
                    result.AddLast(grid[cell.I + 2, cell.J]);

                if (grid[cell.I + 1, cell.J].piece == null)
                    result.AddLast(grid[cell.I + 1, cell.J]);

                if (validIndexes(cell.I + 1, cell.J + 1) && grid[cell.I + 1, cell.J + 1].piece != null && grid[cell.I + 1, cell.J + 1].piece.GetColor() == Color.White)
                    result.AddLast(grid[cell.I + 1, cell.J + 1]);

                if (validIndexes(cell.I + 1, cell.J - 1) && grid[cell.I + 1, cell.J - 1].piece != null && grid[cell.I + 1, cell.J - 1].piece.GetColor() == Color.White)
                    result.AddLast(grid[cell.I + 1, cell.J - 1]);
            }

            else if (cell.I - 1 > -1)
            {
                if (cell.I == 6 && grid[cell.I - 2, cell.J].piece == null)
                    result.AddLast(grid[cell.I - 2, cell.J]);

                if (grid[cell.I - 1, cell.J].piece == null)
                    result.AddLast(grid[cell.I - 1, cell.J]);

                if (validIndexes(cell.I - 1, cell.J + 1) && grid[cell.I - 1, cell.J + 1].piece != null && grid[cell.I - 1, cell.J + 1].piece.GetColor() == Color.Black)
                    result.AddLast(grid[cell.I - 1, cell.J + 1]);

                if (validIndexes(cell.I - 1, cell.J - 1) && grid[cell.I - 1, cell.J - 1].piece != null && grid[cell.I - 1, cell.J - 1].piece.GetColor() == Color.Black)
                    result.AddLast(grid[cell.I - 1, cell.J - 1]);
            }
        }

        /// <summary>
        /// Updates the LinkedList result to contain all Cells to which the knight in cell is allowed to move.
        /// </summary>
        public void CheckMovesKnight(Cell cell, LinkedList<Cell> result)
        {
            if (validIndexes(cell.I - 1, cell.J - 2))
                result.AddLast(grid[cell.I - 1, cell.J - 2]);

            if (validIndexes(cell.I - 1, cell.J + 2))
                result.AddLast(grid[cell.I - 1, cell.J + 2]);

            if (validIndexes(cell.I + 1, cell.J - 2))
                result.AddLast(grid[cell.I + 1, cell.J - 2]);

            if (validIndexes(cell.I + 1, cell.J + 2))
                result.AddLast(grid[cell.I + 1, cell.J + 2]);

            if (validIndexes(cell.I - 2, cell.J - 1))
                result.AddLast(grid[cell.I - 2, cell.J - 1]);

            if (validIndexes(cell.I - 2, cell.J + 1))
                result.AddLast(grid[cell.I - 2, cell.J + 1]);

            if (validIndexes(cell.I + 2, cell.J - 1))
                result.AddLast(grid[cell.I + 2, cell.J - 1]);

            if (validIndexes(cell.I + 2, cell.J + 1))
                result.AddLast(grid[cell.I + 2, cell.J + 1]);

            //Refine result to only include cells that do not contain friendly pieces.
            LinkedList<Cell> remover = new LinkedList<Cell>();
            foreach(Cell checker in result)
            {
                if (checker.piece != null && checker.piece.GetColor() == cell.piece.GetColor()) remover.AddLast(checker);
            }

            foreach(Cell remove in remover)
                result.Remove(remove);
        }

        /// <summary>
        /// Updates the LinkedList result to contain all Cells to which the rook in cell is allowed to move.
        /// </summary>
        public void CheckMovesRook(Cell cell, LinkedList<Cell> result)
        {
            for(int count = 0; count <= 7; count++)
            {
                if (grid[count, cell.J] != cell)
                    result.AddLast(grid[count, cell.J]);

                if (grid[cell.I, count] != cell)
                    result.AddLast(grid[cell.I, count]);
            }
            //Refine rook valid moves to only contain empty cells or cells with enemy pieces. //TODO TODO TODO

            RefineRookByPieces(cell, result);
        }

        /// <summary>
        /// Checks for moves in result that contain other pieces. Moves that contain friendly pieces, will be removed,
        /// as well as all moves that require a rook to 'skip' over the friendly piece. For moves that contain the opponent's
        /// pieces, the case is the same as with friendly pieces, except that the move with the opponent piece will remain.
        /// </summary>
        private void RefineRookByPieces(Cell cell, LinkedList<Cell> result)
        {
            LinkedList<Cell> remover = new LinkedList<Cell>();

            for (int i = 0; i < 8; i++)
            {
                if (grid[i, cell.J].piece != null && i != cell.I)
                {
                    if (grid[i, cell.J].piece.GetColor() == cell.piece.GetColor() && i > cell.I)
                    {
                        foreach (Cell checker in result)
                        {
                            if (checker.I >= i) remover.AddLast(checker);
                        }
                    }

                    else if (grid[i, cell.J].piece.GetColor() == cell.piece.GetColor() && i < cell.I)
                    {
                        foreach (Cell checker in result)
                        {
                            if (checker.I <= i) remover.AddLast(checker);
                        }
                    }

                    else if (grid[i, cell.J].piece.GetColor() != cell.piece.GetColor() && i > cell.I)
                    {
                        foreach (Cell checker in result)
                        {
                            if (checker.I > i) remover.AddLast(checker);
                        }
                    }

                    else if (grid[i, cell.J].piece.GetColor() != cell.piece.GetColor() && i < cell.I)
                    {
                        foreach (Cell checker in result)
                        {
                            if (checker.I < i) remover.AddLast(checker);
                        }
                    }
                }
            }

            for (int j = 0; j < 8; j++)
            {
                if (grid[cell.I, j].piece != null && j != cell.J)
                {
                    if (grid[cell.I, j].piece.GetColor() == cell.piece.GetColor() && j > cell.J)
                    {
                        foreach (Cell checker in result)
                        {
                            if (checker.J >= j) remover.AddLast(checker);
                        }
                    }

                    else if (grid[cell.I, j].piece.GetColor() == cell.piece.GetColor() && j < cell.J)
                    {
                        foreach (Cell checker in result)
                        {
                            if (checker.J <= j) remover.AddLast(checker);
                        }
                    }

                    else if (grid[cell.I, j].piece.GetColor() != cell.piece.GetColor() && j > cell.J)
                    {
                        foreach (Cell checker in result)
                        {
                            if (checker.J > j) remover.AddLast(checker);
                        }
                    }

                    else if (grid[cell.I, j].piece.GetColor() != cell.piece.GetColor() && j < cell.J)
                    {
                        foreach (Cell checker in result)
                        {
                            if (checker.J < j) remover.AddLast(checker);
                        }
                    }
                }
            }

            foreach (Cell remove in remover)
            {
                Cell temp = remove;
                result.Remove(temp);
                //System.Windows.Forms.MessageBox.Show(temp.ToString());
            }
        }

        /// <summary>
        /// Updates the LinkedList result to contain all Cells to which the bishop in cell is allowed to move.
        /// </summary>
        public void CheckMovesBishop(Cell cell, LinkedList<Cell> result)
        {
            int i = cell.I - 1, j = cell.J - 1;

            //Check for available moves in all four diagonal directions from the Bishop.
            while(i > -1 && j > -1)
            {
                if(grid[i, j].piece != null)
                {
                    if (grid[i, j].piece.GetColor() == cell.piece.GetColor()) break;
                    else
                    {
                        result.AddLast(grid[i, j]);
                        break;
                    }
                }

                result.AddLast(grid[i, j]);
                i--;
                j--;
            }

            i = cell.I + 1;
            j = cell.J + 1;
            while (i < 8 && j < 8)
            {
                if (grid[i, j].piece != null)
                {
                    if (grid[i, j].piece.GetColor() == cell.piece.GetColor()) break;
                    else
                    {
                        result.AddLast(grid[i, j]);
                        break;
                    }
                }

                result.AddLast(grid[i, j]);
                i++;
                j++;
            }

            i = cell.I + 1;
            j = cell.J - 1;
            while (i < 8 && j > -1)
            {
                if (grid[i, j].piece != null)
                {
                    if (grid[i, j].piece.GetColor() == cell.piece.GetColor()) break;
                    else
                    {
                        result.AddLast(grid[i, j]);
                        break;
                    }
                }

                result.AddLast(grid[i, j]);
                i++;
                j--;
            }

            i = cell.I - 1;
            j = cell.J + 1;
            while (i > -1 && j < 8)
            {
                if (grid[i, j].piece != null)
                {
                    if (grid[i, j].piece.GetColor() == cell.piece.GetColor()) break;
                    else
                    {
                        result.AddLast(grid[i, j]);
                        break;
                    }
                }

                result.AddLast(grid[i, j]);
                i--;
                j++;
            }
        }

        /// <summary>
        /// Updates the LinkedList result to contain all Cells to which the queen in cell is allowed to move.
        /// </summary>
        public void CheckMovesQueen(Cell cell, LinkedList<Cell> result)
        {
            CheckMovesRook(cell, result);
            CheckMovesBishop(cell, result);
        }

        /// <summary>
        /// Updates the LinkedList result to contain all Cells to which the king in cell is allowed to move.
        /// </summary>
        public void CheckMovesKing(Cell cell, LinkedList<Cell> result)
        {
            LinkedList<Cell> remover = new LinkedList<Cell>();

            //Add spaces king can move to, do not include pieces of the same color.
            for (int i = cell.I - 1; i <= cell.I + 1; i++)
            {
                if (validIndexes(i, cell.J - 1) && ((grid[i, cell.J - 1].piece != null && grid[i, cell.J - 1].piece.GetColor() != GetTurn()) || grid[i, cell.J - 1].piece == null))
                    result.AddLast(grid[i, cell.J - 1]);

                if (validIndexes(i, cell.J) && ((grid[i, cell.J].piece != null && grid[i, cell.J].piece.GetColor() != GetTurn()) || grid[i, cell.J].piece == null))
                    result.AddLast(grid[i, cell.J]);

                if (validIndexes(i, cell.J + 1) && ((grid[i, cell.J + 1].piece != null && grid[i, cell.J + 1].piece.GetColor() != GetTurn()) || grid[i, cell.J + 1].piece == null))
                    result.AddLast(grid[i, cell.J + 1]);

            }

            result.Remove(cell);


            //castling.
            if (cell.piece.GetColor() == Color.White && !castlingHasMoved[0])
            {
                bool clear = true;

                if (grid[7, 1].piece != null || grid[7, 2].piece != null)
                    clear = false;

                if (!castlingHasMoved[2] && clear) result.AddLast(grid[7, cell.J - 2]);

                clear = true;
                if (grid[7, 4].piece != null || grid[7, 5].piece != null || grid[7, 6].piece != null)
                    clear = false;

                if (!castlingHasMoved[3] && clear) result.AddLast(grid[7, cell.J + 2]);
            }

            if (cell.piece.GetColor() == Color.Black && !castlingHasMoved[1])
            {
                bool clear = true;
                if (grid[0, 1].piece != null || grid[0, 2].piece != null)
                    clear = false;

                if (!castlingHasMoved[2] && clear) result.AddLast(grid[0, cell.J - 2]);

                clear = true;
                if (grid[0, 4].piece != null || grid[0, 5].piece != null || grid[0, 6].piece != null)
                    clear = false;

                if (!castlingHasMoved[3] && clear) result.AddLast(grid[0, cell.J + 2]);
            }
        }
        

        /// <summary>
        /// Receives the index [i, j]. Returns true if they exist in grid, else returns false.
        /// </summary>
        private bool validIndexes(int i, int j)
        {
            if (i > -1 && i < 8 && j > -1 && j < 8)
                return true;
            return false;
        }

        /// <summary>
        /// Return true if current player to play is under check. Else returns false.
        /// </summary>
        public bool InCheck(Color color)
        {
            Cell king = FindKing(color);

            LinkedList<Cell> moves = new LinkedList<Cell>();
            // does any of the opponent pices'  GetValidMoves contain the our king\s cell.

            //System.Windows.Forms.MessageBox.Show(king.piece + " " + king.I + " " + king.J);

            foreach (Cell checker in grid)
            {
                if(checker.piece != null && checker.piece.GetColor() != king.piece.GetColor())
                {
                    switch (checker.piece.GetType())
                    {
                        case Type.Pawn:
                            CheckMovesPawn(checker, moves);
                            break;

                        case Type.Knight:
                            CheckMovesKnight(checker, moves);
                            break;

                        case Type.Rook:
                            CheckMovesRook(checker, moves);
                            break;

                        case Type.Bishop:
                            CheckMovesBishop(checker, moves);
                            break;

                        case Type.Queen:
                            CheckMovesQueen(checker, moves);
                            break;

                        case Type.King:
                            CheckMovesKing(checker, moves);
                            break;
                    }

                    if (moves.Contains(king)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a cell with one of the Kings, White if Color is White, and Black if Color is Black.
        /// </summary>
        public Cell FindKing(Color Color)
        {
            foreach(Cell cell in grid)
            {
                if (cell.piece != null && cell.piece.GetType() == Type.King && cell.piece.GetColor() == Color)
                    return cell;
            }

            return null;
        }

        /// <summary>
        /// Return true if current player to play is under checkmate. Else returns false.
        /// </summary>
        public bool InCheckMate(Color color)
        {
            /*if (!IsCheck())
                return false;*/

            // for each king's valid move:
            //     move the king,
            //     check if check
            //     get the king back
            LinkedList<Cell> ValidMovesKing = GetValidMoves(FindKing(color));
            if (ValidMovesKing != null)
            {
                foreach (Cell cell in grid)
                {
                    if (cell.piece != null && cell.piece.GetColor() == color)
                    {
                        if (GetValidMoves(cell) != null)
                            return false;
                    }
                }
            }

            return true;
        }

        public Cell[,] HelpForm()
        {
            return this.grid;
        }
    }

    public class Cell
    {
        public readonly int I, J; // Coordinates of the cell.
        public Piece piece; // The piece that the cell contains.

        public Cell(int I, int J, Piece piece)
        {
            this.I = I;
            this.J = J;
            this.piece = piece;
        }

        public override bool Equals(object obj)
        {
            Cell other = (Cell)obj;
            return other.J == this.J && other.I == this.I;
        }
    }

    public class Piece
    {
        readonly Type Type; // The type of chess piece that this piece is.
        readonly Color Color; // The color to which this piece belongs.

        public Piece(Type Type, Color Color)
        {
            this.Type = Type;
            this.Color = Color;
        }

        /// <summary>
        /// Returns the Type of the current piece.
        /// </summary>
        public Type GetType()
        {
            return this.Type;
        }

        /// <summary>
        /// Returns the Color of the current piece.
        /// </summary>
        public Color GetColor()
        {
            return this.Color;
        }
    }
}

    public enum Color
    {
        White,
        Black
    }

    public enum Type
    {
        King,
        Queen,
        Rook,
        Bishop,
        Knight,
        Pawn
    }
#endregion