#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace MachineLearning {

    internal static class SVMExtensions  {

        internal static void SwapIndex<T>(this T[] list, int i, int j)  {
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}
