/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System;
using System.ComponentModel.Composition;
using LSRetailPosis;
using LSRetailPosis.Transaction;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Dynamics.Retail.Pos.Contracts.DataEntity;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
	/// <summary>
	/// Class implements IDualDisplay interface.
	/// </summary>
    [Export(typeof(IDualDisplay))]
    public class DualDisplay : IDualDisplay
	{

		#region Fields

		private DualDisplayForm dualDisplayForm;

		#endregion

		#region Public Methods

        public DualDisplay()
        {
            this.DeviceName = null;
            this.DeviceDescription = null;
        }

		/// <summary>
		/// Load the device.
		/// </summary>
		/// <exception cref="IOException"></exception>
		public void Load()
		{
			if (LSRetailPosis.Settings.HardwareProfiles.DualDisplay.Active == false)
				return;

			try
			{
                NetTracer.Information("Peripheral [DualDisplay] - Load");
                
                dualDisplayForm = new DualDisplayForm();
				dualDisplayForm.ShowFormOnDualDisplay();
			}
			catch (PosStartupException ex)
			{
                NetTracer.Information("Peripheral [DualDisplay] - Load - Pos Startup Exception: {0}", ex.Message);
                
                Unload();
				throw;
			}
			catch (Exception ex)
			{
                NetTracer.Information("Peripheral [DualDisplay] - Load - Exception: {0}", ex.Message);
			}

			IsActive = true;
		}

		/// <summary>
		/// Unload the device.
		/// </summary>
		public void Unload()
		{
			if (IsActive && dualDisplayForm != null)
			{
                NetTracer.Information("Peripheral [DualDisplay] - Device Released");
                
                dualDisplayForm.Dispose();
				dualDisplayForm = null;
				IsActive = false;
			}
		}

		/// <summary>
		/// Display transaction on device
		/// </summary>
		/// <param name="posTransaction">PosTransation</param>
		public void ShowTransaction(IPosTransaction posTransaction)
		{
            if (IsActive)
            {
                NetTracer.Information("Peripheral [DualDisplay] - Show Transaction");
                dualDisplayForm.ShowTransaction((PosTransaction)posTransaction);
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
