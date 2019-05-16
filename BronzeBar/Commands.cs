using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BronzeBar
{
    public class Command
    {
        private readonly Action<string[]> commandImplementation;

        public Command(Action<string[]> commandToExecute)
        {
            commandImplementation = commandToExecute;
        }

        public void Execute(string[] args) {
            commandImplementation?.Invoke(args);
        }  
    }

    static class Commands
    {
        private static Dictionary<string, Command> CommandList = new Dictionary<string, Command>()
        {
            {"armory", new Command((string[] args) =>
                {
                    PrintLine("* BronzeBar Root");
                    PrintLine("|");
                    foreach (string dir in Directory.GetDirectories(BronzeIO.PackagesDirectory))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        DirectoryInfo currentDirInfo = new DirectoryInfo(dir);
                        PrintLine("|---* " + currentDirInfo.Name);
                        if (currentDirInfo.GetDirectories().Any(o => o.Name == "data"))
                        {
                            FileInfo[] bbdFilesInPackageDirectory = currentDirInfo.GetDirectories("data")[0].GetFiles("*.BBD");
                            foreach (FileInfo package in bbdFilesInPackageDirectory)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                string extension = "";
                                if (!Directory.Exists(File.ReadAllText(package.FullName)))
                                {
                                    extension += " ! MIRROR SOURCE MISSING";
                                }
                                if (!Directory.Exists(Path.Combine(BronzeIO.GetSysFolderInPackage(currentDirInfo.Name, "data"), package.Name.Remove(package.Name.Length - 4))))
                                {
                                    extension += " ! BINARIES MISSING";
                                }
                                PrintLine("|   |---* " + package.Name.Remove(package.Name.Length - 4) + extension);
                            }
                            PrintLine("|");
                        }
                        else PrintLine("|   !---* " + "NO DATA FOLDER FOUND!");
                        if (!currentDirInfo.GetDirectories().Any(o => o.Name == "deployments")) PrintLine("    !---* " + "NO DEPLOYMENTS FOLDER FOUND!");
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                })
            },
            {"update", new Command((string[] args) =>
                {
                    if (string.IsNullOrEmpty(BronzeBar.CurrentPackageSelection))
                    {
                        PrintLine("No object is being smithed. Select an object for smithing first.");
                        return;
                    }
                    if(args == null || args.Length == 0)
                    {
                        PrintLine("Update what?");
                        if(Directory.Exists(Path.Combine(BronzeIO.PackagesDirectory, BronzeBar.CurrentPackageSelection)))
                        {
                            PrintLine("Currently available:");
                            foreach(string appInPackage in Directory.GetDirectories(Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.CurrentPackageSelection,"data"))))
                            {
                                PrintLine($"|-- {new DirectoryInfo(appInPackage).Name}");
                            }
                        }
                        return;
                    }

                    string appIdentifier = args[0];
                    if (!Regex.IsMatch(appIdentifier, BronzeBar.InputValidationRegex))
                    {
                        PrintLine($"Invalid app identifier: {appIdentifier}");
                        PrintLine("Allowed: 1-32 alphanumeric characters including underscores");
                        return;
                    }
                    if (BronzeIO.PackageExists(BronzeBar.CurrentPackageSelection))
                    {
                        string appDataFolder = Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.CurrentPackageSelection, "data"));
                        if (File.Exists($@"{Path.Combine(appDataFolder,appIdentifier)}.BBD"))
                        {
                            PrintLine("Updating app!");
                            string mirrorSourceDirectory = File.ReadAllText($@"{Path.Combine(appDataFolder,appIdentifier)}.BBD").Trim();
                            PrintLine($"Copying {mirrorSourceDirectory} to {Path.Combine(appDataFolder,appIdentifier)}");
                            BronzeIO.DirectoryCopy(mirrorSourceDirectory, Path.Combine(appDataFolder,appIdentifier), true);
                            PrintLine("Done!");
                        }
                        else
                        {
                            PrintLine("Could not find " + $@"{Path.Combine(appDataFolder,appIdentifier)}.BBD");
                        }
                    }
                    })
            },
            {"forge", new Command((string[] args) =>
                {
                    if(args == null || args.Length == 0)
                    {
                        PrintLine("Cannot forge an object with no name.");
                        PrintLine("Allowed: 1-32 alphanumeric characters including underscores");
                        return;
                    }
                    string appIdentifier = args[0];
                    if (!Regex.IsMatch(appIdentifier, BronzeBar.InputValidationRegex))
                    {
                        PrintLine("Cannot forge an object with that name.");
                        PrintLine("Allowed: 1-32 alphanumeric characters including underscores");
                        return;
                    }
                    if (Directory.Exists(Path.Combine(BronzeIO.PackagesDirectory, appIdentifier)))
                    {
                        PrintLine($"Cannot forge {appIdentifier}: Object by that name already exists.");
                        return;
                    }
                    try
                    {
                        Directory.CreateDirectory(Path.Combine(BronzeIO.PackagesDirectory, appIdentifier));
                        Directory.CreateDirectory(Path.Combine(BronzeIO.PackagesDirectory, appIdentifier, "data"));
                        Directory.CreateDirectory(Path.Combine(BronzeIO.PackagesDirectory, appIdentifier, "deployments"));
                    }
                    catch (Exception ex)
                    {
                        PrintLine($"Cannot forge {appIdentifier}: {ex.Message} {ex.InnerException.Message}");
                        return;
                    }
                    PrintLine($"Forged ");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    PrintLine($"{appIdentifier} ");
                    Console.ForegroundColor = ConsoleColor.White;
                })
            },
            {"smith", new Command((string[] args) =>
                {
                    if(args == null || args.Length == 0)
                    {
                        PrintLine("Smith what?");
                        PrintLine("Currently available:");
                        foreach(string package in Directory.GetDirectories(BronzeIO.PackagesDirectory))
                        {
                            PrintLine($"|-- {new DirectoryInfo(package).Name}");
                        }
                        return;
                    }

                    string selectedPackage = args[0];
                    if (!Directory.Exists(Path.Combine(BronzeIO.PackagesDirectory, selectedPackage)))
                    {
                        PrintLine($"Cannot smith {selectedPackage}: No object by that name.");
                        return;
                    }
                    BronzeBar.CurrentPackageSelection = selectedPackage;
                    PrintLine($"{selectedPackage} is ready for smithing.");
                })
            },
            {"add", new Command((string[] args) =>
                {
                    if (string.IsNullOrEmpty(BronzeBar.CurrentPackageSelection))  //Check if user selected a package
                    {
                        PrintLine("You're not smithing an object to which you can add a new application.");
                        return;
                    }
                    if(args == null || args.Length == 0)
                    {
                        PrintLine("Cannot add empty: Full directory path of target application required.");
                        return;
                    }
                    string pathToTargetDirectory = args[0];

                    if (!Directory.Exists(pathToTargetDirectory))
                    {
                        PrintLine($"Cannot add {pathToTargetDirectory} to {BronzeBar.CurrentPackageSelection}: Supplied directory does not exist.");
                        return;
                    }
                    if (!BronzeIO.PackageExists(BronzeBar.CurrentPackageSelection))
                    {
                        PrintLine($"Cannot add {pathToTargetDirectory} to {BronzeBar.CurrentPackageSelection}: Package {BronzeBar.CurrentPackageSelection} not found.");
                        return;
                    }

                    Console.WriteLine("Enter app identifier below:");
                    string programToAddName = BronzeBar.GetUserInput();

                    using (StreamWriter sw = File.CreateText(Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.CurrentPackageSelection, "data"), $"{programToAddName}.BBD")))
                    {
                        sw.Write(pathToTargetDirectory);
                    }
                    string ToCopyTo = Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.CurrentPackageSelection, "data"), programToAddName);
                    BronzeIO.DirectoryCopy(pathToTargetDirectory, ToCopyTo, true);
                    })
            },
            {"deploy", new Command((string[] args) =>
                {
                    if (string.IsNullOrEmpty(BronzeBar.CurrentPackageSelection))
                    {
                        PrintLine("No object selected to deploy.");
                        return;
                    }
                    if (!BronzeIO.PackageExists(BronzeBar.CurrentPackageSelection))
                    {
                        PrintLine($"Unhealthy or missing package: {BronzeBar.CurrentPackageSelection}.");
                        return;
                    }
                    PrintLine($"Enter deployment name:");
                    string deploymentName = BronzeBar.GetUserInput();
                    PrintLine($"Enter external deployment output directory:");
                    string externalDeploymentTarget = BronzeBar.GetUserInput();

                    FileInfo[] bbdFilesInPackageDirectory = new DirectoryInfo(BronzeIO.GetSysFolderInPackage(BronzeBar.CurrentPackageSelection, "data")).GetFiles("*.BBD");
                    BatchScriptFactory bsf = new BatchScriptFactory(BronzeBar.Settings.WorkingDirectory);
                    foreach (FileInfo bbdOfPackageToDeploy in bbdFilesInPackageDirectory)
                    {
                        string appName = bbdOfPackageToDeploy.Name.Remove(bbdOfPackageToDeploy.Name.Length - 4);
                        string packageDeploymentDirectory = Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.CurrentPackageSelection, "deployments"), deploymentName, appName);
                        PrintLine($"Packaging {appName}...");
                        if (Directory.Exists(packageDeploymentDirectory))
                        {
                            PrintLine(packageDeploymentDirectory + " already exists as a deployment folder.");
                        }
                        else
                        {
                            Directory.CreateDirectory(packageDeploymentDirectory);
                        }
                        PrintLine("Generating solo app deployment batch...");

                        string deployerBatch = bsf.CreateAppDeployer(BronzeBar.CurrentPackageSelection, appName, externalDeploymentTarget);
                        using (StreamWriter sw = File.CreateText(Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.CurrentPackageSelection, "deployments"), deploymentName, $"solo_{appName}.bat")))
                        {
                            sw.Write(deployerBatch);
                        }
                        PrintLine("Done generating solo app deployment batch.");
                        PrintLine($"Copying {Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.CurrentPackageSelection, "data"), appName)} to {packageDeploymentDirectory}");
                        BronzeIO.DirectoryCopy(Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.CurrentPackageSelection, "data"), appName), packageDeploymentDirectory, true);
                        PrintLine("Done copying. Enjoy your meal.");
                    }

                    string packageDeployerBatch = bsf.CreatePackageDeployer(BronzeBar.CurrentPackageSelection, externalDeploymentTarget);
                    using (StreamWriter sw = File.CreateText(Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.CurrentPackageSelection, "deployments"), deploymentName, $"pack_{deploymentName}.bat")))
                    {
                        sw.Write(packageDeployerBatch);
                    }
                    })
            },
        };

        private static void PrintLine(string v)
        {
            Console.WriteLine(v);
        }

        public static void ExecuteCommand(string command, string[] args = null)
        {
            command = command.ToLower();
            if(CommandList.ContainsKey(command))
            {
                CommandList[command].Execute(args);
            }
            else
            {
                PrintLine("Unknown command");
            }
        }
    }
}
