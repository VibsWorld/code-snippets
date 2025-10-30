* ref: <https://stackoverflow.com/a/1469790/1436594>
  * ```csharp
    System.Diagnostics.Process process = new System.Diagnostics.Process();
    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
    //Hide Display window if required like below line
    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
    startInfo.FileName = "cmd.exe";
    startInfo.Arguments = "/C copy /b Image1.jpg + Archive.rar Image2.jpg";
    process.StartInfo = startInfo;
    process.Start();
    //It is important that the argument begins with /C, otherwise it won't work. As @scott-ferguson said: /C carries out the command specified by the string and then terminates.
    ```
* Read to end - https://stackoverflow.com/a/66798326/1436594
  * ```csharp
      using System;
      using System.Diagnostics;
      
      class Program
      {
          static void Main(string[] args)
          {
              var p = Process.Start(
                  new ProcessStartInfo("git", "branch --show-current")
                  {
                      CreateNoWindow = true,
                      UseShellExecute = false,
                      RedirectStandardError = true,
                      RedirectStandardOutput = true,
                      WorkingDirectory = Environment.CurrentDirectory
                  }
              );

        p.WaitForExit();
        string branchName =p.StandardOutput.ReadToEnd().TrimEnd();
        string errorInfoIfAny =p.StandardError.ReadToEnd().TrimEnd();

        if (errorInfoIfAny.Length != 0)
        {
            Console.WriteLine($"error: {errorInfoIfAny}");
        }
        else { 
            Console.WriteLine($"branch: {branchName}");
        }

      }
    }
    ```
