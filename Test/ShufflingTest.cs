using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Test
{
    public  class ShufflingTest
    {
        public void TestShuffle()
        {
            var numbers = new List<int>(Enumerable.Range(1, 50));
            numbers.Shuffle();
            var new_numbers = new List<int>(Enumerable.Range(1, 50));
            var shuffled = new_numbers.OrderBy(n => Guid.NewGuid()).ToList();

            Console.WriteLine("The winning numbers are: {0}", string.Join(",  ", numbers.GetRange(0, 10)));
        }
    }

    public static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }

    static class MyExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
