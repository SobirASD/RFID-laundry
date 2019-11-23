﻿using Autofac;
using PALMS.Settings.ViewModel.AppSettings;
using PALMS.Settings.ViewModel.LaundryDetails;
using PALMS.ViewModels.Common;
using PALMS.ViewModels.Common.Services;

namespace PALMS.Settings.ViewModel
{
    public class SettingsModule : IIocModule
    {
        public void Register(ContainerBuilder container)
        {
            container.RegisterType<SettingsSection>().SingleInstance();

            container.Register(x => new SettingsViewModel(x.Resolve<TabsViewModel>(), x.Resolve<ICanExecuteMediator>()))
                .As<SettingsViewModel>()
                .SingleInstance();

            container.RegisterType<TabsViewModel>().SingleInstance();

            container.RegisterType<AppSettingsViewModel>().SingleInstance();
            container.RegisterType<AppSettingsProvider>().As<IAppSettingsProvider>().As<IAppSettings>().SingleInstance();

            container.RegisterType<VendorDetailsViewModel>().SingleInstance();
        }
    }
}