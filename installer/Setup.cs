using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

[assembly: AssemblyTitle("PodsDesk Setup")]
[assembly: AssemblyDescription("Installation de PodsDesk pour Windows")]
[assembly: AssemblyCompany("Dyskolos_")]
[assembly: AssemblyProduct("PodsDesk")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

internal static class Setup
{
    const string UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\PodsDesk";
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    const string RuntimeUrl = "https://dotnet.microsoft.com/en-us/download/dotnet/10.0";
    static readonly string Root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "PodsDesk");
    static readonly string AppDir = Path.Combine(Root, "app");
    static readonly string AppExe = Path.Combine(AppDir, "PodsDesk.exe");
    static readonly string Uninstaller = Path.Combine(Root, "Uninstall.exe");
    static readonly string StartLink = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "PodsDesk.lnk");
    static readonly string DesktopLink = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "PodsDesk.lnk");

    [STAThread] static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        if (args.Length == 1 && args[0] == "--uninstall") { Uninstall(); return; }
        Application.Run(new SetupWindow());
    }

    static bool RuntimeAvailable()
    {
        var roots = new[] { Environment.GetEnvironmentVariable("DOTNET_ROOT"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "dotnet") };
        foreach (string root in roots.Where(x => !String.IsNullOrWhiteSpace(x)))
        {
            try
            {
                var desktop = Path.Combine(root, "shared", "Microsoft.WindowsDesktop.App");
                var core = Path.Combine(root, "shared", "Microsoft.NETCore.App");
                if (!Directory.Exists(desktop) || !Directory.Exists(core)) continue;
                bool desktop10 = Directory.GetDirectories(desktop).Any(x => IsStableVersion10(x));
                bool core10 = Directory.GetDirectories(core).Any(x => IsStableVersion10(x));
                if (desktop10 && core10) return true;
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        return false;
    }
    static bool IsStableVersion10(string folder)
    {
        Version version;
        return Version.TryParse(Path.GetFileName(folder), out version) && version.Major == 10 && File.Exists(Path.Combine(folder, "System.Private.CoreLib.dll")) ||
            (Version.TryParse(Path.GetFileName(folder), out version) && version.Major == 10 && File.Exists(Path.Combine(folder, "WindowsBase.dll")));
    }
    static bool IsInstalledProcessRunning()
    {
        foreach (var process in Process.GetProcessesByName("PodsDesk").Concat(Process.GetProcessesByName("Scanner")))
        {
            try { using (process) if (SamePath(process.MainModule.FileName, AppExe) || SamePath(process.MainModule.FileName, Path.Combine(AppDir,"Scanner.exe"))) return true; }
            catch (System.ComponentModel.Win32Exception) { }
            catch (InvalidOperationException) { }
        }
        return false;
    }
    static bool SamePath(string left, string right) { return String.Equals(Path.GetFullPath(left), Path.GetFullPath(right), StringComparison.OrdinalIgnoreCase); }
    static Stream Payload() { return Assembly.GetExecutingAssembly().GetManifestResourceStream("PodsDesk.Payload"); }
    static string SafeTarget(string directory, string relative)
    {
        string root = Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string full = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Chemin non valide dans le programme d’installation.");
        return full;
    }
    static void InstallPayload(string directory)
    {
        Directory.CreateDirectory(directory);
        using (Stream stream = Payload())
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
        foreach (var entry in zip.Entries)
        {
            string file = SafeTarget(directory, entry.FullName);
            if (String.IsNullOrEmpty(entry.Name)) { Directory.CreateDirectory(file); continue; }
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            using (var input = entry.Open()) using (var output = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None)) input.CopyTo(output);
        }
    }
    static void RemovePayload(string directory)
    {
        using (Stream stream = Payload())
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
        {
            foreach (var entry in zip.Entries.Where(x => !String.IsNullOrEmpty(x.Name)))
            {
                string file = SafeTarget(directory, entry.FullName);
                if (File.Exists(file)) File.Delete(file);
            }
            var dirs = zip.Entries.Select(x => Path.GetDirectoryName(SafeTarget(directory,x.FullName))).Distinct().OrderByDescending(x => x.Length).ToList();
            foreach (string dir in dirs) if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any()) Directory.Delete(dir);
        }
    }
    static void CreateShortcut(string shortcut, string target)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(shortcut));
        if (File.Exists(shortcut) && !ShortcutPointsTo(shortcut, target)) throw new IOException("Un autre raccourci nommé PodsDesk existe déjà : " + shortcut);
        dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
        dynamic link = shell.CreateShortcut(shortcut);
        link.TargetPath = target; link.WorkingDirectory = Path.GetDirectoryName(target); link.Description = "PodsDesk — votre compagnon AirPods"; link.IconLocation = target + ",0"; link.Save();
    }
    static bool ShortcutPointsTo(string shortcut, string target)
    {
        if (!File.Exists(shortcut)) return false;
        try { dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell")); dynamic link = shell.CreateShortcut(shortcut); string path = link.TargetPath; return !String.IsNullOrEmpty(path) && SamePath(path,target); }
        catch { return false; }
    }
    static void RemoveOwnedShortcut(string shortcut) { if (ShortcutPointsTo(shortcut, AppExe)) File.Delete(shortcut); }
    static void Install(bool desktop)
    {
        if (IsInstalledProcessRunning()) throw new IOException("Ferme PodsDesk depuis son icône près de l’horloge avant de l’installer.");
        if (!Environment.Is64BitOperatingSystem) throw new InvalidOperationException("Cette version nécessite Windows x64.");
        if (!RuntimeAvailable()) throw new InvalidOperationException("Le .NET Desktop Runtime 10 x64 est requis. Installe-le depuis la page Microsoft, puis relance l’installation.");
        if (File.Exists(StartLink) && !ShortcutPointsTo(StartLink, AppExe)) throw new IOException("Un autre raccourci PodsDesk existe dans le menu Démarrer.");
        if (desktop && File.Exists(DesktopLink) && !ShortcutPointsTo(DesktopLink, AppExe)) throw new IOException("Un autre raccourci PodsDesk existe sur le bureau.");
        InstallPayload(AppDir);
        File.Copy(Assembly.GetExecutingAssembly().Location, Uninstaller, true);
        CreateShortcut(StartLink, AppExe);
        if (desktop) CreateShortcut(DesktopLink, AppExe);
        using (RegistryKey key = Registry.CurrentUser.CreateSubKey(UninstallKey))
        {
            key.SetValue("DisplayName", "PodsDesk"); key.SetValue("DisplayVersion", "1.0.0"); key.SetValue("Publisher", "Dyskolos_");
            key.SetValue("InstallLocation", Root); key.SetValue("DisplayIcon", AppExe); key.SetValue("UninstallString", "\"" + Uninstaller + "\" --uninstall");
            key.SetValue("NoModify", 1, RegistryValueKind.DWord); key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
        }
    }
    static void Uninstall()
    {
        try
        {
            if (!SamePath(Assembly.GetExecutingAssembly().Location, Uninstaller)) throw new InvalidOperationException("Lance la désinstallation depuis les paramètres Applications de Windows.");
            if (IsInstalledProcessRunning()) throw new IOException("Ferme PodsDesk depuis son icône près de l’horloge avant de le désinstaller.");
            if (MessageBox.Show("Désinstaller PodsDesk ? Les préférences personnelles seront conservées.", "PodsDesk", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            RemovePayload(AppDir); RemoveOwnedShortcut(StartLink); RemoveOwnedShortcut(DesktopLink);
            using (var key = Registry.CurrentUser.OpenSubKey(RunKey, true))
            {
                var value = key == null ? null : key.GetValue("PodsDesk") as string;
                if (!String.IsNullOrEmpty(value) && value.StartsWith("\"" + AppExe + "\"", StringComparison.OrdinalIgnoreCase)) key.DeleteValue("PodsDesk", false);
            }
            Registry.CurrentUser.DeleteSubKeyTree(UninstallKey, false);
            MessageBox.Show("PodsDesk a été désinstallé. Les préférences personnelles ont été conservées.", "PodsDesk", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ScheduleSelfCleanup();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "PodsDesk — désinstallation", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
    static void ScheduleSelfCleanup()
    {
        string encodedPath = Convert.ToBase64String(Encoding.Unicode.GetBytes(Uninstaller));
        string code = "$taskPid=" + Process.GetCurrentProcess().Id + "; $target=[Text.Encoding]::Unicode.GetString([Convert]::FromBase64String('" + encodedPath + "')); " +
          "$expected=Join-Path $env:LOCALAPPDATA 'Programs\\PodsDesk\\Uninstall.exe'; " +
          "if ([IO.Path]::GetFullPath($target) -ne [IO.Path]::GetFullPath($expected)) { exit 1 }; " +
          "for($i=0;$i -lt 40;$i++){if(-not (Get-Process -Id $taskPid -ErrorAction SilentlyContinue)){break}; Start-Sleep -Milliseconds 250}; " +
          "Remove-Item -LiteralPath $target -Force -ErrorAction SilentlyContinue; $dir=Split-Path -Parent $target; " +
          "if((Test-Path -LiteralPath $dir) -and -not (Get-ChildItem -LiteralPath $dir -Force)){Remove-Item -LiteralPath $dir -Force -ErrorAction SilentlyContinue}";
        string encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(code));
        var p = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, @"WindowsPowerShell\v1.0\powershell.exe"), "-NoProfile -NonInteractive -EncodedCommand " + encoded);
        p.UseShellExecute=false; p.CreateNoWindow=true; p.WindowStyle=ProcessWindowStyle.Hidden; Process.Start(p);
    }

    sealed class SetupWindow : Form
    {
        readonly CheckBox desktop; readonly CheckBox launch; readonly Button install;
        public SetupWindow()
        {
            Text="Installer PodsDesk"; ClientSize=new Size(560,390); FormBorderStyle=FormBorderStyle.FixedDialog; MaximizeBox=false; StartPosition=FormStartPosition.CenterScreen;
            BackColor=Color.FromArgb(20,20,20); ForeColor=Color.FromArgb(243,243,241); Font=new Font("Segoe UI",9F); try{Icon=Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);}catch{}
            Controls.Add(Label("P O D S D E S K   /   1 . 0",11,FontStyle.Bold,32,30,480,22,Color.FromArgb(165,165,165)));
            Controls.Add(Label("Votre audio, simplement.",23,FontStyle.Bold,30,65,500,52,Color.White));
            Controls.Add(Label("Installe PodsDesk pour ce compte Windows, sans droits administrateur.",10,FontStyle.Regular,32,128,480,44,Color.FromArgb(190,190,190)));
            Controls.Add(Label("Windows x64 · .NET Desktop Runtime 10 requis",9,FontStyle.Regular,32,179,480,22,Color.FromArgb(150,150,150)));
            desktop=new CheckBox{Text="Créer un raccourci sur le bureau",Left=35,Top=225,Width=440,Checked=true,ForeColor=ForeColor,BackColor=BackColor};Controls.Add(desktop);
            launch=new CheckBox{Text="Ouvrir PodsDesk après l’installation",Left=35,Top=252,Width=440,Checked=true,ForeColor=ForeColor,BackColor=BackColor};Controls.Add(launch);
            install=new Button{Text="Installer",Left=390,Top=315,Width=135,Height=42,FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(240,240,238),ForeColor=Color.FromArgb(22,22,22),Font=new Font("Segoe UI",10,FontStyle.Bold)};install.FlatAppearance.BorderSize=0;install.Click+=InstallClick;Controls.Add(install);
            var cancel=new Button{Text="Annuler",Left=275,Top=315,Width=105,Height=42,FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(38,38,38),ForeColor=ForeColor};cancel.FlatAppearance.BorderColor=Color.FromArgb(65,65,65);cancel.Click+=delegate{Close();};Controls.Add(cancel);AcceptButton=install;CancelButton=cancel;
        }
        static Label Label(string text,float size,FontStyle style,int left,int top,int width,int height,Color color){return new Label{Text=text,Left=left,Top=top,Width=width,Height=height,ForeColor=color,Font=new Font("Segoe UI",size,style),BackColor=Color.Transparent};}
        void InstallClick(object sender, EventArgs e)
        {
            if (!RuntimeAvailable())
            {
                if (MessageBox.Show("Le .NET Desktop Runtime 10 x64 est nécessaire. Ouvrir la page officielle de Microsoft ?", "PodsDesk", MessageBoxButtons.YesNo, MessageBoxIcon.Information)==DialogResult.Yes)
                    Process.Start(new ProcessStartInfo(RuntimeUrl){UseShellExecute=true});
                return;
            }
            install.Enabled=false;
            try { Install(desktop.Checked); MessageBox.Show("PodsDesk 1.0 est installé.", "PodsDesk", MessageBoxButtons.OK, MessageBoxIcon.Information); if(launch.Checked) Process.Start(new ProcessStartInfo(AppExe){UseShellExecute=true}); Close(); }
            catch(Exception ex){MessageBox.Show(ex.Message, "PodsDesk — installation", MessageBoxButtons.OK, MessageBoxIcon.Error);install.Enabled=true;}
        }
    }
}
