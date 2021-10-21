/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using LSRetailPosis.Settings.HardwareProfiles;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
    /// <summary>
    /// Class implements ICashDrawer ineterface.
    /// </summary>
    [Export(typeof(ICashDrawer))]
    public sealed class CashDrawerProxy : ICashDrawer
    {

        #region Fields

        /// <summary>
        /// Cash drawer message event.
        /// </summary>
        public event CashDrawerMessageEventHandler CashDrawerMessageEvent;

        /// <summary>
        /// CashDrawer name to device object dictionary.
        /// </summary>
        private Dictionary<string, CashDrawer> cashDrawers = new Dictionary<string, CashDrawer>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CashDrawerProxy"/> class.
        /// </summary>
        public CashDrawerProxy()
        {
            this.DeviceName = null;
            this.DeviceDescription = null;

            // Add enabled devices to list.
            if (Drawer.DeviceType != DeviceTypes.None)
            {
                cashDrawers[Drawer.DeviceName] = new CashDrawer(Drawer.DeviceType, Drawer.DeviceName, Drawer.DeviceDescription);
            }

            if (Drawer2.DeviceType != DeviceTypes.None)
            {
                cashDrawers[Drawer2.DeviceName] = new CashDrawer(Drawer2.DeviceType, Drawer2.DeviceName, Drawer2.DeviceDescription);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load the device.
        /// </summary>
        /// <exception cref="IOException">Cannot load device</exception>
        public void Load()
        {
            Exception caughtScannerException = null;

            foreach (CashDrawer cashDrawer in cashDrawers.Values)
            {
                if (!cashDrawer.IsActive)
                {   // On retry skip scanners that are already active

                    try
                    {
                        cashDrawer.Load();
                        cashDrawer.CashDrawerMessageEvent += new CashDrawerMessageEventHandler(cashDrawer_StatusUpdateEvent);
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
            foreach (CashDrawer cashDrawer in cashDrawers.Values)
            {
                cashDrawer.CashDrawerMessageEvent -= new CashDrawerMessageEventHandler(cashDrawer_StatusUpdateEvent);
                cashDrawer.Unload();
            }
        }

        /// <summary>
        /// Get the collection of properties of currently available cash drawers.
        /// </summary>
        /// <returns>
        /// Collection of tuples containing Names and description of available drawers.
        /// Tuple.Item1 = Drawer name
        /// Tuple.Item2 = Drawer description
        /// </returns>
        public ICollection<Tuple<string, string>> GetAvailableDrawers()
        {
            return (from cd in cashDrawers
                    select Tuple.Create<string, string>(cd.Value.DeviceName, cd.Value.DeviceDescription)).ToList();
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
                CashDrawer cashDrawer = GetCurrentDrawer();

                return cashDrawer != null ? cashDrawer.IsActive : false;
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

        /// <summary>
        /// Open the cash drawer.
        /// </summary>
        public void OpenDrawer()
        {
            if (LSRetailPosis.Settings.ApplicationSettings.Terminal.TrainingMode)
            {
                return;
            }

            if (Peripherals.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled())
            {
                Peripherals.InternalApplication.Services.Peripherals.FiscalPrinter.OpenDrawer();
                return;
            }

            CashDrawer cashDrawer = GetCurrentDrawer();

            if (cashDrawer != null)
            {
                GetCurrentDrawer().OpenDrawer();
            }
        }

        /// <summary>
        /// Check if cash drawer is open.
        /// </summary>
        /// <returns>True if open, false otherwise.</returns>
        public bool DrawerOpen()
        {
            if (Peripherals.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled())
            {
                return Peripherals.InternalApplication.Services.Peripherals.FiscalPrinter.DrawerOpen();
            }

            CashDrawer cashDrawer = GetCurrentDrawer();

            return cashDrawer != null ? cashDrawer.DrawerOpen() : false;
        }

        /// <summary>
        /// Check if the cash drawer is capable of reporting back whether it's closed or open.
        /// </summary>
        /// <returns>True if capable, false otherwise.</returns>
        public bool CapStatus()
        {
            if (Peripherals.InternalApplication.Services.Peripherals.FiscalPrinter.FiscalPrinterEnabled())
            {
                return Peripherals.InternalApplication.Services.Peripherals.FiscalPrinter.CapStatus();
            }

            CashDrawer cashDrawer = GetCurrentDrawer();

            return cashDrawer != null ? cashDrawer.CapStatus() : false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the current cash drawer.
        /// </summary>
        /// <returns>Current cash drawer if available.</returns>
        private CashDrawer GetCurrentDrawer()
        { 
            // If 0 or 1 cash drawer is configured, then behave like legacy. (Use the first, if available)
            if (cashDrawers.Count < 2)
            {
                return cashDrawers.FirstOrDefault().Value;
            }

            // If > 1 devices, but either shift is not available or shift doesn't has drawer, then return null (NoOp)
            if (Peripherals.InternalApplication.Shift == null || string.IsNullOrWhiteSpace(Peripherals.InternalApplication.Shift.CashDrawer))
            {
                return null;
            }

            CashDrawer cashDrawer;
            if (!this.cashDrawers.TryGetValue(Peripherals.InternalApplication.Shift.CashDrawer, out cashDrawer))
            {
                return null;
            }

            return cashDrawer;
        }

        #endregion

        #region Events

        void cashDrawer_StatusUpdateEvent(string data)
        {
            if (CashDrawerMessageEvent != null)
            {
                CashDrawerMessageEvent(data);
            }
        }

        #endregion

    }
}
