using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NIPM_Tech_Support_Report_Generator
{
    class Constants
    {
        public string NipkgPath { get; private set; }
        public string NipmLocalAppDataPath { get; private set; }
        public readonly string CmdFullName = "cmd.exe";
        public readonly string ConfigCmd = "config-list";
        public readonly string ConfigName = "Configurations.txt";
        public readonly string InstalledPkgsCmd = "list-installed";
        public readonly string InstalledPkgName = "Installed Packages.txt";
        public readonly string AvailPkgsCmd = "list";
        public readonly string AvailPkgsName = "Available Packages.txt";
        public readonly string RegFeedsCmd = "feed-list";
        public readonly string RegFeedsName = "Registered Feeds.txt";

        public Constants(string cliPath, string nipmLocalAppDataPath)
        {
            this.NipkgPath = cliPath;
            this.NipmLocalAppDataPath = nipmLocalAppDataPath;
        }
    }
}
