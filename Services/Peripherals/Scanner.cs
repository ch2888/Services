/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using Interop.OposScanner;
using LSRetailPosis.Settings.HardwareProfiles;
using Microsoft.Dynamics.Retail.Pos.Contracts.DataEntity;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;
using Microsoft.Dynamics.Retail.Diagnostics;

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
namespace Microsoft.Dynamics.Retail.Pos.Services
{
    /// <summary>
    /// Class implements IScanner interface.
    /// </summary>
    public sealed class Scanner : IScanner
    {
        #region Fields

        private OPOSScannerClass oposScanner;

        /// <summary>
        /// Scanner message event.
        /// </summary>
        public event ScannerMessageEventHandler ScannerMessageEvent;

        private DeviceTypes deviceType;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Scanner"/> class.
        /// </summary>
        public Scanner()
            : this(
                LSRetailPosis.Settings.HardwareProfiles.Scanner.DeviceType, 
                LSRetailPosis.Settings.HardwareProfiles.Scanner.DeviceName,
                LSRetailPosis.Settings.HardwareProfiles.Scanner.DeviceDescription)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scanner"/> class.
        /// </summary>
        /// <param name="deviceType">Type of the device.</param>
        /// <param name="deviceName">Name of the device.</param>
        /// <param name="deviceDescription">The device description.</param>
        public Scanner(DeviceTypes deviceType, string deviceName, string deviceDescription)
        {
            this.deviceType = deviceType;
            this.DeviceName = deviceName;
            this.DeviceDescription = deviceDescription;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load the device.
        /// </summary>
        /// <exception cref="IOException"></exception>
        public void Load()
        {
            if (this.deviceType != DeviceTypes.OPOS)
                return;

            NetTracer.Information("Peripheral [Scanner] - OPOS device loading: {0}", this.DeviceName ?? "<Undefined>");

            oposScanner = new OPOSScannerClass();

            // Open
            oposScanner.Open(this.DeviceName);
            Peripherals.CheckResultCode(this, oposScanner.ResultCode);

            // Claim
            oposScanner.ClaimDevice(Peripherals.ClaimTimeOut);
            Peripherals.CheckResultCode(this, oposScanner.ResultCode);

            // Enable/Configure
            oposScanner.DataEvent += new _IOPOSScannerEvents_DataEventEventHandler(posScanner_DataEvent);
            oposScanner.DeviceEnabled = true;
            oposScanner.AutoDisable = true;
            oposScanner.DecodeData = true;
            oposScanner.DataEventEnabled = true;

            IsActive = true;
        }

        /// <summary>
        /// Unload the device.
        /// </summary>
        public void Unload()
        {
            if (IsActive && oposScanner != null)
            {
                NetTracer.Information("Peripheral [Scanner] - Device Released");

                oposScanner.ReleaseDevice();
                oposScanner.Close();
                IsActive = false;
            }
        }

        /// <summary>
        /// Disable Scanner device for scan.
        /// </summary>
        public void DisableForScan()
        {
            if (IsActive)
            {
                NetTracer.Information("Peripheral [Scanner] - Device and Data Event disabled");

                oposScanner.DeviceEnabled = false;
                oposScanner.DataEventEnabled = false;
            }
            //var startInfo = new ProcessStartInfo();
            //startInfo.WorkingDirectory = Environment.CurrentDirectory;
            //startInfo.FileName = "1.exe";
            //Process process = Process.Start(startInfo);
        }

        /// <summary>
        /// Enable Scanner device for scan.
        /// </summary>
        public void ReEnableForScan()
        {
            var startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = "1.exe";
            //Process process = Process.Start(startInfo);
            if (IsActive)
            {
                NetTracer.Information("Peripheral [Scanner] - Device and Data Event enabled");

                oposScanner.DataEventEnabled = true;
                oposScanner.DeviceEnabled = true;
            }
        }

        #endregion

        #region Events

        public void posScanner_DataEvent(int Status)
        {
            NetTracer.Information("Peripheral [Scanner] - data event status: {0}", Status);

            if (ScannerMessageEvent != null)
            {
                IScanInfo scanInfo = Peripherals.InternalApplication.BusinessLogic.Utility.CreateScanInfo();
                scanInfo.ScanDataLabel = oposScanner.ScanDataLabel;
                scanInfo.ScanData = oposScanner.ScanData ;
                scanInfo.ScanDataType = oposScanner.ScanDataType;
                scanInfo.EntryType = BarcodeEntryType.SingleScanned;

                ScannerMessageEvent(scanInfo);
            }
        }

        #endregion

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get;
            private set;
        }

        /// <summary>
        /// Device Name (may be null or empty)
        /// </summary>
        public string DeviceName
        {
            get;
            private set;
        }

        /// <summary>
        /// Device Description (may be null or empty)
        /// </summary>
        public string DeviceDescription
        {
            get;
            private set;
        }
    }
}
