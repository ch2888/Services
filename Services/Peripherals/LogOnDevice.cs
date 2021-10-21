/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Media;
using System.Security.Cryptography;
using System.Threading;
using LSRetailPosis;
using LSRetailPosis.DataAccess;
using LSRetailPosis.Settings;
using LSRetailPosis.Settings.FunctionalityProfiles;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Dynamics.Retail.Pos.Contracts.DataEntity;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;
using Microsoft.Dynamics.Retail.Pos.DataEntity;
using Microsoft.Win32;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
    [Export(typeof(ILogOnDevice))]
    public sealed class LogOnDevice : ILogOnDevice
    {

        #region Fields

        /// <summary>
        /// Log on device message event.
        /// </summary>
        public event LogOnDeviceEventHandler DataReceived;

        // Supported logon devices
        private IMSR magneticStripeReader;
        private IScanner barcodeReader;
        private IBiometricDevice biometricReader;

        private int captureReferenceCounter;

        #endregion

        #region Properties

        /// <summary>
        /// Device Name (may be null or empty)
        /// </summary>
        public string DeviceName
        {
            get;
            set;
        }

        /// <summary>
        /// Device Description (may be null or empty)
        /// </summary>
        public string DeviceDescription
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load the device.
        /// </summary>
        public void Load()
        {
            // MSR reader.
            magneticStripeReader = Peripherals.InternalApplication.Services.Peripherals.MSR;

            if (Functions.StaffCardLogOn && magneticStripeReader.IsActive)
            {
                magneticStripeReader.MSRMessageEvent += new MSRMessageEventHandler(OnMSR_DataReceived);
            }

            // Bar code scanner
            barcodeReader = Peripherals.InternalApplication.Services.Peripherals.Scanner;
            if (Functions.StaffBarcodeLogOn && barcodeReader.IsActive)
            {
                barcodeReader.ScannerMessageEvent += new ScannerMessageEventHandler(OnBarcode_DataReceived);
            }

            biometricReader = ((Peripherals)Peripherals.InternalApplication.Services.Peripherals).BiometricDevice;
            if (biometricReader !=null && biometricReader.IsActive)
            {
                biometricReader.DataReceived += new BiometricDeviceDataEventHandler(OnBiometricReader_DataReceived);
                biometricReader.StatusUpdate += new BiometricDeviceStatusEventHandler(OnBiometricReader_StatusUpdate);
            }

            IsActive = true;
        }

        /// <summary>
        /// Unload the device.
        /// </summary>
        public void Unload()
        {
            IsActive = false;

            EndCapture();

            magneticStripeReader.MSRMessageEvent -= new MSRMessageEventHandler(OnMSR_DataReceived);
            barcodeReader.ScannerMessageEvent -= new ScannerMessageEventHandler(OnBarcode_DataReceived);

            if (biometricReader != null && biometricReader.IsActive)
            {
                biometricReader.DataReceived -= new BiometricDeviceDataEventHandler(OnBiometricReader_DataReceived);
                biometricReader.StatusUpdate -= new BiometricDeviceStatusEventHandler(OnBiometricReader_StatusUpdate);
            }
        }

        /// <summary>
        /// Begins the enroll capture.
        /// </summary>
        public void BeginEnrollCapture()
        {
            if (Interlocked.Increment(ref captureReferenceCounter) == 1) // Count the captures in case of nested calls, we don't need to enable this everytime.
            {
                magneticStripeReader.EnableForSwipe();
                barcodeReader.ReEnableForScan();
             
                if (biometricReader != null)
                {
                    biometricReader.BeginEnrollCapture();
                }
            }

            NetTracer.Information("LogOnDevice : BeginEnrollCapture() [Counter : {0}].", captureReferenceCounter);
        }

        /// <summary>
        /// Begins the identify capture.
        /// </summary>
        public void BeginVerifyCapture()
        {
            if (Interlocked.Increment(ref captureReferenceCounter) == 1)
            {
                magneticStripeReader.EnableForSwipe();
                barcodeReader.ReEnableForScan();

                if (biometricReader != null)
                {
                    biometricReader.BeginVerifyCapture();
                }
            }

            NetTracer.Information("LogOnDevice : BeginVerifyCapture() [Counter : {0}].", captureReferenceCounter);
        }

        /// <summary>
        /// Ends the capture.
        /// </summary>
        public void EndCapture()
        {
            // End capture when reference count is 0
            // Reset the reference to zero if its negative, since there might have been multiple EndCaptures which can make reference count negative
            if (Interlocked.Decrement(ref captureReferenceCounter) <= 0) 
            {
                Interlocked.Exchange(ref captureReferenceCounter, 0);
                magneticStripeReader.DisableForSwipe();
                barcodeReader.DisableForScan();

                if (biometricReader != null)
                {
                    biometricReader.EndCapture();
                }
            }

            NetTracer.Information("LogOnDevice : EndCapture(). [Counter : {0}]", captureReferenceCounter);
        }

        public void CaptureSample()
        {
            if (biometricReader != null)
            {
                biometricReader.CaptureSample() ;
            }
        }

        /// <summary>
        /// Identifies the specified capture data.
        /// </summary>
        /// <param name="captureData">The capture data.</param>
        /// <returns>The matched staff ID if found, null otherwise</returns>
        public string Identify(IExtendedLogOnInfo captureData)
        {
            const string SoundSuccess = "HubOnSound";
            const string SoundFail = "HubOffSound";

            string staffID = null;

            if (captureData != null && !string.IsNullOrWhiteSpace(captureData.LogOnKey))
            {
                LogonData logonData = new LogonData(ApplicationSettings.Database.LocalConnection, ApplicationSettings.Database.DATAAREAID);
                string logOnKey = captureData.LogOnKey;

                switch (captureData.LogOnType)
                {
                    case ExtendedLogOnType.Barcode:
                    case ExtendedLogOnType.MagneticStripeReader:
                        // For these devices, logKey is directy mapped.
                        break;

                    case ExtendedLogOnType.Biometric:
                        // Delegate identification to biometric device specific engine.
                        logOnKey = biometricReader.Identify(captureData, logonData.GetExtendedLogOnExtraData(ExtendedLogOnType.Biometric, ApplicationSettings.Terminal.StorePrimaryId));
                        break;

                    default:
                        throw new NotSupportedException(string.Format("Log on type is not supported: {0}", captureData.LogOnType));
                }

                if (!string.IsNullOrWhiteSpace(logOnKey))
                {
                    // Create a logon key hash 
                    string logonKeyHash = LSRetailPosis.DataAccess.LogonData.ComputePasswordHash(((int)captureData.LogOnType).ToString(), logOnKey, ApplicationSettings.Terminal.StaffPasswordHashName);
                    staffID = logonData.GetStaffIDWithLogOnKey(logonKeyHash, captureData.LogOnType);
                }
            }

            if (!string.IsNullOrWhiteSpace(staffID))
            {
                PlaySound(SoundSuccess);
            }
            else 
            {
                // Only play sound for non picture messages
                if (captureData != null && captureData.ExtraData == null)
                {
                    PlaySound(SoundFail);
                    NetTracer.Information("Unrecognized log on key provided.");
                }
            }

            return staffID;
        }

        #endregion

        #region Private Methods

        private void FireDataReceived(IExtendedLogOnInfo extendedLogOnInfo)
        {
            LogOnDeviceEventHandler eventHandler = DataReceived;

            if (eventHandler != null)
            {
                foreach (Delegate listener in eventHandler.GetInvocationList())
                {
                    ISynchronizeInvoke invoker = listener.Target as ISynchronizeInvoke;

                    if (invoker != null && invoker.InvokeRequired)
                    {   // Marshal to the UI thread
                        invoker.EndInvoke(invoker.BeginInvoke(listener, new object[] { extendedLogOnInfo }));
                    }
                    else
                    {
                        listener.DynamicInvoke(new object[] { extendedLogOnInfo });
                    }
                }
            }
        }

        private void OnMSR_DataReceived(ICardInfo cardInfo)
        {
            ExtendedLogOnInfo extendedLogOnInfo = new ExtendedLogOnInfo()
            {
                LogOnType = ExtendedLogOnType.MagneticStripeReader,
                LogOnKey = cardInfo.CardNumber,
                Message = ApplicationLocalizer.Language.Translate(99409), // Card swipe accepted.
                PasswordRequired = Functions.StaffCardLogOnRequiresPassword
            };

            FireDataReceived(extendedLogOnInfo);

            magneticStripeReader.EnableForSwipe();
        }

        private void OnBarcode_DataReceived(IScanInfo scanInfo)
        {
            ExtendedLogOnInfo extendedLogOnInfo = new ExtendedLogOnInfo()
            {
                LogOnType = ExtendedLogOnType.Barcode,
                LogOnKey = scanInfo.ScanData,
                Message = ApplicationLocalizer.Language.Translate(99408), // Bar code accepted.
                PasswordRequired = Functions.StaffBarcodeLogOnRequiresPassword
            };

            FireDataReceived(extendedLogOnInfo);

            // Barcode scanner is auto-disabled, we need to enable again after event.
            barcodeReader.ReEnableForScan();
        }
        
        private void OnBiometricReader_DataReceived(string key, byte[] template)
        {
            ExtendedLogOnInfo extendedLogOnInfo = new ExtendedLogOnInfo()
            {
                LogOnType = ExtendedLogOnType.Biometric,
                LogOnKey = key,
                Message = ApplicationLocalizer.Language.Translate(99413), // Biometric data accepted.
                ExtraData = template,
                PasswordRequired = false
            };

            FireDataReceived(extendedLogOnInfo);
        }

        private void OnBiometricReader_StatusUpdate(string message, IEnumerable<byte> extraData = null)
        {
            ExtendedLogOnInfo extendedLogOnInfo = new ExtendedLogOnInfo()
            {
                LogOnType = ExtendedLogOnType.Biometric,
                DataStream = extraData,
                Message = message
            };

            FireDataReceived(extendedLogOnInfo);
        }

        /// <summary>
        /// Plays a given system sound theme.
        /// </summary>
        /// <param name="soundThemeName">Name of the sound theme.</param>
        private static void PlaySound(string soundThemeName)
        {
            const string SoundThemeRegKeyRoot = @"AppEvents\Schemes\Apps\sapisvr"; // 'Speech Recognition' Group
            const string CurrentSoundFileKey = ".Current";

            try
            {
                using (RegistryKey soundThemeKey = Registry.CurrentUser.OpenSubKey(Path.Combine(SoundThemeRegKeyRoot, soundThemeName, CurrentSoundFileKey)))
                {
                    if (soundThemeKey != null)
                    {
                        string soundFileName = soundThemeKey.GetValue(string.Empty) as string; // Default value of key is Sound file path.

                        if (!string.IsNullOrWhiteSpace(soundFileName) && File.Exists(soundFileName))
                        {
                            using (SoundPlayer soundPlayer = new SoundPlayer(soundFileName))
                            {
                                soundPlayer.Play();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NetTracer.Warning(ex, "LogOnDevice:PlaySound failed.");
            }
        }

        #endregion

    }

}
