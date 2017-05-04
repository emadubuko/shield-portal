using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CommonUtil.Utilities;

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
     
}
