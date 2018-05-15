﻿using System;
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

        public static int? ToInt(this object item)
        {
            if (item == null)
                return null;
            
            int val = 0;
            int.TryParse(item.ToString(), out val);
            return val;
        }

        public static string GetFirstString(this string item)
        {
            if (string.IsNullOrEmpty(item))
                return "";
            var items =  item.ToCharArray();
            string result = "";
            for (int i=0; i< items.Count(); i++)
            {
                if (char.IsLetter(items[i]))
                    result += items[i];
            }
            return result;
        }

        public static decimal ToDecimal(this string item)
        {
            decimal val = 0;
            decimal.TryParse(item, out val);
            return val;
        }

    }

    
}
