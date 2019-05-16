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
        public static void ExecuteCommand(string command, string[] args = null)
        {
            command = command.ToLower();
            if (CommandList.ContainsKey(command))
            {
                CommandList[command].Execute(args);
            }
            else
            {
                PrintLine("Unknown command");
            }
        }

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
                        if (currentDirInfo.GetDirectories().Any(o => o.Name == "data")) //If this package has a data folder...
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
                    if(args == null || args.Length == 0)
                    {
                        PrintLine("Cannot update an app with no name.");
                        PrintLine("Allowed: 1-32 alphanumeric characters including underscores");
                        return;
                    }
                    string appIdentifier = args[0];
                    if (string.IsNullOrEmpty(BronzeBar.PackageSelection))
                    {
                        PrintLine("No object selected to update.");
                        return;
                    }
                    else if (!Regex.IsMatch(appIdentifier, BronzeBar.InputValidationRegex))
                    {
                        PrintLine($"Invalid app identifier: {appIdentifier}");
                        PrintLine("Allowed: 1-32 alphanumeric characters including underscores");
                        return;
                    }
                    if (BronzeIO.PackageExists(BronzeBar.PackageSelection))
                    {
                        string appDataFolder = Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.PackageSelection, "data"), appIdentifier);
                        if (File.Exists(($@"{appDataFolder}.BBD").Trim()))
                        {
                            PrintLine("Updating app!");
                            string mirrorSourceDirectory = File.ReadAllText($@"{appDataFolder}.BBD").Trim();
                            PrintLine($"Copying {mirrorSourceDirectory} to {appDataFolder}");
                            BronzeIO.DirectoryCopy(mirrorSourceDirectory, appDataFolder, true);
                            PrintLine("Done!");
                        }
                        else
                        {
                            PrintLine("Could not find " + $@"{appIdentifier}.BBD");
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
                    BronzeBar.PackageSelection = selectedPackage;
                    PrintLine($"{selectedPackage} is ready for smithing.");
                })
            },
            {"add", new Command((string[] args) =>
                {
                    if (string.IsNullOrEmpty(BronzeBar.PackageSelection))  //Check if user selected a package
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
                        PrintLine($"Cannot add {pathToTargetDirectory} to {BronzeBar.PackageSelection}: Supplied directory does not exist.");
                        return;
                    }
                    if (!BronzeIO.PackageExists(BronzeBar.PackageSelection))
                    {
                        PrintLine($"Cannot add {pathToTargetDirectory} to {BronzeBar.PackageSelection}: Package {BronzeBar.PackageSelection} not found.");
                        return;
                    }

                    Console.WriteLine("Enter app identifier below:");
                    string programToAddName = BronzeBar.GetUserInput();
                    if(!BronzeIO.CreateBBD(pathToTargetDirectory, programToAddName))
                    {
                        PrintLine($"Unable to create BBD for {programToAddName}");
                        return;
                    }
                    string ToCopyTo = Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.PackageSelection, "data"), programToAddName);
                    BronzeIO.DirectoryCopy(pathToTargetDirectory, ToCopyTo, true);
                    })
            },
            {"deploy", new Command((string[] args) =>
                {
                    if (string.IsNullOrEmpty(BronzeBar.PackageSelection))
                    {
                        PrintLine("No object selected to deploy.");
                        return;
                    }
                    if (!BronzeIO.PackageExists(BronzeBar.PackageSelection))
                    {
                        PrintLine($"Unhealthy or missing package: {BronzeBar.PackageSelection}.");
                        return;
                    }
                    PrintLine($"Enter deployment name:");
                    string deploymentName = BronzeBar.GetUserInput();
                    PrintLine($"Enter external deployment output directory:");
                    string externalDeploymentTarget = BronzeBar.GetUserInput();

                    FileInfo[] bbdFilesInPackageDirectory = new DirectoryInfo(BronzeIO.GetSysFolderInPackage(BronzeBar.PackageSelection, "data")).GetFiles("*.BBD");
                    BatchScriptFactory bsf = new BatchScriptFactory(BronzeIO.WorkingDirectory);
                    foreach (FileInfo bbdOfPackageToDeploy in bbdFilesInPackageDirectory)
                    {
                        string appName = bbdOfPackageToDeploy.Name.Remove(bbdOfPackageToDeploy.Name.Length - 4);
                        string packageDeploymentDirectory = Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.PackageSelection, "deployments"), deploymentName, appName);
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

                        string deployerBatch = bsf.CreateAppDeployer(BronzeBar.PackageSelection, appName, externalDeploymentTarget);
                        using (StreamWriter sw = File.CreateText(Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.PackageSelection, "deployments"), deploymentName, $"solo_{appName}.bat")))
                        {
                            sw.Write(deployerBatch);
                        }
                        PrintLine("Done generating solo app deployment batch.");
                        PrintLine($"Copying {Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.PackageSelection, "data"), appName)} to {packageDeploymentDirectory}");
                        BronzeIO.DirectoryCopy(Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.PackageSelection, "data"), appName), packageDeploymentDirectory, true);
                        PrintLine("Done copying. Enjoy your meal.");
                    }

                    string packageDeployerBatch = bsf.CreatePackageDeployer(BronzeBar.PackageSelection, externalDeploymentTarget);
                    using (StreamWriter sw = File.CreateText(Path.Combine(BronzeIO.GetSysFolderInPackage(BronzeBar.PackageSelection, "deployments"), deploymentName, $"pack_{deploymentName}.bat")))
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
    }
}
