/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System.ComponentModel.Composition;
using Interop.OposMSR;
using LSRetailPosis.Settings.HardwareProfiles;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Dynamics.Retail.Pos.Contracts.DataEntity;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
    /// <summary>
    /// Class implements IMSR interface.
    /// </summary>
    [Export(typeof(IMSR))]
    public class MSR : IMSR
    {

        #region Fields

        private OPOSMSRClass oposMSR;

        /// <summary>
        /// Message event.
        /// </summary>
        public event MSRMessageEventHandler MSRMessageEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Get card number.
        /// </summary>
        public string CardNumber
        {
            get { return oposMSR.AccountNumber; }
        }

        /// <summary>
        /// Get card expiry date.
        /// </summary>
        public string ExpiryDate
        {
            get { return oposMSR.ExpirationDate; }
        }

        #endregion

        #region Public Methods

        public MSR()
        {
            this.DeviceName = LSRetailPosis.Settings.HardwareProfiles.MSR.DeviceName;
            this.DeviceDescription = LSRetailPosis.Settings.HardwareProfiles.MSR.DeviceDescription;
        }

        /// <summary>
        /// Load the device.
        /// </summary>
        /// <exception cref="IOException"></exception>
        public void Load()
        {
            if (LSRetailPosis.Settings.HardwareProfiles.MSR.DeviceType != DeviceTypes.OPOS)
                return;

            NetTracer.Information("Peripheral [MSR] - OPOS device loading: {0}", DeviceName ?? "<Undefined>");

            oposMSR = new OPOSMSRClass();

            //Open
            oposMSR.Open(DeviceName);
            Peripherals.CheckResultCode(this, oposMSR.ResultCode);

            //Claim
            oposMSR.ClaimDevice(Peripherals.ClaimTimeOut);
            Peripherals.CheckResultCode(this, oposMSR.ResultCode);

            //Enable/Configure
            oposMSR.DataEvent += new _IOPOSMSREvents_DataEventEventHandler(oposMSR_DataEvent);
            oposMSR.DeviceEnabled = true;
            // Override default configuraiton values as required by POS.
            oposMSR.AutoDisable = true;
            oposMSR.DecodeData = true;
            oposMSR.TransmitSentinels = true;

            // Wait for POS to enable the device...
            oposMSR.DeviceEnabled = false;
            oposMSR.DataEventEnabled = false;

            IsActive = true;
        }

        /// <summary>
        /// Unload the device.
        /// </summary>
        public void Unload()
        {
            if (IsActive && (oposMSR != null))
            {
                NetTracer.Information("Peripheral [MSR] - Device Released");

                oposMSR.DataEvent -= new _IOPOSMSREvents_DataEventEventHandler(oposMSR_DataEvent);
                oposMSR.ReleaseDevice();
                oposMSR.Close();
                IsActive = false;
            }
        }

        /// <summary>
        /// Enable MSR for swipe.
        /// Note: After each swipe the MSR disables itself.
        /// </summary>
        public void EnableForSwipe()
        {
            if (IsActive)
            {
                NetTracer.Information("Peripheral [MSR] - Device and Data Event enabled");

                oposMSR.DeviceEnabled = true;
                oposMSR.DataEventEnabled = true;
            }
        }

        /// <summary>
        /// Disable MSR for swipe.
        /// </summary>
        public void DisableForSwipe()
        {
            if (IsActive)
            {
                NetTracer.Information("Peripheral [MSR] - Device and Data Event disabled");

                oposMSR.DeviceEnabled = false;
                oposMSR.DataEventEnabled = false;
            }
        }

        #endregion

        #region Events

        void oposMSR_DataEvent(int Status)
        {
            NetTracer.Information("Peripheral [MSR] - data event status: {0}", Status);

            if (MSRMessageEvent != null)
            {
                ICardInfo cardInfo = Peripherals.InternalApplication.BusinessLogic.Utility.CreateCardInfo();
                cardInfo.Track1 = oposMSR.Track1Data;
                cardInfo.Track2 = oposMSR.Track2Data;
                cardInfo.Track3 = oposMSR.Track3Data;
                cardInfo.Track4 = oposMSR.Track4Data;

                MSRMessageEvent(cardInfo);
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
