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
        public static string objectSelection = "";
        public static string workingDir = "C:\\bronzebar\\";
        public static string packagesDir = $"{Path.Combine(workingDir, "packages")}";


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
        public static bool PackageExists(string packageName)
        {
            bool a = Directory.Exists(Path.Combine(packagesDir, packageName));
            bool b = Directory.Exists(Path.Combine(packagesDir, packageName, "data"));
            bool c = Directory.Exists(Path.Combine(packagesDir, packageName, "deployments"));
            return a && b && c;
        }
        public static string GetPackageFullPath(string packageName)
        {
            if (!PackageExists(packageName))
            {
                return "";
            }
            return Path.Combine(packagesDir, packageName);
        }
        public static string GetSysFolderInPackage(string packageName, string folderWithinPackage)
        {
            if (!Directory.Exists(Path.Combine(GetPackageFullPath(packageName), folderWithinPackage)))
            {
                return "";
            }
            return Path.Combine(GetPackageFullPath(packageName), folderWithinPackage);
        }

        public static void PrintLine(string line)
        {
            Console.WriteLine($"{line}");
        }

        public static void ParseUserInput(string userInput)
        {
            if (userInput.StartsWith("forge"))
            {
                userInput = userInput.Substring("forge".Length).ToLower();
                if (Regex.IsMatch(userInput, "^\\w{1,32}$"))
                {
                    if (Directory.Exists(Path.Combine(packagesDir, userInput)))
                    {
                        PrintLine($"Cannot forge {userInput}: Object by that name already exists.");
                        return;
                    }
                    try
                    {
                        Directory.CreateDirectory(Path.Combine(packagesDir, userInput));
                        Directory.CreateDirectory(Path.Combine(packagesDir, userInput, "data"));
                        Directory.CreateDirectory(Path.Combine(packagesDir, userInput, "deployments"));
                    }
                    catch (Exception ex)
                    {
                        PrintLine($"Cannot forge {userInput}: {ex.Message} {ex.InnerException.Message}");
                        return;
                    }
                    PrintLine($"Forged ");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    PrintLine($"{userInput} ");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    PrintLine("Cannot forge an object with that name.");
                    PrintLine("Allowed: 1-32 alphanumeric characters including underscores");
                }
            }
            else if (userInput.StartsWith("smith"))
            {
                userInput = userInput.Substring("smith".Length).ToLower().Trim();
                if (!Directory.Exists(Path.Combine(packagesDir, userInput)))
                {
                    PrintLine($"Cannot smith {userInput}: No object by that name.");
                    return;
                }
                objectSelection = userInput;
                PrintLine($"{objectSelection} is ready for smithing.");
            }
            else if (userInput.StartsWith("add"))
            {
                userInput = userInput.Substring("add".Length).ToLower().Trim();
                if (string.IsNullOrEmpty(objectSelection))  //Check if user selected a package
                {
                    PrintLine("You're not smithing an object to which you can add to.");
                    return;
                }
                if (!Directory.Exists(userInput))
                {
                    PrintLine($"Cannot add {userInput} to {objectSelection}: Supplied directory does not exist.");
                    return;
                }
                if (!PackageExists(objectSelection))
                {
                    PrintLine($"Cannot add {userInput} to {objectSelection}: Package {objectSelection} not found.");
                    return;
                }
                Console.WriteLine("Enter app identifier below:");
                string programToAddName = GetUserInput();
                using (StreamWriter sw = File.CreateText(Path.Combine(GetSysFolderInPackage(objectSelection, "data"), $"{programToAddName}.BBD")))
                {
                    sw.Write(userInput);
                }
                string ToCopyTo = Path.Combine(GetSysFolderInPackage(objectSelection, "data"), programToAddName);
                DirectoryCopy(userInput, ToCopyTo, true);
            }
            else if (userInput.StartsWith("armory"))
            {
                PrintLine("* BronzeBar Root");
                foreach (string dir in Directory.GetDirectories(packagesDir))
                {
                    DirectoryInfo currentDirInfo = new DirectoryInfo(dir);
                    PrintLine("|---* " + currentDirInfo.Name);
                    if (currentDirInfo.GetDirectories().Any(o => o.Name == "data"))
                    {
                        FileInfo[] bbdFilesInPackageDirectory = currentDirInfo.GetDirectories("data")[0].GetFiles("*.BBD");
                        foreach (FileInfo package in bbdFilesInPackageDirectory)
                        {
                            string extension = "";
                            if (!Directory.Exists(File.ReadAllText(package.FullName)))
                            {
                                extension += " ! MIRROR SOURCE MISSING";
                            }
                            if (!Directory.Exists(Path.Combine(GetSysFolderInPackage(currentDirInfo.Name, "data"), package.Name.Remove(package.Name.Length - 4))))
                            {
                                extension += " ! BINARIES MISSING";
                            }
                            PrintLine("    |---* " + package.Name.Remove(package.Name.Length - 4) + extension);
                        }
                    }
                    else PrintLine("    !---* " + "NO DATA FOLDER FOUND!");
                        
                    if (!currentDirInfo.GetDirectories().Any(o => o.Name == "deployments")) PrintLine("    !---* " + "NO DEPLOYMENTS FOLDER FOUND!");


                }
            }
            else if (userInput.StartsWith("update"))
            {
                userInput = userInput.Substring("update".Length).ToLower().TrimStart(' ');
                if (string.IsNullOrEmpty(objectSelection))
                {
                    PrintLine("No object selected to update.");
                    return;
                }
                else if (!Regex.IsMatch(userInput, "^\\w{1,32}$"))
                {
                    PrintLine($"Invalid app identifier: {userInput}");
                    PrintLine("Allowed: 1-32 alphanumeric characters including underscores");
                    return;
                }
                if (PackageExists(objectSelection))
                {
                    string appIdentifier = Path.Combine(GetSysFolderInPackage(objectSelection, "data"), userInput);
                    if (File.Exists($@"{appIdentifier}.BBD"))
                    {
                        PrintLine("Updating app!");
                        string mirrorSourceDirectory = File.ReadAllText($@"{appIdentifier}.BBD").Trim();
                        PrintLine($"Copying {mirrorSourceDirectory} to {appIdentifier}");
                        DirectoryCopy(mirrorSourceDirectory, appIdentifier, true);
                        PrintLine("Done!");
                    }
                    else
                    {
                        PrintLine("Could not find " + $@"{appIdentifier}.BBD");
                    }
                }
            }
            else if (userInput.StartsWith("deploy"))
            {
                userInput = userInput.Substring("update".Length).ToLower().TrimStart(' ');
                if (string.IsNullOrEmpty(objectSelection))
                {
                    PrintLine("No object selected to deploy.");
                    return;
                }
                if (!PackageExists(objectSelection))
                {
                    PrintLine($"Unhealthy or missing package: {objectSelection}.");
                    return;
                }
                PrintLine($"Enter deployment name:");
                string deploymentName = GetUserInput();
                PrintLine($"Enter external deployment output directory:");
                string externalDeploymentTarget = GetUserInput();

                FileInfo[] bbdFilesInPackageDirectory = new DirectoryInfo(GetSysFolderInPackage(objectSelection, "data")).GetFiles("*.BBD");
                BatchScriptFactory bsf = new BatchScriptFactory(workingDir);
                foreach (FileInfo bbdOfPackageToDeploy in bbdFilesInPackageDirectory)
                {
                    string appName = bbdOfPackageToDeploy.Name.Remove(bbdOfPackageToDeploy.Name.Length - 4);
                    string packageDeploymentDirectory = Path.Combine(GetSysFolderInPackage(objectSelection, "deployments"), deploymentName, appName);
                    PrintLine($"Packaging {appName}...");
                    if(Directory.Exists(packageDeploymentDirectory))
                    {
                        PrintLine(packageDeploymentDirectory + " already exists as a deployment folder.");
                    }
                    else
                    {
                        Directory.CreateDirectory(packageDeploymentDirectory);
                    }
                    PrintLine("Generating solo app deployment batch...");

                    string deployerBatch = bsf.CreateAppDeployer(objectSelection, appName, externalDeploymentTarget);
                    using (StreamWriter sw = File.CreateText(Path.Combine(GetSysFolderInPackage(objectSelection, "deployments"), deploymentName, $"solo_{appName}.bat")))
                    {
                        sw.Write(deployerBatch);
                    }
                    PrintLine("Done generating solo app deployment batch.");
                    PrintLine($"Copying {Path.Combine(GetSysFolderInPackage(objectSelection, "data"), appName)} to {packageDeploymentDirectory}");
                    DirectoryCopy(Path.Combine(GetSysFolderInPackage(objectSelection, "data"), appName), packageDeploymentDirectory, true);
                    PrintLine("Done copying. Enjoy your meal.");
                }

                string packageDeployerBatch = bsf.CreatePackageDeployer(objectSelection, externalDeploymentTarget);
                using (StreamWriter sw = File.CreateText(Path.Combine(GetSysFolderInPackage(objectSelection, "deployments"), deploymentName, $"pack_{deploymentName}.bat")))
                {
                    sw.Write(packageDeployerBatch);
                }
            }
            else
            {
                PrintLine("Unknown command.");
            }
        }
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                if (File.Exists(temppath))
                {
                    PrintLine($"UPDATE:\t{temppath}");
                }
                else
                {
                    PrintLine($"NEW:\t{temppath}");
                }

                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        public static void DoStartup()
        {
            if (!Directory.Exists(workingDir))
            {
                try
                {
                    Directory.CreateDirectory(workingDir);
                }
                catch (Exception ex)
                {
                    PrintLine($"Error creating working directory: {ex.Message}");
                    PrintLine("Exiting");
                    return;
                }
            }
            if (!Directory.Exists(Path.Combine(workingDir + "packages")))
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(workingDir + "packages"));
                }
                catch (Exception ex)
                {
                    PrintLine($"Error creating packages directory: {ex.Message}");
                    PrintLine("Exiting");
                    return;
                }
            }

            string scriptsFolder = Path.Combine(workingDir + @"scripts\");
            if (!Directory.Exists(scriptsFolder))
            {
                try
                {
                    Directory.CreateDirectory(scriptsFolder);
                }
                catch (Exception ex)
                {
                    PrintLine($"Error creating scripts directory: {ex.Message}");
                    PrintLine("Exiting");
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
                    PrintLine($"File: {batchFileInBinaryDirectory.Name} not present in working directory.");
                    try
                    {
                        File.Copy(batchFileInBinaryDirectory.FullName, Path.Combine(scriptsFolder + batchFileInBinaryDirectory.Name), true);
                        PrintLine($"File: {batchFileInBinaryDirectory.Name} created.");
                    }
                    catch (Exception ex)
                    {
                        PrintLine($"Exception encountered copying {batchFileInBinaryDirectory.FullName} to {Path.Combine(scriptsFolder + batchFileInBinaryDirectory.FullName)}: {ex.Message}");
                    }
                }
                else
                {
                    PrintLine($"File: {batchFileInBinaryDirectory.Name} present in working directory.");
                    if (!File.ReadAllBytes(fileInWorkingDirectory.FullName).SequenceEqual(File.ReadAllBytes(batchFileInBinaryDirectory.FullName)))
                    {
                        try
                        {
                            File.Copy(batchFileInBinaryDirectory.FullName, fileInWorkingDirectory.FullName, true);
                            PrintLine($"File: {fileInWorkingDirectory.Name} updated.");
                        }
                        catch (Exception ex)
                        {
                            PrintLine($"Exception encountered copying {batchFileInBinaryDirectory.FullName} to {fileInWorkingDirectory.FullName}: {ex.Message}");
                        }
                    }
                }
            }

        }
    }
}
