using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    public class AI
    {
        Color Color;
        Logic game;

        public Move ChooseMove()
        {
            var root = buildTree(2, game.GetTurn(), this.CloneGame(game));

            Console.WriteLine(game); //ERASE
            // now we have to find the son with max score
            // we can use the fact the root's score == max score of its sons
            // so we will iterate over sons to find a son with score eq to ours
            foreach (var son in root.sons)
            {
                if (son.score == root.score)
                {
                    game.NextTurn(); //DEBUGGG
                    return son.whatGotUsHere;
                }
            }

            Color OpponentColor = Color == Color.White ? Color.Black : Color.White;
            if (game.InCheckMate(OpponentColor))
                System.Windows.Forms.MessageBox.Show("Checkmate!!! " + Color + " is the winner!");
            else
                System.Windows.Forms.MessageBox.Show("Checkmate!!! " + OpponentColor + " is the winner!");

            return null;
            throw new Exception("ai did not find any move to play. at all"); //Unreachable Code?
        }

        public AI(Color color, Logic game)
        {
            this.Color = color;
            this.game = game;
        }

        private Piece eaten;
        private Cell whatIMoved;

        /// <summary>
        /// Assess the current state of the game, in favor of a certain color (whoIsToPlay).
        /// Returns an integer between -100 and 100, ranking how good the state of the game is for specified color.
        /// </summary>
        private int assess(Logic game, Color whoIsToPlay) // pretty much heuristic
        {
            // todo return a number from -inf(we lose) to inf(we win)
            // optional aspects:
            // are we (or they) close to the centre
            // are any piece lost
            // is the king vulnerable
            int centerControl, piecesLost, kingVulnerability, pieceEaten;
            const int a = 20, b = 12, c = 12, d = 20;
            //First we'll check whoIsToPlay's control over the center.
            //1: How many pieces from each side physically occupy the center?
            int countWhite = occupyCenter(game, Color.White);
            int countBlack = occupyCenter(game, Color.White);

            //2: How many pieces 'threaten' how many squares in the center?
            threatenCenter(game, game.grid[3, 3], countWhite, countBlack);
            threatenCenter(game, game.grid[3, 4], countWhite, countBlack);
            threatenCenter(game, game.grid[4, 3], countWhite, countBlack);
            threatenCenter(game, game.grid[4, 4], countWhite, countBlack);

            centerControl = whoIsToPlay == Color.White ? a * (countWhite - countBlack) : a * (countBlack - countWhite);


            //Next, we will compare our losses against the opponent's.
            countWhite = 0;
            countBlack = 0;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (game.grid[i, j].piece != null)
                    {
                        if (game.grid[i, j].piece.GetColor() == Color.White)
                            countWhite++;
                        else
                            countBlack++;
                    }
                }
            }

            piecesLost = whoIsToPlay == Color.White ? b * (countWhite - countBlack) : b * (countBlack - countWhite);


            //Now, we will assess both kings' vulnerability.
            //Can we be checkmated next turn?
            int MyVul = 0, OpponentVul = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (game.grid[i, j].piece != null)
                    {
                        if (game.grid[i, j].piece.GetColor() != whoIsToPlay)
                        {
                            if (DistanceFromKing(game, whoIsToPlay, game.grid[i, j]) < 4)
                                MyVul += 2;
                            if (DistanceFromKing(game, whoIsToPlay == Color.White ? Color.Black : Color.White, game.grid[i, j]) > 4)
                                OpponentVul++;
                        }

                        else
                        {
                            if (DistanceFromKing(game, whoIsToPlay == Color.White ? Color.Black : Color.White, game.grid[i, j]) < 4)
                                OpponentVul += 2;
                            if (DistanceFromKing(game, whoIsToPlay, game.grid[i, j]) > 4)
                                MyVul++;
                        }
                    }
                }
            }

            kingVulnerability = OpponentVul - MyVul;

            //Finally, we will assess the pieces we can eat, and our pieces, which can be eaten.
            int eatenValue, whatAteMeValue, threatValue;

            if (eaten != null)
            {
                eatenValue = PieceValue(eaten.GetType());
                whatAteMeValue = PieceValue(whatIMoved.piece.GetType());
                threatValue = ThreatValue();

                pieceEaten = eatenValue - whatAteMeValue - threatValue; //If value of eaten piece is more than what ate him,
                                                                        //that is good for current player. If threat on current
                                                                        //player's piece is positive, it is bad because the player is
                                                                        //threatened by lesser piece.
            }

            else
            {
                whatAteMeValue = PieceValue(whatIMoved.piece.GetType());
                threatValue = ThreatValue();

                pieceEaten = whatAteMeValue - threatValue;
            }


            return a * centerControl + b * piecesLost + c * kingVulnerability + d * pieceEaten;
        }

        //Receives a game and a color. Returns how many pieces of that color physically occupy the center.
        private int occupyCenter(Logic game, Color Color)
        {
            int count = 0;
            if (game.grid[3, 3].piece != null && game.grid[3, 3].piece.GetColor() == Color)
                count++;
            if (game.grid[4, 4].piece != null && game.grid[4, 4].piece.GetColor() == Color)
                count++;
            if (game.grid[3, 4].piece != null && game.grid[3, 4].piece.GetColor() == Color)
                count++;
            if (game.grid[4, 3].piece != null && game.grid[4, 3].piece.GetColor() == Color)
                count++;
            return count;

        }

        private void threatenCenter(Logic game, Cell cell, int countWhite, int countBlack)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (game.grid[i, j].piece != null)
                    {
                        if (game.grid[i, j].piece.GetColor() == Color.White && game.GetValidMoves(game.grid[i, j]).Contains(cell))
                            countWhite++;
                        else if (game.grid[i, j].piece.GetColor() == Color.Black && game.GetValidMoves(game.grid[i, j]).Contains(cell))
                            countBlack++;
                    }
                }
            }
        }

        private double DistanceFromKing(Logic game, Color Color, Cell BeingChecked)
        {
            Cell King = game.FindKing(Color);
            return Math.Sqrt(Math.Pow(BeingChecked.I + King.I, 2) + Math.Pow(BeingChecked.J + King.J, 2));
        }

        private bool hasValidMoves(Logic state, Color WhoIsToPlay)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (state.grid[i, j].piece != null && state.grid[i, j].piece.GetColor() == WhoIsToPlay && state.GetValidMoves(state.grid[i, j]).Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public int PieceValue(Type type)
        {
            int queen = 9, rook = 5, knight = 3, bishop = 3, pawn = 1;

            switch (type)
            {
                case Type.Queen:
                    return queen;

                case Type.Rook:
                    return rook;

                case Type.Knight:
                    return knight;

                case Type.Bishop:
                    return bishop;

                case Type.Pawn:
                    return pawn;
            }

            return 0; //Code should never reach here.
        }

        public int ThreatValue()
        {
            int maxThreat = -9;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (game.grid[i, j].piece != null && game.GetValidMoves(game.grid[i, j]).Contains(whatIMoved) && PieceValue(game.grid[i, j].piece.GetType()) - PieceValue(whatIMoved.piece.GetType()) > maxThreat)
                        maxThreat = PieceValue(game.grid[i, j].piece.GetType()) - PieceValue(whatIMoved.piece.GetType());


                }
            }

            return maxThreat;
        }


        int idFactory = 0;

        TreeNode buildTree(int levelsLeftToBuild, Color WhoIsToPlay, Logic state) // the function's final result would be the tree's root

        {
            TreeNode resultNode = new TreeNode();

            bool hasAnyValidMoves; // todo check if checkmate or stalemate.
            hasAnyValidMoves = hasValidMoves(state, WhoIsToPlay);

            if (levelsLeftToBuild > 0 && hasAnyValidMoves) // recursion end
            {
                /* for each possible move, named m:
                 *     clone the current game (deep copy)
                 *     in the clone, play m. alternatively - remember what you have done to reמשהו it
                 *     son (with all its descendants along) = buildTree(levels - 1, opposite_color)
                 *     result.sons.add(son);
                */

                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (state.grid[i, j].piece != null && state.grid[i, j].piece.GetColor() == WhoIsToPlay)
                        {
                            // Console.WriteLine(idFactory + " before recur:");
                            // Console.WriteLine(state);

                            LinkedList<Cell> ValidMoves = state.GetValidMoves(state.grid[i, j]);

                            Piece movingPiece = state.grid[i, j].piece;
                            state.grid[i, j].piece = null;
                            Piece occupied_piece;// To save any existing pieces in the current move of the current piece, from being deleted.

                            foreach (var target_cell in ValidMoves)
                            {
                                occupied_piece = target_cell.piece;
                                target_cell.piece = movingPiece;

                                if (occupied_piece != null)
                                    eaten = occupied_piece;

                                else
                                    eaten = null;

                                whatIMoved = target_cell;

                                var son = buildTree(levelsLeftToBuild - 1, WhoIsToPlay == Color.Black ? Color.White : Color.Black, state);
                                son.whatGotUsHere = new Move(state.grid[i, j], target_cell);
                                resultNode.sons.Add(son);

                                target_cell.piece = occupied_piece;
                            }
                            state.grid[i, j].piece = movingPiece;

                            // Console.WriteLine(idFactory + " after recur:");
                            // Console.WriteLine(state);

                            idFactory++;
                        }
                    }
                }
                // now after recur is done, 

                if (WhoIsToPlay == this.Color)
                    resultNode.score = resultNode.GetMaxSonScore();
                else
                    resultNode.score = resultNode.GetMinSonScore();
            }
            else // we are a leaf!
            {
                if (!hasAnyValidMoves)
                {
                    if (state.InCheck(WhoIsToPlay))
                        resultNode.score = WhoIsToPlay == this.Color ? int.MinValue : int.MaxValue; // won or lost
                    else
                        resultNode.score = 0; // stale, mate!
                }
                else
                    resultNode.score = assess(state, WhoIsToPlay);

            }

            return resultNode;
        }

        public Logic CloneGame(Logic game)
        {
            Cell[,] Grid = new Cell[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (game.grid[i, j].piece != null)
                        Grid[i, j] = new Cell(i, j, new Piece(game.grid[i, j].piece.GetType(), game.grid[i, j].piece.GetColor()));

                    else
                        Grid[i, j] = new Cell(i, j, null);

                }
            }

            Logic Clone = new Logic();
            Clone.grid = Grid;
            if (game.GetTurn() != Color.White)
                game.NextTurn();

            return Clone;
        }
    }

    public class Move
    {
        readonly public Cell from, to;

        public Move(Cell from, Cell to)
        {
            this.from = from;
            this.to = to;
        }
    }

    public class TreeNode
    {
        public List<TreeNode> sons = new List<TreeNode>();
        public int score;
        public Move whatGotUsHere = null;

        public int GetMaxSonScore()
        {
            int MaxScore = int.MinValue;

            foreach (TreeNode son in this.sons)
            {
                if (MaxScore < son.score)
                {
                    MaxScore = son.score;
                }
            }

            return MaxScore;
        }

        public int GetMinSonScore()
        {
            int MinScore = int.MaxValue; // todo make it like the above function
            foreach (TreeNode son in this.sons)
            {
                if (MinScore > son.score)
                {
                    MinScore = son.score;
                }
            }

            return MinScore;
        }

    }
}
