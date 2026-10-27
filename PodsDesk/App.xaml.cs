using System.Threading;
using System.Windows;
namespace PodsDesk;
public partial class App : System.Windows.Application
{
    private Mutex? singleInstance;
    private bool ownsMutex;
    protected override void OnStartup(StartupEventArgs e)
    {
        singleInstance = new Mutex(true, @"Local\PodsDesk.SingleInstance.v1", out ownsMutex);
        if (!ownsMutex)
        {
            if (!e.Args.Contains("--background"))
                System.Windows.MessageBox.Show("PodsDesk est déjà ouvert. Retrouvez-le dans la zone de notification près de l’horloge.", "PodsDesk");
            Shutdown();
            return;
        }
        base.OnStartup(e);
    }
    protected override void OnExit(ExitEventArgs e)
    {
        if (ownsMutex) singleInstance?.ReleaseMutex();
        singleInstance?.Dispose();
        base.OnExit(e);
    }
}
