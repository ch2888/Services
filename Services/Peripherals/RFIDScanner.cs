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
using System.Text;
using LSRetailPosis.Settings.HardwareProfiles;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
	/// <summary>
	/// Class implements IRFIDScanner interface.
	/// </summary>
    [Export(typeof(IRadioFrequencyIDScanner))]
    public class RFIDScanner : IRadioFrequencyIDScanner
	{

		#region Fields


		/// <summary>
		/// RFID Scanner message event.
		/// </summary>
		public event RFIDScannerMessageEventHandler RFIDScannerMessageEvent
        {   // add empty handlers on unused event to avoid compiler warnings CS0535 and CS0667.  See http://blogs.msdn.com/b/trevor/archive/2008/08/14/c-warning-cs0067-the-event-event-is-never-used.aspx
			add { }
			remove { }
		}

		#endregion

		#region Methods

        public RFIDScanner()
        {
            this.DeviceName = LSRetailPosis.Settings.HardwareProfiles.RFIDScanner.DeviceName;
            this.DeviceDescription = LSRetailPosis.Settings.HardwareProfiles.RFIDScanner.DeviceDescription;
        }

		/// <summary>
		/// Load the device.
		/// </summary>
		/// <exception cref="IOException"></exception>
		public void Load()
		{
			if (LSRetailPosis.Settings.HardwareProfiles.RFIDScanner.DeviceType != DeviceTypes.OPOS)
				return;

            // Add code here to enable the device then set:
            // IsActive = true;
		}

		/// <summary>
		/// Unload the device.
		/// </summary>
		public void Unload()
		{
			if (!IsActive)
				return;

            IsActive = false;
		}

		/// <summary>
		/// Disable RFID device for scan.
		/// </summary>
		public void DisableForScan()
		{
			if (!IsActive)
				return;
		}

		/// <summary>
		/// Enable RFID device for scan.
		/// </summary>
		public void ReEnableForScan()
		{
			if (!IsActive)
				return;
		}

		/// <summary>
		/// Concluded RFIDs.
		/// </summary>
		public void ConcludeRFID()
		{
			if (!IsActive)
				return;
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
