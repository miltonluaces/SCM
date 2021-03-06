#region 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace AibuSet {

    internal interface ITTItem {
        string GetName();
        void SetOrderDays(bool[] orderDays);
        bool[] GetOrderDays();
        void SetWeeklyFreq(int weeklyFreq);
        int GetWeeklyFreq();
    }
}
