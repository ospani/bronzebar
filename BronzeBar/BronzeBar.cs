using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BronzeBar
{
    static class BronzeBar
    {
        public static string PackageSelection = "";
        public static string InputValidationRegex = "^\\w{1,32}$";

        public static string GetUserInput()
        {
            string userInput = "";
            bool enterPressed = false;

            while (!enterPressed)
            {
                ConsoleKeyInfo pressedKey = Console.ReadKey();
                enterPressed = (pressedKey.Key == ConsoleKey.Enter);
                if (enterPressed) break;
                else if (pressedKey.Key == ConsoleKey.Backspace)
                {
                    if (userInput.Length > 0)
                    {
                        Console.Write(" \b");
                        userInput = userInput.Remove(userInput.Length - 1);
                    }
                }
                else
                {
                    userInput += pressedKey.KeyChar;
                }
            }
            return userInput;
        }
    
        public static bool ParseUserInput(string userInput)
        {
            if (userInput.StartsWith("exit")) return true;
            string[] splitUserInput = userInput.Split();
            if (splitUserInput.Length != 0)
            {
                string userCommand = splitUserInput[0];
                splitUserInput = splitUserInput.Skip(1).ToArray();
                Commands.ExecuteCommand(userCommand, splitUserInput);
            }
            return false;
        }

        public static void DoStartup()
        {
            if (!Directory.Exists(BronzeIO.WorkingDirectory))
            {
                try
                {
                    Directory.CreateDirectory(BronzeIO.WorkingDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating working directory: {ex.Message}");
                    Console.WriteLine("Exiting");
                    return;
                }
            }
            if (!Directory.Exists(Path.Combine(BronzeIO.WorkingDirectory + "packages")))
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(BronzeIO.WorkingDirectory + "packages"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating packages directory: {ex.Message}");
                    Console.WriteLine("Exiting");
                    return;
                }
            }

            string scriptsFolder = Path.Combine(BronzeIO.WorkingDirectory + @"scripts\");
            if (!Directory.Exists(scriptsFolder))
            {
                try
                {
                    Directory.CreateDirectory(scriptsFolder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating scripts directory: {ex.Message}");
                    Console.WriteLine("Exiting");
                    return;
                }
            }
            
            string assemblyFolder = AppDomain.CurrentDomain.BaseDirectory;
            FileInfo[] batchFilesInBinaryDirectory = new DirectoryInfo(Path.Combine(assemblyFolder, @"scripts\")).GetFiles("*.bat");
            FileInfo[] filesInWorkingDirectory = new DirectoryInfo(scriptsFolder).GetFiles("*.bat");
            foreach(FileInfo batchFileInBinaryDirectory in batchFilesInBinaryDirectory)
            {
                FileInfo fileInWorkingDirectory = filesInWorkingDirectory.Where(bat => bat.Name == batchFileInBinaryDirectory.Name).FirstOrDefault();
                if(fileInWorkingDirectory == null)
                {
                    Console.WriteLine($"File: {batchFileInBinaryDirectory.Name} not present in working directory.");
                    try
                    {
                        File.Copy(batchFileInBinaryDirectory.FullName, Path.Combine(scriptsFolder + batchFileInBinaryDirectory.Name), true);
                        Console.WriteLine($"File: {batchFileInBinaryDirectory.Name} created.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception encountered copying {batchFileInBinaryDirectory.FullName} to {Path.Combine(scriptsFolder + batchFileInBinaryDirectory.FullName)}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"File: {batchFileInBinaryDirectory.Name} present in working directory.");
                    if (!File.ReadAllBytes(fileInWorkingDirectory.FullName).SequenceEqual(File.ReadAllBytes(batchFileInBinaryDirectory.FullName)))
                    {
                        try
                        {
                            File.Copy(batchFileInBinaryDirectory.FullName, fileInWorkingDirectory.FullName, true);
                            Console.WriteLine($"File: {fileInWorkingDirectory.Name} updated.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception encountered copying {batchFileInBinaryDirectory.FullName} to {fileInWorkingDirectory.FullName}: {ex.Message}");
                        }
                    }
                }
            }

        }
    }
}
