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
using Interop.OposKeylock;
using LSRetailPosis.Settings.HardwareProfiles;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
	/// <summary>
	/// Class implements IKeyLock interface.
	/// </summary>
    [Export(typeof(IKeyLock))]
    public class KeyLock : IKeyLock
	{

		#region Fields

		private OPOSKeylockClass oposKeylock;

		/// <summary>
		/// Key Lock supervisor position event.
		/// </summary>
		public event KeyLockSupervisorPositionEventHandler KeyLockSupervisorPositionEvent;

		#endregion

		#region Public Methods

        public KeyLock()
        {
            this.DeviceName = LSRetailPosis.Settings.HardwareProfiles.Keylock.DeviceName;
            this.DeviceDescription = LSRetailPosis.Settings.HardwareProfiles.Keylock.DeviceDescription;
        }

		/// <summary>
		/// Load the device.
		/// </summary>
		/// <exception cref="IOException">Cannot load device</exception>
		public void Load()
		{
			if (Keylock.DeviceType != DeviceTypes.OPOS)
				return;

            NetTracer.Information("Peripheral [Keylock] - OPOS device loading: {0}", DeviceName ?? "<Undefined>");

            oposKeylock = new OPOSKeylockClass();

			//Open
			oposKeylock.Open(DeviceName);
			Peripherals.CheckResultCode(this, oposKeylock.ResultCode);

			//Enable
			oposKeylock.StatusUpdateEvent += new _IOPOSKeylockEvents_StatusUpdateEventEventHandler(posKeylock_StatusUpdateEvent);
			oposKeylock.DeviceEnabled = true;

			IsActive = true;
		}

		/// <summary>
		/// Unload the device.
		/// </summary>
		public void Unload()
		{
			if (IsActive && oposKeylock != null)
			{
                NetTracer.Information("Peripheral [Keylock] - Device Released");
                
                oposKeylock.StatusUpdateEvent -= new _IOPOSKeylockEvents_StatusUpdateEventEventHandler(posKeylock_StatusUpdateEvent);
				oposKeylock.ReleaseDevice();
				oposKeylock.Close();
				IsActive = false;
			}
		}

		/// <summary>
		/// Check if the key is in the supervisor position.
		/// </summary>
		/// <returns>True for supervisor position, false otherwise.</returns>
		public bool SupervisorPosition()
		{
			bool result = false;

			if (IsActive && (oposKeylock.KeyPosition == (int)OPOSKeylockConstants.LOCK_KP_SUPR))
			{
				result = true;
			}

			return result;
		}

		/// <summary>
		///  Check if the key is in the locked position.
		/// </summary>
		/// <returns>True for locked position, false otherwise.</returns>
		public bool LockedPosition()
		{
			bool result = false;

			if (IsActive && (oposKeylock.KeyPosition == (int)OPOSKeylockConstants.LOCK_KP_LOCK))
			{
				result = true;
			}

			return result;
		}

		#endregion

		#region Event Methods

		private void posKeylock_StatusUpdateEvent(int Data)
		{
			if ((KeyLockSupervisorPositionEvent != null) && (Data == (int)OPOSKeylockConstants.LOCK_KP_SUPR))
			{
				KeyLockSupervisorPositionEvent();
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
