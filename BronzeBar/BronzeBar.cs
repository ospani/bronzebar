using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace BronzeBar
{
    static class BronzeBar
    {
        public static Settings Settings;
        public static string CurrentPackageSelection = "";
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
        public static bool HandleUserInput(string userInput)
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

        public static void Initialize()
        {
            BronzeBar.Settings = LoadSettings("settings.cfg");
            LoadWorkingDirectory();
            LoadScripts(Path.Combine(BronzeBar.Settings.WorkingDirectory + @"\scripts\"));
        }

        private static Settings GetDefaultSettings()
        {
            Settings defaultSettings = new Settings() { WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"\bronzebar") };
            return defaultSettings;
        }
        private static Settings LoadSettings(string settingsFileName)
        {
            string assemblyFolder = AppDomain.CurrentDomain.BaseDirectory;
            Settings bronzeBarSettings = null;
            //Check if a settings file exists, if so, load its settings.
            if (File.Exists(Path.Combine(assemblyFolder, settingsFileName)))
            {
                XmlSerializer settingsSerializer = new XmlSerializer(typeof(Settings));
                using (FileStream fileStream = new FileStream(Path.Combine(assemblyFolder, "settings.cfg"), FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fileStream))
                    {
                        try
                        {
                            bronzeBarSettings = settingsSerializer.Deserialize(sr) as Settings;
                            Console.WriteLine("Settings loaded.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error encountered when deserializing settings file. {ex.Message}. Loading default settings.");
                            bronzeBarSettings = GetDefaultSettings();
                        }
                    }
                }
            }
            else
            {
                XmlSerializer settingsSerializer = new XmlSerializer(typeof(Settings));
                using (FileStream fileStream = new FileStream(Path.Combine(assemblyFolder, "settings.cfg"), FileMode.OpenOrCreate))
                {
                    using (StreamWriter sw = new StreamWriter(fileStream))
                    {
                        bronzeBarSettings = GetDefaultSettings();
                        settingsSerializer.Serialize(sw, bronzeBarSettings);
                        Console.WriteLine($"No settings file found. Created settings.cfg in {assemblyFolder}");
                    }
                }
            }
            return bronzeBarSettings;
        }
        private static void LoadWorkingDirectory()
        {
            //Check if the working directory exists, since BronzeBar does not use its executable directory for its functionality.
            if (!Directory.Exists(BronzeBar.Settings.WorkingDirectory))
            {
                try
                {
                    Directory.CreateDirectory(Settings.WorkingDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating working directory: {ex.Message}");
                    Console.WriteLine("Exiting");
                    return;
                }
            }
            //Check if the packages folder exists inside of the working directory. BronzeBar relies on packages that users create and these end up in this folder.
            if (!Directory.Exists(Path.Combine(BronzeBar.Settings.WorkingDirectory, "packages")))
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(BronzeBar.Settings.WorkingDirectory, "packages"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating packages directory: {ex.Message}");
                    Console.WriteLine("Exiting");
                    return;
                }
            }
            //Check if the scripts folder exists inside of the working directory. BronzeBar relies on this folder to find template deployment batch files for use in creating deployment packages.
            string scriptsFolder = Path.Combine(BronzeBar.Settings.WorkingDirectory, @"scripts\");
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
        }
        private static void LoadScripts(string scriptsFolder)
        {
            //Check if each of the template batch files are the latest version. If not, overwrite them.
            //The latest versions of these template batch files are located in the BronzeBar executable directory, which also contains a scripts folder.
            string assemblyFolder = AppDomain.CurrentDomain.BaseDirectory;
            FileInfo[] batchFilesInBinaryDirectory = new DirectoryInfo(Path.Combine(assemblyFolder, @"scripts\")).GetFiles("*.bat");
            FileInfo[] batchFilesInWorkingDirectory = new DirectoryInfo(scriptsFolder).GetFiles("*.bat");
            foreach (FileInfo batchFileInBinaryDirectory in batchFilesInBinaryDirectory)
            {
                FileInfo batchFileInWorkingDirectory = batchFilesInWorkingDirectory.Where(bat => bat.Name == batchFileInBinaryDirectory.Name).FirstOrDefault();
                if (batchFileInWorkingDirectory == null) //If the file was missing, copy a fresh one from the binary directory.
                {
                    Console.WriteLine($"File: {batchFileInBinaryDirectory.Name} was not present in working directory.");
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
                    Console.WriteLine($"File: {batchFileInBinaryDirectory.Name} present in working directory.");//If the file already existed...
                    if (!File.ReadAllBytes(batchFileInWorkingDirectory.FullName).SequenceEqual(File.ReadAllBytes(batchFileInBinaryDirectory.FullName))) //...check if the two are equal.
                    {
                        try
                        {
                            File.Copy(batchFileInBinaryDirectory.FullName, batchFileInWorkingDirectory.FullName, true);
                            Console.WriteLine($"File: {batchFileInWorkingDirectory.Name} updated.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception encountered copying {batchFileInBinaryDirectory.FullName} to {batchFileInWorkingDirectory.FullName}: {ex.Message}");
                        }
                    }
                }
            }
        }

    }
}
