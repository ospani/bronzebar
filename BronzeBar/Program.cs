using System;
using BronzeBar;

namespace BronzeBar
{
    class Program
    {
        static void Main(string[] args)
        {
            BronzeBar.PrintLine("Welcome to Bronze Bar.");
            BronzeBar.DoStartup();
            bool exitRequested = false;
            while (!exitRequested)
            {
                string userInput = BronzeBar.GetUserInput();
                BronzeBar.ParseUserInput(userInput);
            }
        }
    }
}
