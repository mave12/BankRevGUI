
using LibCommon;
using LibCommon.Extension_methods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace BankRev
{
  internal class Program
  {
    internal static string AppName = Application.ProductName;
    internal static bool DirtyArgs = true;
    internal static bool DirtyRun = true;
    internal static string Game = "DzRetail";
    internal static string ToolsBinPath = Path.Combine(Config.RegGet("Dayz Tools", "path", string.Empty), "bin");
    internal static bool HasArgs = false;
    internal static bool IsInError = false;
    internal static bool JobDone = false;
    internal static List<Thread> ThreadList = new List<Thread>();
    internal static Program.State AppState = Program.State.Unchanged;
    private static bool _userCancel = false;

    internal static bool UserCancel
    {
      get => Program._userCancel;
      set
      {
        if (Program.IsInError)
          return;
        Program._userCancel = value;
        if (!value)
          return;
        Program.ExitApplication(true);
      }
    }

        internal static void ExitApplication(bool isUserCancel = false, bool cancelTasks = false, bool revertChanges = false)
    {
      if (Program.IsInError)
        return;
      if (Program._userCancel && !Program.JobDone)
      {
        Logger.Log.Fatal((object) "User cancellation requested.");
        Environment.ExitCode = -1;
      }
      if (Program.ThreadList.Count > 0 && !Program.JobDone)
      {
        foreach (Thread thread in Program.ThreadList.Where<Thread>((Func<Thread, bool>) (thread => thread.IsAlive)))
          thread.Abort();
      }
      Logger.Log.InfoFormat("ExitCode='{0}'", (object) Environment.ExitCode);
      Logger.Log.InfoFormat("Terminate {0}", (object) Application.ProductName);
      Environment.Exit(Environment.ExitCode);
    }

    internal static void ExitApplicationNormal()
    {
      if (Program.IsInError)
        return;
      Environment.ExitCode = 0;
      Program.ExitApplication();
    }

    [STAThread]
    private static void Main()
    {
      if (!Program.ToolsBinPath.IsDirectory())
        throw new Exception("Cannot ToolsBinPath");
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new FormMain());
    }

    internal enum State
    {
      Unchanged,
      Ready,
      Canceled,
      Succeeded,
      Failed,
      Busy,
      Working,
    }
  }
}
