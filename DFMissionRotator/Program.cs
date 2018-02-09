using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

namespace DFMissionRotator
{
    internal class Program
    {
        //Settings
        private const string ProgramName = "Delta Force Mission Rotator";
        private static readonly string ProgramVersion = Assembly.GetCallingAssembly().GetName().Version.Major + "." + Assembly.GetCallingAssembly().GetName().Version.Minor + "." + Assembly.GetCallingAssembly().GetName().Version.Build;

        private static bool Debug;

        private const string NovaHq = "Novahq.net";

        private const string ProgramTitle = @"
             _   _  _____     ___    _   _  ___  
            | \ | |/ _ \ \   / / \  | | | |/ _ \ 
            |  \| | | | \ \ / / _ \ | |_| | | | |
            | |\  | |_| |\ V / ___ \|  _  | |_| |
            |_| \_|\___/  \_/_/   \_\_| |_|\__\_\
        ";

        private const string FolderNamePrefix = "nhqMR-";

        private static void Main(string[] args)
        {

            Debug |= (args.Length > 0 && args[1] == "debug");

            Console.CancelKeyPress += delegate (object s, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                ExitApp();
            };

            while (true)
            {
                Console.Clear();

                #region Intro Text
                WriteConsole(ProgramTitle, true, ConsoleColor.DarkGray);
                WriteConsole("     " + ProgramName + " v" + ProgramVersion + " by <" + NovaHq + ">", true, ConsoleColor.DarkGray);
                WriteConsole("         Rotates mission files for DF2, LW and TFD", true, ConsoleColor.DarkGray);
                WriteConsole("           Press CTRL+C anytime to exit this app", true, ConsoleColor.DarkGray);
                WriteConsole("---------------------------------------------------------------------", true, ConsoleColor.DarkGray);
                WriteConsole(null);
                #endregion

                #region Ask important questions
                WriteConsole("This app removes all *.bms files from your games main folder.");
                WriteConsole("This app will NEVER remove a map from the nhqMR-# folders!");
                WriteConsole("Have you backed up all the maps in your games folder? [y/n]: ", false);

                if (Console.ReadKey().Key != ConsoleKey.Y)
                {
                    WriteConsole(null);
                    WriteConsole(null);
                    WriteConsole("Backup your maps and run this program again.", true, ConsoleColor.Red);

                    WriteConsole(null);
                    ExitApp(0, true);
                }
                #endregion

                WriteConsole(null);
                WriteConsole(null);

                var missionFolders = Directory.GetDirectories(@"./", FolderNamePrefix + "*");

                WriteConsole("Locating possible mission folders", false);
                WaitingDots();
                WriteConsole(missionFolders.Length + " found!", false);
                WriteConsole(null);
                WriteConsole(null);

                if (missionFolders.Length == 0)
                {
                    WriteConsole("It looks like this is your first time running this app.");
                    WriteConsole("Create folders named \"nhqMR-1, nhqMR-2, nhqMR-3\" (Valid: 0-9) in your games folder");
                    WriteConsole("Place your *.bms files inside these folders.");
                    WriteConsole("Each folder contains a group of maps that will be rotated.");
                    WriteConsole("When you are done, run this app again.");

                    WriteConsole(null);
                    ExitApp(0, true);
                }

                var folderIdList = new List<int>();
                foreach (var folder in missionFolders)
                {
                    var folderParts = folder.Split('-');

                    if (string.IsNullOrEmpty(folderParts[1]))
                    {
                        WriteConsole(folder + " has an invalid name", true, ConsoleColor.Red);
                        continue;
                    }

                    if (!int.TryParse(folderParts[1], out var result))
                        continue;

                    if (!(result >= 0 && result <= 9))
                    {
                        WriteConsole(folder + " has an invalid name", true, ConsoleColor.Red);
                        continue;
                    }

                    var bmsList = Directory.GetFiles(folder, "*.bms");

                    if (bmsList.Length == 0)
                    {
                        WriteConsole("ID: " + result + "  Folder: " + folder + " has no maps, excluding!", true, ConsoleColor.Red);
                        continue;
                    }

                    if (bmsList.Length > 255)
                    {
                        WriteConsole("ID: " + result + "  Folder: " + folder + " has over 255 maps. This may cause your game to crash! (" + bmsList.Length + " maps found)", true, ConsoleColor.Yellow);
                    }
                    else
                    {
                        WriteConsole("ID: " + result + "  Folder: " + folder + " OK! (" + bmsList.Length + " maps found)", true, ConsoleColor.Green);
                    }

                    folderIdList.Add(result);
                }

                WriteConsole(null);

                if (folderIdList.Count == 0)
                {
                    
                    WriteConsole("No folders with maps were found. Cannot continue.", true, ConsoleColor.Red);
                    WriteConsole("Add some maps to the folders and run this app again.", true, ConsoleColor.Red);
                    WriteConsole(null);
                    ExitApp(0, true);
                }

                selectMapList:

                WriteConsole("Enter an ID from above to move the maps into the games folder: ", false);

                if (!int.TryParse(Console.ReadLine(), out var idSelect) || !folderIdList.Contains(idSelect))
                {
                    WriteConsole(null);
                    WriteConsole("Invalid ID entered. Please try again or CTRL+C to exit.", true, ConsoleColor.Red);
                    WriteConsole(null);
                    goto selectMapList;
                }

                var folderSelected = FolderNamePrefix + idSelect;

                WriteConsole(null);
                WriteConsole("Copy maps from " + folderSelected + " to the games folder? (This will remove the current maps)? [y/n]: ", false);

                if (Console.ReadKey().Key != ConsoleKey.Y)
                {

                    WriteConsole(null);
                    WriteConsole(null);
                    WriteConsole("Select another ID? [y/n]: ", false);


                    if (Console.ReadKey().Key != ConsoleKey.Y)
                    {
                        WriteConsole(null);
                        WriteConsole(null);
                        WriteConsole("Nothing was changed.", true, ConsoleColor.Blue);
                        WriteConsole(null);
                        ExitApp(0, true);
                    }

                    WriteConsole(null);
                    WriteConsole(null);

                    goto selectMapList;
           
                }

                WriteConsole(null);

                if (!Directory.Exists(folderSelected))
                {
                    WriteConsole(folderSelected + " No longer exists. Cannot continue!", true, ConsoleColor.Red);
                    WriteConsole(null);
                    ExitApp(0, true);
                }

                foreach (var file in Directory.GetFiles("./", "*.bms"))
                {
                    if(Debug)
                        WriteConsole("Removing " + file);

                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception e)
                    {
                        WriteConsole("^^ " + e.Message, true, ConsoleColor.Red);
                    }
        
                }

                foreach (var file in Directory.GetFiles(folderSelected, "*.bms"))
                {
                    if (Debug)
                        WriteConsole("Moving " + file + " to " + "./" + Path.GetFileName(file));

                    try
                    {
                        File.Copy(file, "./" + Path.GetFileName(file));
                    }
                    catch (Exception e)
                    {
                        WriteConsole("^^ " + e.Message, true, ConsoleColor.Red);
                    }

                }

                WriteConsole(null);
                WriteConsole("All done!", true, ConsoleColor.Green);
                WriteConsole(null);
                ExitApp(0, true);

                break;

            }

        }

        public static void ClearCurrentLine()
        {
            var clCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            for (var i = 0; i < Console.WindowWidth; i++)
            {
                Console.Write(" ");
            }
            Console.SetCursorPosition(0, clCursor);
        }

        private static void WriteConsole(string text = "", bool newLine = true, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;

            if (newLine)
            {
                Console.WriteLine(" " + text);
            }
            else
            {
                Console.Write(" " + text);
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void WaitingDots(int sleep = 250, ConsoleColor color = ConsoleColor.Gray, bool newLine = false)
        {
            Thread.Sleep(sleep);

            Console.ForegroundColor = color;

            Console.Write(".");
            Thread.Sleep(sleep);
            Console.Write(".");
            Thread.Sleep(sleep);
            Console.Write(". ");
            Thread.Sleep(sleep);

            if (newLine)
                Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void ExitApp(int exitCode = 0, bool readKeyFirst = false, string message = "Press any key to exit...")
        {
            if (readKeyFirst)
            {
                WriteConsole(message, false);
                Console.ReadKey();
            }

            Environment.Exit(exitCode);
        }

    }

}