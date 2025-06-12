﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SecureDrive.Models
{
    public class Configuration
    {
        [JsonIgnore]
        public bool IsConfigured { get; set; } = true;

        //Startup
        public bool AutoMount { get; set; } = true;
        public bool AutoStart { get; set; } = true;

        //Drive Parameter
        public String DriveLetter { get; set; }
        public String ServerURL { get; set; } = "http://192.168.3.113/remote.php/dav/files/admin/";
        public Int32 ServerType { get; set; } = 0;
        public String ServerLogin { get; set; }
        public String ServerPassword { get; set; }
        public Int32 KernelCacheMode { get; set; } = -1;
        public Int32 FlushMode { get; set; } = 1;
        public bool SyncOps { get; set; } = false;
        public bool PreLoading { get; set; } = true;
        public bool MountAsNetworkDrive { get; set; } = false;

        //Proxy
        public Int16 HTTPProxyMode { get; set; } = 0; //0 = no proxy, 1 = default proxy, 2 = user defined proxy
        public String ProxyURL { get; set; } = "";
        public bool UseProxyAuthentication { get; set; } = false;
        public String ProxyLogin { get; set; } = "";
        public String ProxyPassword { get; set; } = "";

        //Client cert
        public bool UseClientCertForAuthentication { get; set; } = false;
        public String CertStoreName { get; set; }
        public String CertStoreLocation { get; set; }
        public String CertSerial { get; set; }
        public List<String> Permission { get; set; }
        public String VolumeLabel { get; set; }
        public ulong quota { get; set; }
        public ulong size { get; set; }
    }
}
