#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Maths {

    internal class Combinatory {

        Random rand;

        #region Constructor

        internal Combinatory()
        {
            rand = new Random();
        }

        #endregion

        #region Basic Combinatory

        internal double Combinations(int n, int g)
        {
            return Factorial(n) / (Factorial(g) * Factorial(n - g));
        }

        internal double Permutations(int n) { return Factorial(n); }

        internal double Factorial(int n)
        {
            if (n < 0) { throw new Exception("n must not be less than zero"); }
            if (n == 0) { return 1; }
            if (n == 1) { return 1; }
            else { return n * Factorial(n - 1); }
        }

        #endregion

        #region Sums Combinatory

        #region Sums Combinations

        internal List<int[]> SumsCombinations(List<int> values, int n, int k, bool order)
        {
            if (order) { values.Sort(); }

            List<int[]> sumsCombinations = new List<int[]>();
            int[] comb = new int[n];
            SumsCombinations(sumsCombinations, values, n, comb, 0, 0, k);
            return sumsCombinations;
        }

        private void SumsCombinations(List<int[]> sumsCombs, List<int> values, int n, int[] comb, int top, int actSum, int sum)
        {

            if (sum < values[0] * comb.Length || sum > values[values.Count - 1] * comb.Length) { return; }

            if (top == n - 1)
            {
                if (actSum + values[0] <= sum && actSum + values[values.Count - 1] >= sum)
                {
                    comb[top] = sum - actSum;
                    int[] clone = new int[n];
                    comb.CopyTo(clone, 0);
                    sumsCombs.Add(clone);
                }
            }
            else
            {
                for (int i = 0; i < values.Count; i++)
                {
                    comb[top] = values[i];
                    if ((n - top - 1) * values[0] <= sum - actSum - values[i] && (n - top - 1) * values[values.Count - 1] >= sum - actSum - values[i])
                    {
                        SumsCombinations(sumsCombs, values, n, comb, top + 1, actSum + values[i], sum);
                    }
                }
            }
        }

        #endregion

        #region Sums Variations

        internal List<int[]> SumsVariations(List<int> values, int n, int k, bool order)
        {
            if (order) { values.Sort(); }

            List<int[]> sumsVariations = new List<int[]>();
            int[] var = new int[n];
            SumsVariations(sumsVariations, values, n, var, 0, 0, k, 0);
            return sumsVariations;
        }

        private void SumsVariations(List<int[]> sumsVars, List<int> values, int n, int[] var, int top, int actSum, int sum, int firstIndex)
        {

            if (sum < values[0] * var.Length || sum > values[values.Count - 1] * var.Length || firstIndex > values.Count - 1) { return; }

            if (top == n - 1)
            {
                if (actSum + values[firstIndex] <= sum && actSum + values[values.Count - 1] >= sum)
                {
                    int diff = sum - actSum;
                    for (int i = firstIndex; i < values.Count; i++)
                    {
                        if (values[i] == diff)
                        {
                            var[top] = diff;
                            int[] clone = new int[n];
                            var.CopyTo(clone, 0);
                            sumsVars.Add(clone);
                        }
                    }
                }
            }
            else
            {
                for (int i = firstIndex; i < values.Count; i++)
                {
                    var[top] = values[i];
                    if ((n - top - 1) * values[0] <= sum - actSum - values[i] && (n - top - 1) * values[values.Count - 1] >= sum - actSum - values[i])
                    {
                        SumsVariations(sumsVars, values, n, var, top + 1, actSum + values[i], sum, i);
                    }
                }
            }
        }

        internal Dictionary<int, List<int[]>> SumsVariationsUntil(List<int> values, int n, int k, bool order)
        {
            if (order) { values.Sort(); }

            Dictionary<int, List<int[]>> sumsVarsDict = new Dictionary<int, List<int[]>>();
            int[] var = new int[n];
            SumsVariationsUntil(sumsVarsDict, values, n, var, 0, 0, k, 0);
            return sumsVarsDict;
        }

        private void SumsVariationsUntil(Dictionary<int, List<int[]>> sumsVarsDict, List<int> values, int n, int[] var, int top, int actSum, int sum, int firstIndex)
        {

            if (sum < values[0] * var.Length || sum > values[values.Count - 1] * var.Length || firstIndex > values.Count - 1) { return; }

            if (top == n - 1)
            {
                if (actSum + values[firstIndex] <= sum /*&& actSum + values[values.Count-1] >= sum*/)
                {
                    int val = 0;
                    double max = sum - actSum;
                    int i = firstIndex;
                    int[] clone;
                    int finalSum;
                    while (val < max && i < values.Count)
                    {
                        val = values[i];
                        finalSum = actSum + val;
                        var[top] = values[i];
                        clone = new int[n];
                        var.CopyTo(clone, 0);
                        if (val < max)
                        {
                            if (!sumsVarsDict.ContainsKey(finalSum)) { sumsVarsDict.Add(finalSum, new List<int[]>()); }
                            sumsVarsDict[finalSum].Add(clone);
                        }
                        i++;
                    }
                }
            }
            else
            {
                for (int i = firstIndex; i < values.Count; i++)
                {
                    var[top] = values[i];
                    if ((n - top - 1) * values[0] <= sum - actSum - values[i] /*&& (n-top-1) * values[values.Count-1] >= sum - actSum - values[i]*/)
                    {
                        SumsVariationsUntil(sumsVarsDict, values, n, var, top + 1, actSum + values[i], sum, i);
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Permutations

        internal List<int> Permutation(IList<int> indexes)
        {
            List<int> perm = new List<int>();
            while (indexes.Count > 0)
            {
                int i = rand.Next(indexes.Count - 1);
                perm.Add(indexes[i]);
                indexes.RemoveAt(i);
            }
            return perm;
        }

        internal List<int> Permutation(int min, int max)
        {
            List<int> indexes = new List<int>();
            for (int i = min; i <= max; i++) { indexes.Add(i); }
            return Permutation(indexes);
        }

        #endregion

    }
}
