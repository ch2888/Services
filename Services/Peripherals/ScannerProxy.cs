/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Interop.OposScanner;
using LSRetailPosis.Settings.HardwareProfiles;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Dynamics.Retail.Pos.Contracts.DataEntity;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
    /// <summary>
    /// This class acts as a scanner but it does so by acting as a proxy for multiple other scanners.  Thus
    /// multiple scanners operate and can be controled via this one scanner.
    /// </summary>
    [Export(typeof(IScanner))]
    public sealed class ScannerProxy : IScanner
    {
        public event ScannerMessageEventHandler ScannerMessageEvent;

        private Collection<IScanner> scanners;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScannerProxy"/> class.
        /// </summary>
        public ScannerProxy()
        {
            this.DeviceName = null;
            this.DeviceDescription = null;

            scanners = new Collection<IScanner>();

            scanners.Add(new Scanner(
                LSRetailPosis.Settings.HardwareProfiles.Scanner.DeviceType, 
                LSRetailPosis.Settings.HardwareProfiles.Scanner.DeviceName, 
                LSRetailPosis.Settings.HardwareProfiles.Scanner.DeviceDescription));

            scanners.Add(new Scanner(
                LSRetailPosis.Settings.HardwareProfiles.Scanner2.DeviceType, 
                LSRetailPosis.Settings.HardwareProfiles.Scanner2.DeviceName, 
                LSRetailPosis.Settings.HardwareProfiles.Scanner2.DeviceDescription));
        }

        /// <summary>
        /// Disable Scanner device for scan.
        /// </summary>
        public void DisableForScan()
        {
            foreach (IScanner scanner in scanners)
            {
                scanner.DisableForScan();
            }
        }

        /// <summary>
        /// Enable Scanner device for scan.
        /// </summary>
        public void ReEnableForScan()
        {
            foreach (IScanner scanner in scanners)
            {
                scanner.ReEnableForScan();
            }
        }

        /// <summary>
        /// Load the device.
        /// </summary>
        /// <exception cref="IOException"></exception>
        public void Load()
        {
            Exception caughtScannerException = null;

            foreach (IScanner scanner in scanners)
            {
                if (!scanner.IsActive)
                {   // On retry skip scanners that are already active

                    try
                    {
                        scanner.Load();
                        scanner.ScannerMessageEvent += new ScannerMessageEventHandler(scanner_ScannerMessageEvent);
                    }
                    catch (Exception ex)
                    {   // Save the exception for now so we can try to load the other scanners...
                        caughtScannerException = ex;
                    }
                }
            }

            if (caughtScannerException != null)
            {
                throw caughtScannerException;
            }
        }

        /// <summary>
        /// Unload the device.
        /// </summary>
        public void Unload()
        {
            foreach (IScanner scanner in scanners)
            {
                scanner.ScannerMessageEvent -= new ScannerMessageEventHandler(scanner_ScannerMessageEvent);
                scanner.Unload();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get 
            {
                // if any scanner is active then this scanner is active.
                return scanners.Any(s => s.IsActive);
            }
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

        private void scanner_ScannerMessageEvent(IScanInfo scanInfo)
        {
            if (ScannerMessageEvent != null)
            {
                ScannerMessageEvent(scanInfo);
            }
        }


    }
}
