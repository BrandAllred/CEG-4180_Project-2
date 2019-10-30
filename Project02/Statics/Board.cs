using Project02.Classes.Abstracts;
using Project02.Classes.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project_02.Statics
{
    public static class Board
    {
        // Why any of these numbers? Magic.
        public const int BOARD_MAX_WIDTH = 5;
        public const int BOARD_MAX_HEIGHT = 5;
        public const int BOARD_MIN_WIDTH = 1;
        public const int BOARD_MIN_HEIGHT = 1;
        public const int AMOUNT_OF_SHIPS = 5;

        private static Mutex queryingActiveShips = new Mutex();
        private static Dictionary<string, SpaceShip> activeShips = new Dictionary<string, SpaceShip>();

        private static Mutex queryingShipPositions = new Mutex();
        private static Dictionary<Tuple<int, int>, SpaceShip> shipPositions = new Dictionary<Tuple<int, int>, SpaceShip>();
        
        public static Random randomNumberGenerator = new Random();

        private static bool shipsCanMakeActions = true;

        public static bool TryAddShip(SpaceShip ship)
        {
            if (!shipsCanMakeActions)
            {
                throw new ShipDestroyedException();
            }

            try
            {
                queryingShipPositions.WaitOne();

                shipPositions.Add(ship.location, ship);

                queryingShipPositions.ReleaseMutex();
            }
            catch (ArgumentException)
            {
                queryingShipPositions.ReleaseMutex();

                return false;
            }

            queryingActiveShips.WaitOne();

            // Since the GUIDs are unique, adding them shouldn't be a problem
            // Therefore, there is no try catch block.
            activeShips.Add(ship.name, ship);

            queryingActiveShips.ReleaseMutex();

            return true;
        }

        public static bool TryMoveShip(SpaceShip ship, Tuple<int, int> targetLocation, out SpaceShip otherShip)
        {
            try
            {
                otherShip = null;

                if (!shipsCanMakeActions)
                {
                    return false;
                }

                // Essentially a queue that the threads will voluntarilly leave.
                // This is the enqueue statment.
                if (queryingShipPositions.WaitOne(250))
                {
                    // This will cause an exception if there is already a ship at the requested location.
                    shipPositions.Add(targetLocation, ship);

                    shipPositions.Remove(ship.location);
                    // This is the pop statement.
                    queryingShipPositions.ReleaseMutex();

                    ship.location = targetLocation;
                }
            }
            catch (ArgumentException)
            {
                otherShip = shipPositions[targetLocation];

                queryingShipPositions.ReleaseMutex();

                return false;
            }

            return true;
        }

        public static SpaceShip GetSpaceShip(string shipName)
        {
            SpaceShip shipToReturn = null;

            try
            {
                queryingActiveShips.WaitOne();

                shipToReturn = activeShips[shipName];

                queryingActiveShips.ReleaseMutex();
            }
            catch (ArgumentException e)
            {
                queryingActiveShips.ReleaseMutex();
                // Close the mutex and safely indicate that there was an error.
                throw e;
            }
            catch (Exception)
            {
                queryingActiveShips.ReleaseMutex();
            }

            return shipToReturn;
        }

        public static void DestroySpaceShip(SpaceShip ship)
        {
            queryingActiveShips.WaitOne();

            activeShips.Remove(ship.name);

            queryingActiveShips.ReleaseMutex();

            queryingShipPositions.WaitOne();

            shipPositions.Remove(ship.location);

            queryingShipPositions.ReleaseMutex();
        }

        public static void StopExecution()
        {
            shipsCanMakeActions = false;
            List<SpaceShip> list = activeShips.Values.ToList();

            foreach (SpaceShip ship in list)
            {
                ship.DestroyShip();
            }
        }

        public static Dictionary<Tuple<int, int>, SpaceShip> DisplayShips()
        {
            Dictionary<Tuple<int, int>, SpaceShip> returnDictionary = new Dictionary<Tuple<int, int>, SpaceShip>();

            queryingShipPositions.WaitOne();
            
            foreach (Tuple<int, int> position in shipPositions.Keys)
            {
                returnDictionary.Add(position, shipPositions[position]);
            }

            queryingShipPositions.ReleaseMutex();

            return returnDictionary;
        }

        public static int ActiveShipCount()
        {
            queryingActiveShips.WaitOne();

            int shipCount = activeShips.Count;

            queryingActiveShips.ReleaseMutex();

            return shipCount;
        }
    }
}
