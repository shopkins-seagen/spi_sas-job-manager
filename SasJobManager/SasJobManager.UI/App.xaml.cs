using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using Microsoft.Extensions.Configuration;
using SasJobManager.UI.Startup;

namespace SasJobManager.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    
    public partial class App : Application
    {
        private IConfigurationRoot _cfg;
    
        

        public void Application_Startup(object sender, StartupEventArgs e)
        {
            this.Dispatcher.UnhandledException += new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);

            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var mw = container.Resolve<MainWindow>();

            if (e.Args.Length > 0)
            {
             
                // Folder
                if (File.GetAttributes(e.Args[0]).HasFlag(FileAttributes.Directory))
                {
                    mw.Root = e.Args[0];
                }
                // File(s)
                else
                {
                    mw.Root = Path.GetDirectoryName(e.Args[0]);
                    mw.Files = e.Args.Select(x=>Path.GetFileName(x)).ToList();
                }

                mw.Show();
            }
            else
            {
              
            }
        }

        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                try
                {
                    using (var s = new StreamWriter($"{System.IO.Path.GetTempPath()}\\sjm-ui-ex.txt"))
                    {
                        s.WriteLine($"Dispatcher Error: {e.Exception.Message}\n");
                        s.WriteLine($"Stack Trace: {e.Exception.StackTrace}");
                    }
                }
                catch
                {
                    e.Handled = true;
                }
                finally
                {
                    e.Handled = true;
                }
            }
        }
    }
}
