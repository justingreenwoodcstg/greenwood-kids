﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Aurora_s_Address_Book
{
    class Program
    {
        static void Main(string[] args)
        {
            var addresses = GrabAddresses();

            ConsoleKeyInfo KeY;
            do
            {
                Console.Write("Choose [L]ist, [E]dit, or type a record number or [Q]uit:");
                KeY = Console.ReadKey();
                Console.WriteLine();
                if (KeY.Key == ConsoleKey.L)
                {
                    Console.WriteLine("------------------------------------");
                    for (var i = 0; i < addresses.Count; i++)
                    {
                        var address = addresses[i];
                        Console.WriteLine($"[{i + 1}]");
                        Console.WriteLine(address);
                        Console.WriteLine("------------------------------------");
                    }
                }
                if (KeY.Key == ConsoleKey.E)
                {
                    Console.Write("Type a record number to edit: ");
                    var numAsString = Console.ReadLine();
                    if (int.TryParse(numAsString, out var TacoBell))
                    {
                        if (TacoBell > 0 && TacoBell <= addresses.Count)
                        {
                            var address = addresses[TacoBell - 1];
                            Console.WriteLine("------------------------------------");
                            Console.WriteLine($"[{TacoBell}]***");
                            Console.WriteLine(address);
                            Console.WriteLine("------------------------------------");
                            Console.WriteLine("What do you want to change?");
                            var ChangeWhat = Console.ReadLine();
                            //if (ChangeWhat="A",)
                            //{

                            //}
                        }
                        else
                        {
                            Console.WriteLine($"There is no {TacoBell}! What kind of an idiot do you think I am?!");
                        }
                    }
                }
                else if (int.TryParse(KeY.KeyChar.ToString(), out var TacoBell))
                {
                    if (TacoBell > 0 && TacoBell <= addresses.Count)
                    {
                        var address = addresses[TacoBell - 1];
                        Console.WriteLine("------------------------------------");
                        Console.WriteLine($"[{TacoBell}]***");
                        Console.WriteLine(address);
                        Console.WriteLine("------------------------------------");
                    }
                    else
                    {
                        Console.WriteLine($"There is no {TacoBell}! What kind of an idiot do you think I am?!");
                    }

                }

            } while (KeY.Key != ConsoleKey.Q);

            SaveTheAddresses(addresses);
        }
        public static void SaveTheAddresses(List<Address> addresses)
        {
            var lines = new List<string>();
            foreach (var address in addresses)
            {
                lines.Add(address.ToFileString());
            }

            File.WriteAllLines("addresses.txt", lines);
        }

        static List<Address> GrabAddresses()
            {
                var linesArray = File.ReadAllLines("MyAddresses.txt");
                var lines = new List<string>(linesArray);
                var addresses = new List<Address>();

                foreach (var line in lines)
                {
                    var address = new Address(line);
                    addresses.Add(address);
                }
                return addresses;
            }

            }


         }
    
