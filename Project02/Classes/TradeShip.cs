using Newtonsoft.Json;
using Project02.Classes.Abstracts;
using Project02.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project02.Classes
{
    class TradeShip : SpaceShip
    {
        public TradeShip()
        {

        }

        public MessageDTO ReactionToShip(string otherShipName)
        {
            if (tradeDtos.Count != 0)
            {
                return new MessageDTO()
                {
                    intent = (int)Intents.Trade,
                    fromShipName = name,
                    tradeObject = JsonConvert.DeserializeObject<TradeObjectDTO>(tradeDtos.Dequeue())
                };
            }
            // Effectivly an else.
            return new MessageDTO() 
            {
                intent = (int)Intents.Steal,
                fromShipName = name
            };
        }
    }
}
