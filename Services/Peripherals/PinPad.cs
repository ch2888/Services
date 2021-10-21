/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System;
using System.ComponentModel.Composition;
using Interop.OposPinpad;
using LSRetailPosis.Settings.HardwareProfiles;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Dynamics.Retail.Pos.Contracts.DataEntity;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
    /// <summary>
    /// Class implements IPinPad interface.
    /// </summary>
    [Export(typeof(IPinPad))]
    public class PinPad : IPinPad
    {
        #region Types

        /// <summary>
        /// UPOS EftTransactionType
        /// </summary>
        private enum EFTTransactionType
        {
            Debit = 1, // PPAD_TRANS_DEBIT
            Credit = 2, // PPAD_TRANS_CREDIT
            Inquiry = 3, // PPAD_TRANS_INQ
            Reconcile = 4, // PPAD_TRANS_RECONCILE
            Admin = 5  // PPAD_TRANS_ADMIN
        }

        /// <summary>
        /// UPOS EftTransactionCompletion
        /// </summary>
        private enum EFTTransactionCompletion
        {
            Normal = 1,
            Abnormal = 2
        }

        #endregion

        #region Fields

        private OPOSPINPadClass oposPinpad;
        private const string encryptionAlgorithm = "DUKPT";

        /// <summary>
        /// PinPad entry complete event.
        /// </summary>
        public event PinPadEntryCompleteEventHandler EntryCompleteEvent;

        #endregion

        #region Public Methods

        public PinPad()
        {
            this.DeviceName = LSRetailPosis.Settings.HardwareProfiles.PinPad.DeviceName;
            this.DeviceDescription = LSRetailPosis.Settings.HardwareProfiles.PinPad.DeviceDescription;
        }

        /// <summary>
        /// Load the device.
        /// </summary>
        /// <exception cref="IOException"></exception>
        public void Load()
        {
            if (LSRetailPosis.Settings.HardwareProfiles.PinPad.DeviceType != PinPadTypes.OPOS)
                return;

            NetTracer.Information("Peripheral [PinPad] - OPOS device loading: {0}", DeviceName ?? "<Undefined>");

            oposPinpad = new OPOSPINPadClass();

            // Open
            oposPinpad.Open(DeviceName);
            Peripherals.CheckResultCode(this, oposPinpad.ResultCode);

            // Claim
            oposPinpad.ClaimDevice(Peripherals.ClaimTimeOut);
            Peripherals.CheckResultCode(this, oposPinpad.ResultCode);

            // Configure/Enable
            oposPinpad.DataEvent += new _IOPOSPINPadEvents_DataEventEventHandler(posPinpad_DataEvent);
            oposPinpad.ErrorEvent += new _IOPOSPINPadEvents_ErrorEventEventHandler(posPinpad_ErrorEvent);
            oposPinpad.DeviceEnabled = true;

            IsActive = true;
        }

        /// <summary>
        /// Unload the device.
        /// </summary>
        public void Unload()
        {
            if (IsActive && oposPinpad != null)
            {
                NetTracer.Information("Peripheral [PinPad] - Device Released");

                oposPinpad.DataEvent -= new _IOPOSPINPadEvents_DataEventEventHandler(posPinpad_DataEvent);
                oposPinpad.ErrorEvent -= new _IOPOSPINPadEvents_ErrorEventEventHandler(posPinpad_ErrorEvent);

                oposPinpad.ReleaseDevice();
                oposPinpad.Close();
                IsActive = false;
            }
        }

        /// <summary>
        /// Begin transaction workflow on the claimed OPOS Pinpad device.
        /// Pinpad device will enable pin entry and request the user to follow debit workflows.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="accountNumber"></param>
        public void BeginTransaction(decimal amount, string accountNumber)
        {
            if (IsActive)
            {
                NetTracer.Information("Peripheral [PinPad] - Begin Transcation");

                // Clear any all device input that has been buffered (for data events)
                this.oposPinpad.ClearInput();

                this.oposPinpad.Amount = amount;
                //CheckResultCode();  // We know verifone has problems with amount.
                this.oposPinpad.AccountNumber = accountNumber;
                this.oposPinpad.TerminalID = LSRetailPosis.Settings.ApplicationSettings.Terminal.TerminalIdEFT;
                if (amount < 0)
                {   // Money is being refunded to the debit card - this is credit
                    this.oposPinpad.TransactionType = (int)EFTTransactionType.Credit;
                }
                else
                {   // Debit trans
                    this.oposPinpad.TransactionType = (int)EFTTransactionType.Debit;
                }

                this.oposPinpad.Track1Data = String.Empty;
                this.oposPinpad.Track2Data = String.Empty;
                this.oposPinpad.Track3Data = String.Empty;
                this.oposPinpad.Track4Data = String.Empty;

                this.oposPinpad.DataEventEnabled = true;

                // NOTE FILED PS#2802: Terminal Host not same as TerminalID
                int th = Convert.ToInt32(this.oposPinpad.TerminalID);

                // Try to do BeginEFTTransaction (if busy, do retry until available or TMO).
                // If we are going to support retry logic, we may need to check for device 
                // OPOS_E_BUSY with System.Threading.Thread.Sleep(2000);
                // 
                Peripherals.CheckResultCode(this, oposPinpad.BeginEFTTransaction(encryptionAlgorithm, th));
                Peripherals.CheckResultCode(this, oposPinpad.EnablePINEntry());
            }
        }

        /// <summary>
        /// End PinPad transaction.
        /// </summary>
        /// <param name="normal"></param>
        public void EndTransaction(bool normal)
        {
            if (IsActive)
            {
                NetTracer.Information("Peripheral [PinPad] - End Transcation");

                int resultCode = 0;

                if (normal)
                {
                    // Tell the device to end the EFT transcation (Normal)
                    resultCode = oposPinpad.EndEFTTransaction((int)EFTTransactionCompletion.Normal);
                }
                else
                {
                    // Tell the device to end the EFT transcation (Abonormal)
                    resultCode = oposPinpad.EndEFTTransaction((int)EFTTransactionCompletion.Abnormal);
                }

                Peripherals.CheckResultCode(this, resultCode);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the Data event received from POS PinPad device.
        /// Triggered if the customer successfully enters pin or cancel the transaction.
        /// </summary>
        /// <param name="status"></param>
        private void posPinpad_DataEvent(int status)
        {
            NetTracer.Information("Peripheral [PinPad] - Data Event");

            IPinPadInfo args = Peripherals.InternalApplication.BusinessLogic.Utility.CreatePinPadInfo();

            args.DataEvent = true;
            args.Status = (PinPadEntryStatus)status;
            args.EncryptedPIN = oposPinpad.EncryptedPIN;
            args.AdditionalSecurityInformation = oposPinpad.AdditionalSecurityInformation;

            if (EntryCompleteEvent != null)
                EntryCompleteEvent(this, args);
        }

        private void posPinpad_ErrorEvent(int ResultCode, int ResultCodeExtended, int ErrorLocus, ref int pErrorResponse)
        {
            NetTracer.Warning("Peripheral [PinPad] - Error Event Result Code: {0} ExtendedResultCode: {1}", ResultCode, ResultCodeExtended);

            IPinPadInfo args = Peripherals.InternalApplication.BusinessLogic.Utility.CreatePinPadInfo();

            args.DataEvent = false;
            args.Status = PinPadEntryStatus.Error;
            args.EncryptedPIN = string.Empty;
            args.AdditionalSecurityInformation = string.Empty;

            if (EntryCompleteEvent != null)
            {
                EntryCompleteEvent(this, args);
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
