using Project02.Classes.Abstracts;
using Project02.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project02.Classes
{
    class FighterShip : SpaceShip
    {
        public FighterShip()
        {
            InteractWithOtherShip += ReactionToOtherShip;
        }

        public MessageDTO ReactionToOtherShip(string otherShipName)
        {
            return new MessageDTO()
            {
                intent = (int)Intents.Destroy,
                fromShipName = name
            };
        }

        /// <summary>
        /// Instead of being stolen from, this ship destroys the other ship.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public override MessageDTO StealDelegate(MessageDTO dto)
        {
            return new MessageDTO()
            {
                fromShipName = name,
                intent = (int)Intents.Destroy
            };
        }
    }
}
