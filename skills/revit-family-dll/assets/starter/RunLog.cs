using System;
using System.IO;
using System.Text;
using Autodesk.Revit.UI;

namespace RevitFamilyGenerator
{
    internal static class RunLog
    {
        internal static string PathName { get; private set; }

        internal static void Start(UIApplication ui)
        {
            PathName = null;
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (string.IsNullOrEmpty(desktop) || !Directory.Exists(desktop))
                throw new IOException("Windows Desktop could not be resolved. No family was generated.");
            PathName = Path.Combine(desktop, "RevitFamilyGenerator_Log.txt");
            Write("=== RUN " + DateTimeOffset.Now.ToString("O") + " ===");
            Write("Assembly=" + typeof(RunLog).Assembly.GetName().Name +
                " Version=" + typeof(RunLog).Assembly.GetName().Version +
                " MVID=" + typeof(RunLog).Module.ModuleVersionId);
            Write("Revit=" + ui.Application.VersionNumber + " Build=" + ui.Application.VersionBuild);
            Write("Source=embedded synthetic SOURCE_NOTES.md; internal lengths=feet; source lengths=mm");
        }

        internal static void Write(string value)
        {
            if (PathName == null) throw new InvalidOperationException("Log not initialized.");
            // Each append closes/flushed the file so earlier stages survive a later crash.
            File.AppendAllText(PathName, DateTimeOffset.Now.ToString("O") + " | " + value +
                Environment.NewLine, new UTF8Encoding(false));
        }

        internal static string TryWrite(string value)
        {
            try { Write(value); return null; }
            catch (Exception ex) { return "Diagnostic log could not be written: " + ex.Message; }
        }
    }
}
