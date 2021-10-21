/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System;
using System.IO;
using System.IO.Ports;
using Interop.OposCashDrawer;
using LSRetailPosis.Settings.HardwareProfiles;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
	/// <summary>
	/// Class implements CashDrawer device.
	/// </summary>
    public sealed class CashDrawer : IPeripheral
	{

		#region Fields

		private OPOSCashDrawerClass oposCashDrawer;

		// <ESC>p\0dd  -> 1B 70 5C 30 64 64
		private const string OPEN_DRAWER_SEQUENCE = "\x1B\x70\x5C\x30\x64\x64";

        /// <summary>
        /// Gets the type of the device.
        /// </summary>
        /// <value>
        /// The type of the device.
        /// </value>
        public DeviceTypes DeviceType { get; private set; }

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        public string DeviceName { get; private set; }

        /// <summary>
        /// Gets the device description.
        /// </summary>
        public string DeviceDescription { get; private set; }

		/// <summary>
		/// Cash drawer message event.
		/// </summary>
		public event CashDrawerMessageEventHandler CashDrawerMessageEvent;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CashDrawer"/> class.
        /// </summary>
        /// <param name="deviceType">Type of the device.</param>
        /// <param name="deviceName">Name of the device.</param>
        /// <param name="description">The description.</param>
        public CashDrawer(DeviceTypes deviceType, string deviceName, string description)
        {
            this.DeviceType = deviceType;
            this.DeviceName = deviceName;
            this.DeviceDescription = description;
        }

        #endregion

		#region Public Methods

		/// <summary>
		/// Load the device.
		/// </summary>
		/// <exception cref="IOException">Cannot load device</exception>
		public void Load()
		{
			if (this.DeviceType == DeviceTypes.None)
				return;
	
			if (this.DeviceType == DeviceTypes.OPOS)
			{
                NetTracer.Information("Peripheral [CashDrawer] - OPOS device loading: {0}", this.DeviceName ?? "<Undefined>");

				oposCashDrawer = new OPOSCashDrawerClass();

				// Open
				oposCashDrawer.Open(this.DeviceName);
				Peripherals.CheckResultCode(this, oposCashDrawer.ResultCode);

				// Claim
				oposCashDrawer.ClaimDevice(Peripherals.ClaimTimeOut);
				Peripherals.CheckResultCode(this, oposCashDrawer.ResultCode);

				// Enable
				oposCashDrawer.DeviceEnabled = true;
				oposCashDrawer.StatusUpdateEvent += new _IOPOSCashDrawerEvents_StatusUpdateEventEventHandler(posCashDrawer_StatusUpdateEvent);
			}
	
			IsActive = true;
		}

		/// <summary>
		/// Unload the device.
		/// </summary>
		public void Unload()
		{
			if (IsActive && oposCashDrawer != null)
			{
                NetTracer.Information("Peripheral [CashDrawer] - Device Released");
                
                oposCashDrawer.StatusUpdateEvent -= new _IOPOSCashDrawerEvents_StatusUpdateEventEventHandler(posCashDrawer_StatusUpdateEvent);
				oposCashDrawer.ReleaseDevice();
				oposCashDrawer.Close();
			}

            IsActive = false;
		}

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

            if (IsActive)
            {
                NetTracer.Information("Peripheral [CashDrawer] - Open drawer");

                switch (this.DeviceType)
                {
                    case DeviceTypes.OPOS:
                        oposCashDrawer.OpenDrawer();
                        break;

                    case DeviceTypes.Windows:
                        using (SerialPort port = new SerialPort(this.DeviceName, 9600, Parity.None, 8, StopBits.One))
                        {
                            port.Open();
                            port.Write(OPEN_DRAWER_SEQUENCE);
                        }
                        break;

                    case DeviceTypes.Manual:
                        // NoOp
                        break;
                }
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

			if (IsActive && (this.DeviceType == DeviceTypes.OPOS))
			{
				return oposCashDrawer.DrawerOpened;
			}
			else
			{
				return false;
			}
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

			if (IsActive && (this.DeviceType == DeviceTypes.OPOS))
			{
				return oposCashDrawer.CapStatus;
			}
			else
			{
				return false;
			}
		}

		#endregion

		#region Events

		void posCashDrawer_StatusUpdateEvent(int Data)
		{
			if (CashDrawerMessageEvent != null)
			{
				CashDrawerMessageEvent(Data.ToString());
			}
		}

		#endregion

   }
}
