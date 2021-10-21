/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System;
using System.ComponentModel.Composition;
using Interop.OposConstants;
using Interop.OposScale;
using LSRetailPosis.Settings.HardwareProfiles;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
	/// <summary>
	/// Class implements IScale interface.
	/// </summary>
    [Export(typeof(IScale))]
    public class Scale : IScale
	{

		#region Fields

		private OPOSScaleClass oposScale;

		/// <summary>
		/// Scale data message event.
		/// </summary>
		public event ScaleDataMessageEventHandler ScaleDataMessageEvent;

		/// <summary>
		/// Scale error message event.
		/// </summary>
		public event ScaleErrorMessageEventHandler ScaleErrorMessageEvent;

		#endregion

        public Scale()
        {
            this.DeviceName = LSRetailPosis.Settings.HardwareProfiles.Scale.DeviceName;
            this.DeviceDescription = LSRetailPosis.Settings.HardwareProfiles.Scale.DeviceDescription;
        }

		#region IPeripheral Members

		/// <summary>
		/// Load the device.
		/// </summary>
		/// <exception cref="IOException"></exception>
		public void Load()
		{
			if (LSRetailPosis.Settings.HardwareProfiles.Scale.DeviceType != DeviceTypes.OPOS)
				return;

            NetTracer.Information("Peripheral [Scale] - OPOS device loading: {0}", DeviceName ?? "<Undefined>");

			oposScale = new OPOSScaleClass();

			// Open
			oposScale.Open(DeviceName);
			Peripherals.CheckResultCode(this, oposScale.ResultCode);

			// Claim
			oposScale.ClaimDevice(Peripherals.ClaimTimeOut);
			Peripherals.CheckResultCode(this, oposScale.ResultCode);

			// Enable/Configure
			oposScale.DataEvent += new _IOPOSScaleEvents_DataEventEventHandler(posScale_DataEvent);
			oposScale.ErrorEvent += new _IOPOSScaleEvents_ErrorEventEventHandler(posScale_ErrorEvent);
			oposScale.DeviceEnabled = true;
			oposScale.AsyncMode = true;
			oposScale.AutoDisable = true;
			oposScale.DataEventEnabled = true;
			oposScale.PowerNotify = (int)OPOS_Constants.OPOS_PN_ENABLED;

			IsActive = true;
		}

		/// <summary>
		/// Unload the device.
		/// </summary>
		public void Unload()
		{
			if (IsActive && oposScale != null)
			{
                NetTracer.Information("Peripheral [Scale] - Device Released");
                
                oposScale.DataEvent -= new _IOPOSScaleEvents_DataEventEventHandler(posScale_DataEvent);
				oposScale.ErrorEvent -= new _IOPOSScaleEvents_ErrorEventEventHandler(posScale_ErrorEvent);

				oposScale.ReleaseDevice();
				oposScale.Close();
				IsActive = false;
			}
		}

		/// <summary>
		/// Synchronously read from Scale.
		/// </summary>
		public void ReadFromScale()
		{
            if (IsActive)
            {
                NetTracer.Information("Peripheral [Scale] - Read Weight");

                int weight = 0;
                oposScale.DeviceEnabled = true;
                oposScale.DataEventEnabled = true;
                oposScale.AutoDisable = true;

                int result = oposScale.ReadWeight(out weight, LSRetailPosis.Settings.HardwareProfiles.Scale.TimeOut);

                if ((result != (int)OPOS_Constants.OPOS_SUCCESS) && ScaleErrorMessageEvent != null)
                {
                    ScaleErrorMessageEvent(((OPOS_Constants)result).ToString());
                }
            }
		}

		#endregion

		#region Private Methods

		private void posScale_ErrorEvent(int ResultCode, int ResultCodeExtended, int ErrorLocus, ref int pErrorResponse)
		{
            NetTracer.Warning("Peripheral [Scale] - Error Event Result Code: {0} ExtendedResultCode: {1}", ResultCode, ResultCodeExtended);
            
            if ((ResultCode != (int)OPOS_Constants.OPOS_SUCCESS) && ScaleErrorMessageEvent != null)
			{
				ScaleErrorMessageEvent(Enum.GetName(typeof(OPOS_Constants), ResultCode));

				pErrorResponse = (int)OPOS_Constants.OPOS_ER_CLEAR;
			}
		}

		private void posScale_DataEvent(int weight)
		{
            NetTracer.Information("Peripheral [Scale] - Data Event: {0}", weight);
			
            if (ScaleDataMessageEvent != null)
			{
				ScaleDataMessageEvent(weight);
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
