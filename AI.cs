using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    public class AI
    {
        readonly Color Color;

        // Tree tree; // previously calculated...

        public Move ChooseMove(Logic game)
        {
            // ...
            // game.GetValidMoves
            return null;
        }
    }

    public class Move
    {
        readonly Cell from, to;
        // todo simple constructor
    }
}
