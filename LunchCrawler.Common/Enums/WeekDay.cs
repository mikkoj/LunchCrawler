using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunchCrawler.Common.Enums
{
    /// <summary>
    /// TODO: vaikka attribuuteilla linkit regexpiin?
    /// </summary>
    [Serializable]
    public enum WeekDay
    {
        Unknown = 0,

        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
        Sunday = 7
    }
}
