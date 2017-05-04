using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CommonUtil.Utilities
{
   public class RandomShuffle
    {
        [ThreadStatic]
        private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }

   public static class ExtendList
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = RandomShuffle.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static int ToInt(this string item)
        {
            int val = 0;
            int.TryParse(item, out val);
            return val;
        }

        public static decimal ToDecimal(this string item)
        {
            decimal val = 0;
            decimal.TryParse(item, out val);
            return val;
        }

    }

    
}
