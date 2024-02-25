using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

class Program
{
    static void Main(string[] args)
    {      
        //יצירת פקודת bundle
        var bundlOptionOutput = new Option<FileInfo>("--o", "File path and name");
        var bundlOptionLanguage = new Option<string>("--l", "Programming language to include use all for all languages");
        var bundlOptionNote = new Option<bool?>("--note", "The source code is at the beginning of the file");
        var bundlOptionSort = new Option<string>("--sort", "Sort order for code file by letters or type");
        var bundlOptionRemoveEmptyLine = new Option<bool?>("--remove", "Remove empty line");
        var bundlOptionAuthor = new Option<string>("--author", "Concatenation the author at bundle");

        var createRspCommand = new Command("create-rsp", "Create a response file for bundle command");
        var bundleCommand = new Command("bundle", "bundle code files to single file");
        bundleCommand.AddOption(bundlOptionOutput);
        bundleCommand.AddOption(bundlOptionLanguage);
        bundleCommand.AddOption(bundlOptionNote);
        bundleCommand.AddOption(bundlOptionSort);
        bundleCommand.AddOption(bundlOptionRemoveEmptyLine);
        bundleCommand.AddOption(bundlOptionAuthor);

        bundleCommand.SetHandler((output, languages, note, sort, remove, author) =>
        {
            try
            {
                //output
                // Check if there is a directory in the output.FullName
                string directory = Path.GetDirectoryName(output.FullName);
                if (string.IsNullOrWhiteSpace(directory))
                {
                    // If no directory, use current directory
                    directory = Directory.GetCurrentDirectory();
                }
                // Check if output.FullName has a file extension
                if (string.IsNullOrWhiteSpace(Path.GetExtension(output.FullName)))
                {
                    // If no extension, append default file name
                    output = new FileInfo(Path.Combine(directory, "outputFileName.txt"));
                }
                if (output.Exists)
                {
                    Console.WriteLine("Output file already exist. Please choose a differente name.");
                    return;
                }
                var currentPath = Directory.GetCurrentDirectory();//ניתוב התיקייה
                var allFiles = Directory.GetFiles(currentPath, "*", SearchOption.AllDirectories)//כל הקבצים
                    .Where(file => !file.Contains("bin") && !file.Contains("Debug")).ToList();
                //languages option:
                if (string.IsNullOrEmpty(languages))
                {
                    Console.WriteLine("ERROR:language option is requiered");
                    return;
                }
                var codeFiles = languages.Contains("all") ? allFiles : allFiles.Where(file =>
                languages.Contains(Path.GetExtension(file).TrimStart('.'))).ToList();//אם כתב שפות מכניס רק את השפות שכתב
                foreach (var codeFile in codeFiles)//כתיבה בcommand line את הקבצים שהצטרפו
                {
                    Console.WriteLine($"Including code file: {codeFile}");
                }
                if (allFiles == null)
                {
                    Console.WriteLine("No files to bundle");
                    return;
                }
                bundlOptionSort.SetDefaultValue("name");
                if (sort.ToLower() == "type")
                {
                    codeFiles.Sort((a, b) => Path.GetExtension(a).CompareTo(Path.GetExtension(b)));
                }
                else
                    codeFiles.Sort();
                try
                {
                    using (var outputFile = System.IO.File.CreateText(output.FullName))
                    {
                        // הוספת הערה עם השם שסופק על פי האפשרות --author
                        if (author != null)
                        {
                            outputFile.WriteLine($"// Author: {author}");
                        }
                        foreach (var file in codeFiles)
                        {
                            string fileContent = System.IO.File.ReadAllText(file);
                            //note:
                            if (note != null)
                                outputFile.WriteLine($"// source: {file}");
                            // הסרת שורות ריקות
                            if (remove != null)
                            {
                                string[] lines = fileContent.Split('\n');
                                lines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
                                fileContent = string.Join('\n', lines);
                            }
                            //כותב לקובץ הbundle
                            outputFile.WriteLine(fileContent);
                            outputFile.WriteLine("-----------------------------------------");
                        }
                    }
                    Console.WriteLine($"Files bundled successfully: {output.FullName}");
                }
                catch (DirectoryNotFoundException ex)
                {
                    Console.WriteLine("file path invalid");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:" + ex.Message);
            }
        }, bundlOptionOutput, bundlOptionLanguage, bundlOptionNote, bundlOptionSort, bundlOptionRemoveEmptyLine, bundlOptionAuthor);

        createRspCommand.SetHandler(() =>
        {
            try
            {
                Console.Write("Enter value for language: (for all  write 'all')");
                var languages = Console.ReadLine();
                Console.Write("Do you want to save the bundle file in the current folder? (y/n)");
                var isCurrent = Console.ReadLine();
                string path = "";
                if (isCurrent == "n")
                {
                    Console.WriteLine("Enter the path to the folder you want to save the file and the file name");
                    path = Console.ReadLine();
                }
                else if (isCurrent == "y")
                {
                    Console.WriteLine("Enter the file name");
                    path = Console.ReadLine();
                }
                else
                    Console.WriteLine("ERROR: Invalid input. Exiting...");
                Console.Write("Should I list the source code as a comment in the bundle file?(y/n)");
                var niteValue = Console.ReadLine();
                while (!(niteValue == "y" || niteValue == "n"))
                {
                    Console.Write("Enter again value for note (y/n): ");
                    niteValue = Console.ReadLine();
                }
                bool isNote = false;
                if (niteValue == "n")
                    isNote = false;
                if (niteValue == "y")
                    isNote = true;
                Console.Write("Enter value for sort: ");
                var sortValue = Console.ReadLine();
                bool isSort = false;
                if (sortValue == "n")
                    isSort = false;
                if (sortValue == "y")
                    isSort = true;
                Console.Write("Do you want to remove empty line (y/n): ");
                var removeEmptyLinesValue = Console.ReadLine();
                bool isRemove = false;
                if (removeEmptyLinesValue == "n")
                    isRemove = false;
                if (removeEmptyLinesValue == "y")
                    isRemove = true;
                Console.Write("Enter value for author: ");
                var ValueAuthor = Console.ReadLine();

                string responseContent = $"--l {languages}" + '\n' +
                                        $" --o {path}" + '\n' +
                                        $" --note {isNote}" + '\n' +
                                        $" --sort {sortValue}" + '\n' +
                                        $" --remove {isSort}" + '\n' +
                                        $" --author {ValueAuthor}";
                File.WriteAllText("response.rsp", responseContent);
                Console.WriteLine("Response file 'response.rsp' created successfully.");
                Console.WriteLine("Now you can run 'project1 bundle @response.rsp' to execute the command.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        });

        var rootCommand = new RootCommand("root command for bundel files CLI");
        rootCommand.AddCommand(createRspCommand);
        rootCommand.AddCommand(bundleCommand);

        rootCommand.InvokeAsync(args);
    }

}
