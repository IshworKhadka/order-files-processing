// See https://aka.ms/new-console-template for more information
using Ordering;

/*
 * Ordering-Discount-Party-Supplies
   Author: Ishwor Khadka
*/
Console.WriteLine("Order File Processing Console Application......Written by: Ishwor Khadka\n");

IOrderingService orderingService = new OrderingService();
orderingService.Process();

Console.WriteLine("Check Ordering\\Ordering\\bin\\Debug\\net6.0 for export.CSV file\n");
Console.WriteLine("THANK YOU!");


Console.ReadKey();