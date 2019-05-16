using System;
using BronzeBar;

namespace BronzeBar
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Bronze Bar.");
            BronzeBar.Initialize();
            bool exitRequested = false;
            while (!exitRequested)
            {
                string userInput = BronzeBar.GetUserInput();
                exitRequested = BronzeBar.HandleUserInput(userInput);
            }
        }
    }
}
