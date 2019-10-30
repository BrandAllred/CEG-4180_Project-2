using Project_01.Statics.Factories;
using Project_02.Statics;
using Project02.Classes.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project02
{
    class Program
    {
        private enum DisplayEnum { Display, No, Quit }
        private static string GetBoardStateStr = "b";
        private static string ExitProgram = "q";

        static void Main(string[] args)
        {
            DisplayEnum inputResponse;

            ShipFactory.GenerateShips();

            while (DisplayEnum.Quit != (inputResponse = DisplayMenu()))
            {
                if (DisplayEnum.Display == inputResponse)
                {
                    DisplayShips();
                }

                ShipFactory.GenerateShips();
            }

            Board.StopExecution();

            return;
        }

        private static DisplayEnum DisplayMenu()
        {
            Console.WriteLine("To stop the execution enter Q/q.");
            Console.WriteLine("To get a readout of the current board state, enter B/b.");

            string input = Console.ReadLine().ToLower();

            if (GetBoardStateStr == input)
            {
                return DisplayEnum.Display;
            }
            else if (ExitProgram == input)
            {
                return DisplayEnum.Quit;
            }
            else
            {
                return DisplayEnum.No;
            }
        }

        private static void DisplayShips()
        {
            Dictionary<Tuple<int, int>, SpaceShip> positionsAndShips = Board.DisplayShips();
            Dictionary<Tuple<int, int>, int> shipNumbers = new Dictionary<Tuple<int, int>, int>();
            Tuple<int, int> currentPosition;

            foreach (Tuple<int, int> location in positionsAndShips.Keys)
            {
                shipNumbers.Add(location, shipNumbers.Count + 1);
                Console.WriteLine("The ship numebr: " + shipNumbers[location] + ":\n" + 
                    "\tShip name: " + positionsAndShips[location].name + ".\n" +
                    "\tShip location: " + location + ".\n" +
                    "\tNumber of trade objects: " + positionsAndShips[location].tradeDtos.Count
                    );
            }

            StringBuilder stringBuilder = new StringBuilder();

            for (int y = Board.BOARD_MIN_HEIGHT; y <= Board.BOARD_MAX_HEIGHT; y++)
            {
                for (int x = Board.BOARD_MIN_WIDTH; x <= Board.BOARD_MAX_WIDTH; x++)
                {
                    currentPosition = new Tuple<int, int>(x, y);

                    if (positionsAndShips.ContainsKey(currentPosition))
                    {
                        stringBuilder.Append(shipNumbers[currentPosition]);
                    }
                    else
                    {
                        stringBuilder.Append('.');
                    }
                }

                Console.WriteLine(stringBuilder.ToString());
                stringBuilder.Clear();
            }
        }
    }
}
