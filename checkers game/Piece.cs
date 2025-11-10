using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace checkersclaude
{
    public class Piece
    {
        public PieceColor Color { get; set; }
        public PieceType Type { get; set; }
        public Position Position { get; set; }

        public Piece(PieceColor color, Position position)
        {
            Color = color;
            Type = PieceType.Regular;
            Position = position;
        }

        public void PromoteToKing()
        {
            Type = PieceType.King;

      
        }

        public bool CanMoveInDirection(int rowDirection)
        {
            if (Type == PieceType.King)
                return true;

            // Red pieces move down (positive row direction)
            if (Color == PieceColor.Red && rowDirection > 0)
                return true;

            // Black pieces move up (negative row direction)
            if (Color == PieceColor.Black && rowDirection < 0)
                return true;

            return false;
        }
    }
}