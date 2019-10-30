using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project02.DTOs
{
    [Serializable]
    public class MessageDTO
    {
        public int intent;

        public string fromShipName;

        public TradeObjectDTO tradeObject;
    }
}
