using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;

using GoDaddyRestAPI;

namespace gddnsserviceuserinfo
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Make sure that the dialog is in front
            Application.Current.MainWindow.Activate();

            // Verify we can write to the registry
            try
            {
                RegistryKey lkey = Registry.LocalMachine;

                lkey = lkey.CreateSubKey(@"SYSTEM\CurrentControlSet\services\GoDaddyDNSUpdate\Config");
            }
            catch (System.UnauthorizedAccessException)
            {
                this.Error.Content = "Unable to save values. Please run application with Administrator privilages.";                
            }
        }



        private void Button_Click_Cancel(object sender, RoutedEventArgs e)
        {                        
            //Non zero should abort installation 
            this.shuttingDown = true;
            Application.Current.Shutdown(1);
        }

        private static bool IsValidDomainName(string name)
        {
            if (!name.Contains('.'))
                return false;

            return Uri.CheckHostName(name) != UriHostNameType.Unknown;
        }

        private static bool IsValidSubDomainName(string domainName, string subDomainName)
        {
            if (subDomainName.Contains('.'))
                return false;

            return IsValidDomainName(subDomainName + "." + domainName);
        }
       
        private void Button_Click_OK(object sender, RoutedEventArgs e)
        {
            RegistryKey lkey = Registry.LocalMachine;

            // Verify we can write to the registry
            try
            {
                lkey = lkey.CreateSubKey(@"SYSTEM\CurrentControlSet\services\GoDaddyDNSUpdate\Config");
            }
            catch (System.UnauthorizedAccessException)
            {
                this.Error.Content = "Unable to save values. Please run application with Administrator privilages.";

                return;
            }

            string domainnameText = this.domainname.Text.Trim();
            string subdomainnameText = this.subdomainname.Text.Trim();
            string keyText = this.key.Text.Trim();
            string secretText = this.secret.Text.Trim();

            if (domainnameText == null || domainnameText == "" || !IsValidDomainName(domainnameText))
            {
                this.Error.Content = "Valid domain name is required.";

                return;
            }

            if (subdomainnameText != null && subdomainnameText != "" && !IsValidSubDomainName(domainnameText, subdomainnameText))
            {
                this.Error.Content = "Sub domain entered is not valid.";

                return;
            }

            if (secretText == null || secretText == "" || keyText == null || secretText == "")
            {
                this.Error.Content = "Key and Secret must be entered.";

                return;
            }
            
            // See if we can read from the domain
            string data = null;
            int status = 0;
        
            data = GoDaddyRestAPI.GoDaddyRestAPI.GetGoDaddyIP(domainnameText, keyText + ":" + secretText, "A", "", out status);
            if (status == 0 || status >= 500)
            {
                this.Error.Content = "Unable to connect to the service at this time.";
                return;
            }
            
            if (data == null)
            {
                this.Error.Content = "Key and Secret does not have access to this domain.";

                return;
            }            

            // Add domain name
            lkey.SetValue("Domain", domainnameText);

            // Add subdomain
            // Set value, none or blank in the dialog implies apex
            if (subdomainnameText == null || subdomainnameText == "")
                lkey.SetValue("AName", "@");
            else
                lkey.SetValue("AName", subdomainnameText);

            // Add key:secret 
            lkey.SetValue("ApiKey", keyText + ":" + secretText);

            // -i is passed from installer.  When run from command line, tell the user they need to restart
            string[] args = Environment.GetCommandLineArgs();            
            if (!args.Contains("-i"))
                MessageBox.Show("Settings applied. You must restart the GoDaddy DNS Update Service for changes to take affect.", "Restart Service");

            // Exit application
            this.shuttingDown = true;
            Application.Current.Shutdown(0);
        }

        private bool shuttingDown = false;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Non zero should abort installation 
            if (!this.shuttingDown)
                Application.Current.Shutdown(1);
        }
    }
}