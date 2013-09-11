using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace FNWifiLocatorService
{
    [RunInstaller(true)]
    public partial class FNInstaller : System.Configuration.Install.Installer
    {
        public FNInstaller()
        {
            InitializeComponent();
        }

        private void serviceInstaller1_AfterInstall(object sender, System.Configuration.Install.InstallEventArgs e)
        {

            ServiceController sc = new ServiceController(this.serviceInstaller1.ServiceName);
            sc.Start();
        }

    }
}
