using Project02.Classes.Abstracts;
using Project02.DTOs;

namespace Project02.Classes
{
    class PirateShip : SpaceShip
    {
        public PirateShip()
        {
            InteractWithOtherShip += ReactionToShip;
            // Instead of trading, we just take the thing that was traded, and stop interacting.
            // Thus effectivly stealing from them.
            reactionDictionary[Intents.Trade] = StopInteractionDelegate;
        }

        public MessageDTO ReactionToShip(string otherShipName)
        {
            return new MessageDTO()
            {
                intent = (int)Intents.Steal,
                fromShipName = name
            };
        }

        /// <summary>
        /// If the other ship wants to stop interacting with this ship,
        /// Steal from it.
        /// But if it didn't send a trade object, destroy it instead.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public override MessageDTO StopInteractionDelegate(MessageDTO dto)
        {
            if (dto.tradeObject == null)
            {
                // From Russia, with love.
                return new MessageDTO()
                {
                    intent = (int)Intents.Destroy,
                    fromShipName = name
                };
            }

            return base.StopInteractionDelegate(dto);
        }
    }
}