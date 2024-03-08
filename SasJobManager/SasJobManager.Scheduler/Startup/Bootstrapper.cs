using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Prism.Events;
using SasJobManager.Scheduler.Models;
using SasJobManager.Domain;
using SasJobManager.Scheduler.ViewModels;

namespace SasJobManager.Scheduler.Startup
{
    public class Bootstrapper
    {
        public IContainer Bootstrap()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<MainWindow>().AsSelf();
            builder.RegisterType<MainViewModel>().AsSelf();
            builder.RegisterType<SasContext>().AsSelf();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();

            return builder.Build();
        }
    }
}
