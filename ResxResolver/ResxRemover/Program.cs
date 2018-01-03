﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="James Croft">
//   Copyright (c) James Croft.
// </copyright>
// <summary>
//   Defines the console application which removes string resources by their name from files in RESW and RESX files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ResxRemover
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using ResxCommon;

    using WinUX;

    /// <summary>
    /// Defines the console application which removes string resources by their name from files in RESW and RESX files.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">
        /// Program entry parameter, should be the directory where the resources are located.
        /// </param>
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Not enough information provided. Please provide the path to the folder containing resources and the resource name to remove, e.g. ResxRemover.exe \"C:\\Resources\\\" \"Resource1\".");
                return;
            }

            ConsoleHelper.StartFileLogging();

            List<FileInfo> resourceFiles = new List<FileInfo>();

            string resourceName = string.Empty;

            try
            {
                DirectoryInfo rootDirectory = new DirectoryInfo(args[0]);
                resourceName = args[1];

                if (string.IsNullOrWhiteSpace(resourceName))
                {
                    Console.WriteLine("Cannot remove a resource name which is null or empty.");
                    return;
                }

                Console.WriteLine($"Removing resource '{resourceName}' from resource files in '{rootDirectory.FullName}'.");

                GetResourcesFromDirectory(rootDirectory, resourceFiles);
            }
            catch (Exception)
            {
                // ToDo
            }

            Console.WriteLine($"Attempting to remove {resourceName} from {resourceFiles.Count} files.");

            foreach (FileInfo resourceFile in resourceFiles)
            {
                RemoveResourceFromFile(resourceFile, resourceName);
            }

            ConsoleHelper.StopFileLogging();

            Console.WriteLine("Completed");
        }

        private static void GetResourcesFromDirectory(DirectoryInfo directoryInfo, List<FileInfo> resourceFiles)
        {
            if (resourceFiles == null)
            {
                resourceFiles = new List<FileInfo>();
            }

            // Ignore checking the obj or bin folders.
            if (directoryInfo.FullName.Contains("\\obj\\", CompareOptions.IgnoreCase)
                || directoryInfo.FullName.Contains("\\bin\\", CompareOptions.IgnoreCase))
            {
                return;
            }

            try
            {
                Console.WriteLine($"Looking for resource files in '{directoryInfo.FullName}'.");

                List<DirectoryInfo> childDirectories = directoryInfo.GetDirectories().ToList();

                if (childDirectories.Count > 0)
                {
                    foreach (DirectoryInfo directory in childDirectories)
                    {
                        GetResourcesFromDirectory(directory, resourceFiles);
                    }
                }

                resourceFiles.AddRange(directoryInfo.GetFiles("*.resx"));
                resourceFiles.AddRange(directoryInfo.GetFiles("*.resw"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void RemoveResourceFromFile(FileSystemInfo fileInfo, string resourceToRemove)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileInfo.FullName);

                SortedList<string, XmlNode> currentResources = new SortedList<string, XmlNode>();

                XmlNodeList resourceNodes = xmlDocument.SelectNodes("//data[@name]");
                if (resourceNodes != null)
                {
                    foreach (XmlNode resource in resourceNodes)
                    {
                        if (resource.Attributes != null)
                        {
                            string resourceName = resource.Attributes["name"].Value;

                            if (!resourceName.Equals(resourceToRemove, StringComparison.CurrentCultureIgnoreCase))
                            {
                                currentResources.Add(resourceName, resource);
                            }
                            else
                            {
                                Console.WriteLine($"Removed {resourceToRemove} from '{fileInfo.FullName}'.");
                            }
                        }

                        resource.ParentNode?.RemoveChild(resource);
                    }
                }

                foreach (string resourceKey in currentResources.Keys)
                {
                    xmlDocument.DocumentElement?.AppendChild(currentResources[resourceKey]);
                }

                xmlDocument.Save(fileInfo.FullName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}