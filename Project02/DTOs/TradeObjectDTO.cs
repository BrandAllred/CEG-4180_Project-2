using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters;

namespace Project02.DTOs
{
    /// <summary>
    /// The whole point of this class is to be turned into a JSON string.
    /// </summary>
    [Serializable]
    public class TradeObjectDTO
    {
        public string originalOwner;

        public string name;
    }
}
