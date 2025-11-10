using System;
using System.Collections.Generic;

namespace checkersclaude
{
    public class Move
    {
        public Position From { get; set; }
        public Position To { get; set; }
        public bool IsJump { get; set; }

        // For multi-jumps, store all jumped positions
        public List<Position> JumpedPositions { get; set; }

        // Constructor for normal moves
        public Move(Position from, Position to)
        {
            From = from;
            To = to;
            IsJump = false;
            JumpedPositions = new List<Position>();
        }

        // Constructor for single jump
        public Move(Position from, Position to, bool isJump, Position jumpedPosition)
        {
            From = from;
            To = to;
            IsJump = isJump;
            JumpedPositions = new List<Position>();
            if (jumpedPosition != null)
                JumpedPositions.Add(jumpedPosition);
        }

        // Constructor for multi-jumps
        public Move(Position from, Position to, bool isJump, List<Position> jumpedPositions)
        {
            From = from;
            To = to;
            IsJump = isJump;
            JumpedPositions = jumpedPositions ?? new List<Position>();
        }

        // Add a jumped piece (useful for chaining)
        public void AddJumped(Position pos)
        {
            if (JumpedPositions == null)
                JumpedPositions = new List<Position>();
            JumpedPositions.Add(pos);
        }
    }
}
