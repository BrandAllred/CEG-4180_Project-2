using Newtonsoft.Json;
using Project_02.Statics;
using Project02.Classes.Exceptions;
using Project02.DTOs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Project02.Classes.Abstracts
{
    /// <summary>
    /// This is the class that all other space ships are modeled off of.
    /// This class handles all of the interaction with the board and other ships.
    /// </summary>
    public abstract class SpaceShip
    {

        #region Events and delegates

        /// <summary>
        /// This signals that the ship has ran into another ship.
        /// And that the general interaction is needed.
        /// </summary>
        /// <param name="otherShipName"></param>
        /// <returns></returns>
        public delegate MessageDTO MessageToOtherShip(string otherShipName);
        public MessageToOtherShip InteractWithOtherShip;

        public delegate MessageDTO IntentInteractions(MessageDTO dto);

        //private IntentInteractions BaseStopInteractions = 

        #endregion

        #region variables

        public enum ShipStatuses { Working, Waiting, Destroyed }
        public ShipStatuses shipStatus = new ShipStatuses();

        /// <summary>
        /// Can trade back and foreth for as much as they want.
        /// One ship can steal from the ship as much as they want.
        /// One ship can Destroy the other ship once.
        /// And the ships can stop interacting at any point.
        /// </summary>
        public enum Intents { Trade, Steal, Destroy, StopInteraction }

        /// <summary>
        /// This is a concurrent queue of json strings.
        /// </summary>
        public ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();

        public Tuple<int, int> location;

        /// <summary>
        /// Objects to trade/take between ships.
        /// </summary>
        public Queue<string> tradeDtos = new Queue<string>();

        public string name;
        private string shipThatWasMessaged;

        // If you spoof a switch case... is it still a switch case?
        protected Dictionary<Intents, IntentInteractions> reactionDictionary;
        // Cause I will argue that this is essentially equivalent to a switch case for C#.

        // This may complain that this should be read-only.
        // No, I don't think this should be read-only.
        private Thread thread;

        #endregion

        #region constructor

        /// <summary>
        /// Base constructor to find out where the ship is going to spawn.
        /// </summary>
        public SpaceShip()
        {
            name = Guid.NewGuid().ToString();

            int y = Board.randomNumberGenerator.Next(Board.BOARD_MIN_HEIGHT, Board.BOARD_MAX_HEIGHT);
            int x = Board.randomNumberGenerator.Next(Board.BOARD_MIN_WIDTH, Board.BOARD_MAX_WIDTH);

            location = new Tuple<int, int>(x, y);

            try
            {
                while (!Board.TryAddShip(this))
                {
                    if (x++ > Board.BOARD_MAX_WIDTH)
                    {
                        x = Board.BOARD_MIN_WIDTH;
                    }

                    if (y++ > Board.BOARD_MAX_HEIGHT)
                    {
                        y = Board.BOARD_MIN_HEIGHT;
                    }

                    location = new Tuple<int, int>(x, y);
                }
            }
            catch (ShipDestroyedException)
            {
                // If a new ship cannot be made, the ShipDestroyedException gets thrown.
                // Meaning that the ship cannot be made.
                return;
            }
            
            // This fancy switch needs to be dynamically set.
            reactionDictionary = new Dictionary<Intents, IntentInteractions>()
            {
                {Intents.StopInteraction, new IntentInteractions(StopInteractionDelegate)},
                {Intents.Destroy, new IntentInteractions(DestroyDelegate)},
                {Intents.Steal, new IntentInteractions(StealDelegate)},
                {Intents.Trade, new IntentInteractions(TradeDelegate)}
            };

            // Giving each ship three trade objects.
            tradeDtos.Enqueue(JsonConvert.SerializeObject(new TradeObjectDTO()
            {
                name = Guid.NewGuid().ToString(),
                originalOwner = name
            }));

            tradeDtos.Enqueue(JsonConvert.SerializeObject(new TradeObjectDTO()
            {
                name = Guid.NewGuid().ToString(),
                originalOwner = name
            }));

            tradeDtos.Enqueue(JsonConvert.SerializeObject(new TradeObjectDTO()
            {
                name = Guid.NewGuid().ToString(),
                originalOwner = name
            }));

            thread = new Thread(new ThreadStart(Think));
            thread.Start();
        }

        #endregion

        #region General thread loop

        /// <summary>
        /// Main loop for each thread.
        /// This method will decide how each ship responds.
        /// And once the thread exits this thread, the thread ends.
        /// </summary>
        public void Think()
        {
            while (shipStatus != ShipStatuses.Destroyed)
            {
                // If we need to interact, interact.
                if (messageQueue.TryDequeue(out string message))
                {
                    // We busy wait so that the responses can get proccessed fully.
                    shipStatus = ShipStatuses.Waiting;

                    MessageDTO messageDto = JsonConvert.DeserializeObject<MessageDTO>(message);
                    RespondToOtherShipAction(messageDto);
                }
                else if (shipStatus != ShipStatuses.Waiting) 
                {
                    Move();
                }
            }
        }

        #endregion

        #region movement methods

        /// <summary>
        /// Decide where to move, and call the interaction method if need be.
        /// </summary>
        private void Move()
        {
            // Note, the ship can choose to stay still.
            int verticalMovement = Board.randomNumberGenerator.Next(-1, 1);
            int horizontalMovement = Board.randomNumberGenerator.Next(-1, 1);

            int finalXLocation = location.Item1 + horizontalMovement;
            int finalYLocation = location.Item2 + verticalMovement;
            

            if (finalXLocation > Board.BOARD_MAX_WIDTH)
            {
                finalXLocation = Board.BOARD_MIN_WIDTH;
            }
            else if (finalXLocation < Board.BOARD_MIN_WIDTH)
            {
                finalXLocation = Board.BOARD_MAX_WIDTH;
            }

            if (finalYLocation > Board.BOARD_MAX_HEIGHT)
            {
                finalYLocation = Board.BOARD_MIN_HEIGHT;
            }
            else if (finalYLocation < Board.BOARD_MIN_HEIGHT)
            {
                finalYLocation = Board.BOARD_MAX_HEIGHT;
            }

            // Fresh reference, so we can maintain the previous location.
            Tuple<int, int> targetLocation = new Tuple<int, int>(finalXLocation, finalYLocation);

            // Since the movement is handled in the TryMoveShip method, we can only care about
            // when it is false. Because we will have to do something else if the movement is
            // false.
            if (!Board.TryMoveShip(this, targetLocation, out SpaceShip otherShip))
            {
                SendOtherShipAction(otherShip);
            }
        }

        #endregion

        #region ship interactions
        
        /// <summary>
        /// THIS IS ONE OF THE MESSAGE DESIGN PATTERN METHODS.
        /// This method sends the message to the other thread to be dealt with.
        /// </summary>
        /// <param name="otherShip"></param>
        private void SendOtherShipAction(SpaceShip otherShip)
        {
            try
            {
                MessageDTO messageDto = InteractWithOtherShip?.Invoke(otherShip.name) ?? new MessageDTO() { intent = -1 };

                if (messageDto.intent != -1)
                {
                    shipThatWasMessaged = otherShip.name;
                    string message = JsonConvert.SerializeObject(messageDto);
                    otherShip.RecieveOtherShipAction(message);
                    // The ship waiting on the other ship to respond will busy wait to deal with any interactions.
                    shipStatus = ShipStatuses.Waiting;
                }
            }
            catch (NullReferenceException)
            {
                // During the exiting of the code, the interactions will become null.
                // Thus there will be a null referece exception thrown.
                // This is because a thread is relying on a resource that is becoming removed when exiting.
                return;
            }
        }

        /// <summary>
        /// THIS IS ONE OF THE MESSAGE DESIGN PATTERN METHODS.
        /// It would have been better to handle the interactions of threads
        /// through the Board static class. So that way this program could be
        /// split into different threads on a computer.
        /// Buuuuuuut I didn't. I'm sorry.
        /// </summary>
        /// <param name="message"></param>
        private void RecieveOtherShipAction(string message)
        {
            messageQueue.Enqueue(message);
        }

        /// <summary>
        /// THIS IS ONE OF THE MESSAGE DESIGN PATTERN METHODS.
        /// This sends the response of the ship to the thread that concerns the other ship.
        /// </summary>
        /// <param name="messageDto">
        /// message to respond to.
        /// </param>
        private void RespondToOtherShipAction(MessageDTO messageDto)
        {
            try
            {
                SpaceShip otherShip = Board.GetSpaceShip(messageDto.fromShipName);

                if (otherShip == null)
                {
                    return;
                }

                if (shipThatWasMessaged == otherShip.name)
                {
                    shipStatus = ShipStatuses.Working;
                }

                if (reactionDictionary.Keys.Contains((Intents)messageDto.intent))
                {
                    MessageDTO responseDto = reactionDictionary[(Intents)messageDto.intent]?.Invoke(messageDto) ?? new MessageDTO() { intent = -1 };

                    if (responseDto.intent == -1 || responseDto.intent == (int)Intents.Destroy)
                    {
                        shipStatus = ShipStatuses.Working;
                    }

                    string responseString = JsonConvert.SerializeObject(responseDto);

                    otherShip.RecieveOtherShipAction(responseString);
                }
                else if (messageDto.intent == -1)
                {
                    shipStatus = ShipStatuses.Working;
                }
                else
                {
                    string noInteractionMessage = JsonConvert.SerializeObject(new MessageDTO()
                    {
                        intent = (int)Intents.StopInteraction,
                        fromShipName = name,
                        tradeObject = null
                    });
                    otherShip.RecieveOtherShipAction(noInteractionMessage);
                }
            }
            catch (ArgumentException)
            {// safely catch if we can no longer respond to the other ship. So we can carry onwards.
                Console.WriteLine("The ship " + messageDto.fromShipName + " was not found when we went to respond to it.");
            }
            catch (ShipDestroyedException)
            {
                Console.WriteLine("The ship " + name + " has been destroyed!");
            }
        }

        #endregion

        #region Delegation methods

        /// <summary>
        /// Base method to respond to the other ship wanting to stop reacting.
        /// </summary>
        /// <param name="dto">
        /// message stating that the other ship wants to stop interacting.
        /// </param>
        /// <returns>
        /// A message to stop sending the other ship messages.
        /// </returns>
        public virtual MessageDTO StopInteractionDelegate(MessageDTO dto)
        {
            if (dto.tradeObject != null)
            {
                tradeDtos.Enqueue(JsonConvert.SerializeObject(dto.tradeObject));
            }
            
            return new MessageDTO()
            {
                intent = -1,
                fromShipName = name
            };
        }

        /// <summary>
        /// Base method to respond to the other ship wanting to Trade with this ship.
        /// </summary>
        /// <param name="dto">
        /// the message to respond to.
        /// </param>
        /// <returns>
        /// The command to send back to the other ship.
        /// </returns>
        public virtual MessageDTO TradeDelegate(MessageDTO dto)
        {
            MessageDTO responseDto = new MessageDTO()
            {
                intent = (int)Intents.StopInteraction,
                fromShipName = name
            };

            if (dto.tradeObject != null && tradeDtos.Count != 0)
            {
                tradeDtos.Enqueue(JsonConvert.SerializeObject(dto.tradeObject));
                responseDto.tradeObject = JsonConvert.DeserializeObject<TradeObjectDTO>(tradeDtos.Dequeue());
            }

            return responseDto;
        }

        /// <summary>
        /// Base method to respond to the other ship wanting to steal from this ship.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public virtual MessageDTO StealDelegate(MessageDTO dto)
        {
            MessageDTO responseDto = new MessageDTO()
            {
                intent = (int)Intents.StopInteraction,
                fromShipName = name,
            };

            if (tradeDtos.Count != 0)
            {
                responseDto.tradeObject = JsonConvert.DeserializeObject<TradeObjectDTO>(tradeDtos.Dequeue());
            }

            return responseDto;
        }

        /// <summary>
        /// Base method to respond to the other ship wanting to destroy this ship.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public virtual MessageDTO DestroyDelegate(MessageDTO dto)
        {
            DestroyShip();
            throw new ShipDestroyedException();
        }

        #endregion

        /// <summary>
        /// Method to handle the destruction of this ship.
        /// </summary>
        public void DestroyShip()
        {
            Board.DestroySpaceShip(this);
            shipStatus = ShipStatuses.Destroyed;
        }
    }
}