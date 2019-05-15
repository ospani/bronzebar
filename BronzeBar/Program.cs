using System;
using BronzeBar;

namespace BronzeBar
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Bronze Bar.");
            BronzeBar.DoStartup();
            bool exitRequested = false;
            while (!exitRequested)
            {
                string userInput = BronzeBar.GetUserInput();
                exitRequested = BronzeBar.ParseUserInput(userInput);
            }
        }
    }
}
