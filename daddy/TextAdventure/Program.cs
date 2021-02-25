﻿using System;
using System.Collections.Generic;

namespace TextAdventure
{
    class Program
    {
        static void Main(string[] args)
        {
            var processor = new CommandProcessor();
            var bathroom = new Room
            {
                Name = "Bathroom",
                Description = "It's just like the one you have at home, but it's a bit smaller.",
                Synonyms = new List<string>
                {
                    "restroom", "powder room"
                }
            };

            bathroom.ThingsInTheRoom.Add(new Thing
            {
                Name = "Bottle of Water",
                Description = "This is a complementary bottle of water this hotel usually provides guests. It's missing its cap.",
                CanBeConsumed = true,
                Synonyms = new List<string>
                {
                    "bottle", "water"
                }
            });
            bathroom.ThingsInTheRoom.Add(new Thing
            {
                Name = "Stinky Pile",
                Description = "It looks like someone missed the toilet, and there is something shiny in it.",
                CanBeTaken = false,
                Synonyms = new List<string>
                {
                    "pile", "poop", "stinky", "sp"
                },
                Things = new List<Thing>
                {
                    new Thing
                    {
                        Name = "Brass Key",
                        CanBeTaken=false,
                        Description= " It's an average key, but it's stuck in the pile of manuer.",
                        Synonyms = new List<string>
                        {
                            "larger key", "key", "brass", " brass key"
                        }
                    }
                }
            });

            var hotel = new Room();
            hotel.Name = "Hotel Suite";
            hotel.Description = "You find yourself in a small hotel room. There is a window and luggage on the ground. The queen sized bed is unmade and covered in Cheeto dust.";
           
            hotel.ThingsInTheRoom.Add(new Thing
            {
                Name = "Luggage",
                CanBeTaken = false,
                CanBeOpened = true,
                HasBeenOpened = false,
                HasNotBeenOpenedDescription = "It is closed.",
                Description = "A black suitcase with a red ribbon around the end of the zipper! Your wallet is in there.",
                Synonyms=new List<string>
                {
                    "suitcase", "belongings", "package", "sewing machine carrier"
                },
                Things = new List<Thing>
                {
                    new Thing
                    {
                        Name = "Wallet",
                        CanBeTaken = true,
                        Description = "This appears to be your wallet.",
                        Synonyms = new List<string>
                        {
                            "billfold", "pocketbook"
                        },
                        Things = new List<Thing>
                        {
                         new Thing
                         {
                             Name = "Small Key",
                            Description=" It's the sort of key that you'd use on a lock. We could use it on something.",
                            Synonyms = new List<string>
                            {
                               "smaller key", "little key", "lock key", "cute key"
                            }
                         }
                        }
                    }
           
                }
            });
            var closet = new Room();
            closet.Name = "Closet";
            closet.Description = "It's a walk-in closet. It's bigger than the bathroom at home.";
            closet.ThingsInTheRoom.Add(new Thing
            {
                Name = "Mug",
                Description = " The mug is black with your name (or at least you think that's your name), 'Joe', on it.",
                Synonyms = new List<string>
                {
                    "cup", "coffee mug", "coffee cup"
                }
            }) ;
            closet.ThingsInTheRoom.Add(new Thing
            {
                Name = "Lockbox",
                Description = "A black box with a keyhole. It seems to be locked.",
                CanBeTaken = false,
                HasBeenOpened = false,
                CanBeOpened = true,
                Synonyms = new List<string>
                {
                    "chest", "container", "box"
                },
                Things = new List<Thing>
                {
                    new Thing
                    {
                        Name = "Bottle of Aspirin",
                        Description = " It's a half full bottle of aspirin. ",
                        CanBeConsumed=true,
                        Synonyms = new List<string>
                        {
                            "aspirin", "aspirin bottle"
                        }
                    }
                }
            });
            
            hotel.Connections.Add(bathroom);
            bathroom.Connections.Add(hotel);
            hotel.Connections.Add(closet);
            closet.Connections.Add(hotel);

            var playerJoe = new Player();
            playerJoe.CurrentRoom = hotel;

            // intro text\\
            Console.WriteLine("Welcome to the text adventure.");
            Console.WriteLine("You wake up with a pounding headache.");

            // Game Loop\\
            while (!playerJoe.IsReadyToQuit)
            {
                Console.WriteLine();
                DrawLine("=");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(playerJoe.CurrentRoom.Name);
                DrawLine("_/\\_", 25);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(playerJoe.CurrentRoom.Description);
                Console.Write("Items in the room include: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                var first = true;
                foreach (var thing in playerJoe.CurrentRoom.ThingsInTheRoom)
                {
                    thing.HasBeenLookedAt = true;

                    if (!first) Console.Write($", ");
                    Console.Write($"{thing.Name}");
                    first = false;
                }
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("Connecting Rooms: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                first = true;
                foreach (var rm in playerJoe.CurrentRoom.Connections)
                {
                    if (!first) Console.Write($", ");
                    Console.Write($"{rm.Name}");
                    first = false;
                }
                Console.WriteLine();
                DrawLine("=");

                processor.ReadCommand(playerJoe);

            }
        }

        private static void DrawLine(string c = "-", int count = 100, ConsoleColor color = ConsoleColor.White)
        {
            var tmp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            for (var i=0; i< count; i++)
            {
                Console.Write(c);
            }
            Console.WriteLine();
            Console.ForegroundColor = tmp;
        }
    }
}
