using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using Autofac;
using Microsoft.Extensions.Configuration;
using SasJobManager.Scheduler.Startup;

namespace SasJobManager.Scheduler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public void Application_Startup(object sender, StartupEventArgs e)
        {

            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var mw = container.Resolve<MainWindow>();
            mw.Show();

        }
    }
}
