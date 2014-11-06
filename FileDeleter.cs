/**
 *	Handles cleaning up of temp files.
 *  Reads path data from a SQLite database and age limit on files. 
 *  All files that match are deleted. 
 * 
 *  Author: Nicholas Dunnaway
 *  Version: $Id: FileDeleter.cs,v 1.3 2011/07/27 20:11:29 nkd Exp $
 */
using System;
using System.IO;
using System.Data;

namespace TecNicPro.FileDeleter
{
    /// <summary>
    /// Handles cleaning up of temp files.
    /// </summary>
    class FileDeleter
    {
        private static bool _verbose; // Are we in verbose mode?
        private const string DBFile = "db.sqlite";
        private static readonly SQLiteWrapper.SQLiteWrapper DB = new SQLiteWrapper.SQLiteWrapper(DBFile);
        private static bool PathDBInstalled 
        {
            get
            {
              string tmp = DB.Get("PathDBInstalled");
              return tmp == "TRUE";
            }

          set { DB.Set("PathDBInstalled", value ? "TRUE" : "FALSE"); }
        }

        /// <summary>
        /// App entry point.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //Console.WriteLine("");
            //Console.Read(); // Pause

            // Print the version number. 
            if (args.Length > 0 && (args[0] == "-v" || args[0] == "-version" || args[0] == "-ver"))
            {
                string sVersion = "$Revision: 1.3 $";
                sVersion = sVersion.Replace("$", "");               // Remove the CVS dollar sign.
                sVersion = sVersion.Replace("Revision", "Version"); // Replace revision with version.
                Console.WriteLine(sVersion);
                Exit();
            }

            try
            {
                // Check if the database is installed. 
                if (PathDBInstalled != true)
                {
                    InstallDB(); // Install it
                }

                // Check if called with out a Parameter. 
                if (args.Length == 0)
                {
                    var aTmp = new string[1];
                    aTmp[0] = "run";
                    ProcessCommands(aTmp);
                }
                else
                {
                    ProcessCommands(args);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e);
            }
        }

        /// <summary>
        /// Installs the path database.
        /// </summary>
        private static void InstallDB()
        {
            DB.RawCmd("CREATE TABLE paths (id INTEGER PRIMARY KEY AUTOINCREMENT, path VARCHAR(250) UNIQUE, age int(4))");
            DB.Set("PathDBInstalled", "TRUE");
        }

      /// <summary>
      /// Logic case statement for processing commands.
      /// </summary>
      private static void ProcessCommands(string[] aParameters)
        {
            switch (aParameters[0])
            {
                case "-vb":
                    // Check if we are doing verbose reporting
                    if (aParameters.Length == 2 && aParameters[1] == "-v")
                    {
                        _verbose = true;
                    }
                    break;

                case "-add":
                    // Check if enough parameters were passed.
                    if (aParameters.Length < 3)
                    {
                        ErrorExit("Unable to add path to database. Missing Parameter. See help for more info.");
                    }
                    Add(aParameters[1], aParameters[2]);
                    break;

                case "-a":
                    // Check if enough parameters were passed.
                    if (aParameters.Length < 3)
                    {
                        ErrorExit("Unable to add path to database. Missing Parameter. See help for more info.");
                    }
                    Add(aParameters[1], aParameters[2]);
                    break;

                case "-del":
                    // Check if enough parameters were passed.
                    if (aParameters.Length < 2)
                    {
                        ErrorExit("Unable to delete path from database. Missing Parameter. See help for more info.");
                    }
                    Del(aParameters[1]);
                    break;

                case "-d":
                    // Check if enough parameters were passed.
                    if (aParameters.Length < 2)
                    {
                        ErrorExit("Unable to delete path from database. Missing Parameter. See help for more info.");
                    }
                    Del(aParameters[1]);
                    break;

                case "-list":
                    List();
                    break;

                case "-l":
                    List();
                    break;

                case "-path":
                    // Check if enough parameters were passed.
                    if (aParameters.Length < 3)
                    {
                        ErrorExit("Unable to add path to database. Missing Parameter. See help for more info.");
                    }
                    int i = int.Parse(aParameters[2]);
                    DeleteFiles(aParameters[1], i);
                    break;

                case "-p":
                    // Check if enough parameters were passed.
                    if (aParameters.Length < 3)
                    {
                        ErrorExit("Unable to add path to database. Missing Parameter. See help for more info.");
                    }
                    int i2 = int.Parse(aParameters[2]);
                    DeleteFiles(aParameters[1], i2);
                    break;

                case "-h":
                    Help();
                    break;

                case "-help":
                    Help();
                    break;

                case "run":
                    NormalRun();
                    break;

                default:
                    Console.WriteLine("Parameter Unknown: Try \"FileDeleter -h\" for help.");
                    break;
            }
        }

        /// <summary>
        /// This runs when called w/o parameters. This will read 
        /// from the database and loop each directory.
        /// </summary>
        private static void NormalRun()
        {
            DataTable results = DB.Select("SELECT path, age FROM paths");
            DataRow[] rows = results.Select();
            foreach (DataRow r in rows)
            {
                DeleteFiles(r["path"].ToString(), int.Parse(r["age"].ToString()));
            }            
        }

        /// <summary>
        /// Deletes a path from the database
        /// </summary>
        /// <param name="p">PathID of the path to delete.</param>
        private static void Del(string p)
        {
            DB.RawCmd("DELETE FROM paths WHERE id = " + p);
            Console.WriteLine("Path deleted.");
        }


        /// <summary>
        /// Prints the error message to the user and exits the app. 
        /// </summary>
        /// <param name="p">Error message to print.</param>
        private static void ErrorExit(string p)
        {
            Console.WriteLine(p);
            Exit();
        }

        /// <summary>
        /// Adds a path to check to the database.
        /// </summary>
        /// <param name="p">Path to check for files.</param>
        /// <param name="p2">Delete files over this many days old.</param>
        private static void Add(string p, string p2)
        {
            DB.RawCmd("INSERT INTO paths (path, age) VALUES ('" + p + "', '" + p2 + "')");
            Console.WriteLine("Path added to database.");
        }

        /// <summary>
        /// Prints the help items to the screen.
        /// </summary>
        private static void Help()
        {
            Console.WriteLine("Usage: FileDeleter [options ...] [parameters]\n");
            Console.WriteLine("Options:");

            Console.WriteLine("\t-a or -add - Add a new path");
            Console.WriteLine("\t\tFileDeleter -a path age\n");

            Console.WriteLine("\t-d or -del - Delete a path");
            Console.WriteLine("\t\tFileDeleter -d pathid \n");

            Console.WriteLine("\t-l or -list - List all paths from database.\n");

            Console.WriteLine("\t-p or -path - Delete files from the path older then age. Do not add to database.");
            Console.WriteLine("\t\tFileDeleter -p path age\n");

            Console.WriteLine("\t-h or -help - This help list.");

        }

        /// <summary>
        /// Prints a list of paths, ages and ids to the screen.
        /// </summary>
        private static void List()
        {
            DataTable results = DB.Select("SELECT * FROM paths");
            DataRow[] rows = results.Select();
            Console.WriteLine("PathID\t Path\t Age");
            foreach (DataRow r in rows)
            {
                foreach (DataColumn c in r.Table.Columns)
                {
                    Console.Write("{0}\t ", r[c]);
                }
                Console.WriteLine();
            }
        }


        /// <summary>
        /// Exits the App
        /// </summary>
        private static void Exit()
        {
            //Console.WriteLine("Press any key to continue.");
            //Console.Read(); // Pause
            Environment.Exit(0);
        }


      /// <summary>
      /// Checks files and del anything over iAge days old
      /// </summary>
      /// <param name="path">Path you want to clean</param>
      /// <param name="age"></param>
      private static void DeleteFiles(string path, int age)
        {
            // Check if DIR is valid.
            if (!Directory.Exists(path))
            {
                Console.WriteLine("The path (" + path + ") is not a valid path.");
                Exit();
            }
            try
            {
                var di = new DirectoryInfo(path);
                FileInfo[] fi = di.GetFiles();

                if (_verbose)
                {
                    Console.WriteLine("Number of files: {0}", fi.Length);
                    Console.WriteLine("The following files exist in the current directory:");
                }

                // Print out the names of the files in the current directory.
                foreach (FileInfo fiTemp in fi)
                {
                    if (_verbose)
                    {
                        Console.Write(fiTemp.Name + " ... Checking File ... ");
                        Console.Write(fiTemp.LastWriteTime.ToShortDateString() + " ... "); // mod date
                    }

                    if (fiTemp.LastWriteTime < DateTime.Now.AddDays((age * -1)))
                    {
                        // Check if file is over iAge days old. If so del it.
                        try
                        {
                            fiTemp.Delete();
                        }
                        catch (Exception e)
                        {
                            if (_verbose)
                            {
                                Console.WriteLine(e.Message);
                            } 
                        }
                        
                        if (_verbose)
                        {
                            Console.WriteLine("Deleted.");
                        }

                    }
                    else
                    {
                        // If file is less then age days old then we ignore it.
                        if (_verbose)
                        {
                            Console.WriteLine("Ignored.");
                        }
                    }
                }

                // Recursive
                DirectoryInfo[] dii = di.GetDirectories();
                foreach (DirectoryInfo diTemp in dii)
                {
                    DeleteFiles(diTemp.FullName, age); // Recursive
                    FileInfo[] fii = diTemp.GetFiles();
                    if (fii.Length > 0)
                    {
                        // There are files in the dir so we can not delete it. 
                        if (_verbose)
                        {
                            foreach (FileInfo fiiTemp in fii)
                            {
                                Console.WriteLine(fiiTemp.FullName); // Debugging.
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            Directory.Delete(diTemp.FullName);
                        }
                        catch (Exception e)
                        {
                            if (_verbose)
                            {
                                Console.WriteLine(e.Message);
                            } 
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
        }
    }
}
