﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace codeingAsignmentsPart3
{
    class Program
    {
        static void Main(string[] args)
        {
            int Add(int a, int b)
            {
                return a + b;
            }
            Console.WriteLine(Add(1, 1)); 
            Console.WriteLine(Add(2, 2));
            Console.WriteLine(Add(3, 7));
        }
    }
    //april 22
    public class HighScore
    {
        public string Name { get; set; }
        public int Score { get; set; }

    }
}
