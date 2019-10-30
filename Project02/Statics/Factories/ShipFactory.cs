using Project_02.Statics;
using Project02.Classes;
using Project02.Classes.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_01.Statics.Factories
{
    static class ShipFactory
    {
        /// <summary>
        /// Add new worm types to list to have them spawn.
        /// </summary>
        private static List<Type> shipList = new List<Type>()
        {
            typeof(PirateShip),
            typeof(TradeShip),
            typeof(FighterShip)
        };

        /// <summary>
        /// Create a new instance of a class that inherites the AbstractWorm class.
        /// </summary>
        /// <returns></returns>
        public static SpaceShip CreateShip()
        {
            return Activator.CreateInstance(shipList[Board.randomNumberGenerator.Next(shipList.Count() - 1)]) as SpaceShip;
        }

        /// <summary>
        /// Generates the minnimum amount of worms needed.
        /// </summary>
        public static void GenerateShips()
        {
            for (int i = Board.ActiveShipCount(); i < Board.AMOUNT_OF_SHIPS; i++)
            {
                CreateShip();
            }
        }
    }
}
