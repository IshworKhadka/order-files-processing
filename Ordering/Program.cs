﻿// See https://aka.ms/new-console-template for more information
using Ordering;

Console.WriteLine("Hello, World!");

OrderingService orderingService = new OrderingService();
orderingService.Process();


Console.ReadKey();