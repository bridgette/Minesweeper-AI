using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperAI
{
    /// <summary>
    /// Represents the Minesweeper Board and squares on it
    /// </summary>
    public class GameLogic
    {
        public enum SquareStates { Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Unknown, Flagged, Question, Bomb }

        public GameLogic(int width, int height)
        {
            Square[,] BoardArray = new Square[width,height];
        }

        public int width { get; set; }
        public int height { get; set; }

        public class Square
        {
            public SquareStates square { get; set; }
        }
    }


}
