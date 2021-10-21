/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing;
using Interop.OposConstants;
using Interop.OposSigCap;
using LSRetailPosis.Settings.HardwareProfiles;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Dynamics.Retail.Pos.Contracts.DataEntity;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
    /// <summary>
    /// Class implements ISignatureCapture interface.
    /// </summary>
    [Export(typeof(ISignatureCapture))]
    public sealed class SignatureCapture : ISignatureCapture
    {
        private OPOSSigCapClass oposSigCapClass;

        #region ISignatureCapture Members

        /// <summary>
        /// Gets a flag to identify that device is able to display form or data entry screen.
        /// <remarks>This property is initialized by the Load method.</remarks>
        /// </summary>
        public bool CapDisplay
        {
            get; 
            private set;
        }

        /// <summary>
        /// Gets a flag to identify that device is able to supply signature data real time.
        /// <remarks>This property is initialized by the Load method.</remarks>
        /// </summary>
        public bool CapRealTimeData
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a flag to identify that user is able to terminate capture by checking a completion box, pressing a completion button, or perfoming some other interaction with the device.
        /// <remarks>This property is initialized by the Load method.</remarks>
        /// </summary>
        public bool CapUserTerminated
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the maximum horizontal coordinate of the signature capture device.
        /// <remarks>This property is initialized by the Load method.</remarks>
        /// </summary>
        public int MaximumX
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the maximum vertical coordinate of the signature capture device.
        /// <remarks>This property is initialized by the Load method.</remarks>
        /// </summary>
        public int MaximumY
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a flag to identify if device is ready for capture.
        /// </summary>
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
        /// <summary>
        /// Signature capture complete event.
        /// </summary>
        public event SignatureCaptureCompleteEventHandler CaptureCompleteEvent;

        /// <summary>
        /// Signature capture error event.
        /// </summary>
        public event SignatureCaptureErrorMessageEventHandler CaptureErrorEvent;

        public SignatureCapture()
        {
            this.DeviceName = LSRetailPosis.Settings.HardwareProfiles.SignatureCapture.DeviceName;
            this.DeviceDescription = LSRetailPosis.Settings.HardwareProfiles.SignatureCapture.DeviceDescription;
        }

        /// <summary>
        /// Open and claim the device.
        /// </summary>
        public void Load()
        {
            if (LSRetailPosis.Settings.HardwareProfiles.SignatureCapture.DeviceType == DeviceTypes.OPOS && !this.IsActive)
            {
                NetTracer.Information("Peripheral [SigCap] - OPOS device loading: {0}", DeviceName ?? "<Undefined>");

                int resultCode = 0;
                this.oposSigCapClass = new OPOSSigCapClass();

                this.oposSigCapClass.DataEvent += new _IOPOSSigCapEvents_DataEventEventHandler(oposSigCapClass_DataEvent);
                this.oposSigCapClass.ErrorEvent += new _IOPOSSigCapEvents_ErrorEventEventHandler(oposSigCapClass_ErrorEvent);

                resultCode = this.oposSigCapClass.Open(DeviceName);
                Peripherals.CheckResultCode(this, resultCode);

                resultCode = this.oposSigCapClass.ClaimDevice(Peripherals.ClaimTimeOut);
                Peripherals.CheckResultCode(this, resultCode);

                this.CapDisplay = this.oposSigCapClass.CapDisplay;
                this.CapRealTimeData = this.oposSigCapClass.CapRealTimeData;
                this.CapUserTerminated = this.oposSigCapClass.CapUserTerminated;
                this.MaximumX = this.oposSigCapClass.MaximumX;
                this.MaximumY = this.oposSigCapClass.MaximumY;

                this.IsActive = true;
            }
        }

        /// <summary>
        /// Release and close device.
        /// </summary>
        public void Unload()
        {
            if (this.oposSigCapClass != null && this.IsActive)
            {
                NetTracer.Information("Peripheral [SigCap] - Device Released");

                this.oposSigCapClass.DataEvent -= new _IOPOSSigCapEvents_DataEventEventHandler(oposSigCapClass_DataEvent);
                this.oposSigCapClass.ErrorEvent -= new _IOPOSSigCapEvents_ErrorEventEventHandler(oposSigCapClass_ErrorEvent);

                this.oposSigCapClass.ReleaseDevice();
                this.oposSigCapClass.Close();
                this.IsActive = false;
            }
        }

        /// <summary>
        /// Enable device for capture.
        /// </summary>
        public void BeginCapture()
        {
            if (this.oposSigCapClass != null && this.IsActive)
            {
                NetTracer.Information("Peripheral [SigCap] - Begin Capture");

                this.oposSigCapClass.DeviceEnabled = true;
                this.oposSigCapClass.DataEventEnabled = true;

                int result = this.oposSigCapClass.BeginCapture(LSRetailPosis.Settings.HardwareProfiles.SignatureCapture.FormName);
                Peripherals.CheckResultCode(this, result);
            }
        }

        /// <summary>
        /// Disable device for capture.
        /// </summary>
        public void EndCapture()
        {
            if (this.oposSigCapClass != null && this.IsActive)
            {
                NetTracer.Information("Peripheral [SigCap] - End Capture");

                this.oposSigCapClass.DeviceEnabled = false;
                this.oposSigCapClass.DataEventEnabled = false;
            }
        }

        #endregion

        #region Private Methods

        private void oposSigCapClass_ErrorEvent(int resultCode, int resultCodeExtended, int errorLocus, ref int pErrorResponse)
        {
            NetTracer.Warning("Peripheral [SigCap] - Error Event Result Code: {0} ExtendedResultCode: {1}", resultCode, resultCodeExtended);

            if (resultCode != (int)OPOS_Constants.OPOS_SUCCESS)
            {
                if (this.CaptureErrorEvent != null)
                {
                    this.CaptureErrorEvent(Enum.GetName(typeof(OPOS_Constants), resultCode));
                }

                pErrorResponse = (int)OPOS_Constants.OPOS_ER_CLEAR;

                if (this.oposSigCapClass.DeviceEnabled)
                {
                    this.EndCapture();
                }
            }
        }

        private void oposSigCapClass_DataEvent(int status)
        {
            NetTracer.Information("Peripheral [SigCap] - Data Event: {0}", status);

            if (this.CaptureCompleteEvent != null && !string.IsNullOrEmpty(this.oposSigCapClass.PointArray))
            {
                // Verifone Only: ISignatureCaptureInfo signatureCaptureInfo = ParsePointArray(this.oposSigCapClass.RawData.Substring(10));
                // Point array returns very choppy data. We need to follow up on why it has low accuracy.
                ISignatureCaptureInfo signatureCaptureInfo = ParsePointArray(this.oposSigCapClass.PointArray);

                signatureCaptureInfo.StatusCode = status;
                this.CaptureCompleteEvent(this, signatureCaptureInfo);
            }
        }

        /// <summary>
        /// Convert point array string into array of points.
        /// </summary>
        /// <param name="pointArray">Point array string.</param>
        /// <returns>Returns ISignatureCaptureInfo.</returns>
        private static ISignatureCaptureInfo ParsePointArray(string pointArray)
        {
            ISignatureCaptureInfo signatureCaptureInfo = Peripherals.InternalApplication.BusinessLogic.Utility.CreateSignatureCaptureInfo();
            signatureCaptureInfo.Left = int.MaxValue;
            signatureCaptureInfo.Top = int.MaxValue;

            if (!string.IsNullOrWhiteSpace(pointArray))
            {
                Point point;
                int step = 4; // process 4 characters each step

                List<Point> points = new List<Point>(pointArray.Length / step);

                // Each point is represented by four characters: x(low 8 bits), x(hight 8 bits), y(low 8 bits), y(hight 8 bits)
                for (int i = 0; i + step <= pointArray.Length; i += step)
                {
                    point = GetPoint(pointArray[i], pointArray[i + 1], pointArray[i + 2], pointArray[i + 3]);

                    if (!IsEndPoint(point))
                    {
                        signatureCaptureInfo.Right = signatureCaptureInfo.Right < point.X ? point.X : signatureCaptureInfo.Right;
                        signatureCaptureInfo.Bottom = signatureCaptureInfo.Bottom < point.Y ? point.Y : signatureCaptureInfo.Bottom;
                        signatureCaptureInfo.Left = signatureCaptureInfo.Left > point.X ? point.X : signatureCaptureInfo.Left;
                        signatureCaptureInfo.Top = signatureCaptureInfo.Top > point.Y ? point.Y : signatureCaptureInfo.Top;
                    }

                    points.Add(point);
                }

                signatureCaptureInfo.Points = new ReadOnlyCollection<Point>(points);
            }

            return signatureCaptureInfo;
        }

        /// <summary>
        /// Get a point given the index
        /// </summary>
        /// <param name="index">0 based index</param>
        /// <returns>the point</returns>
        /// <exception>ArgumentOutOfRangeException</exception>
        private static Point GetPoint(char loXchar, char hiXchar, char loYchar, char hiYchar)
        {
            int x;
            int y;

            int loX = Microsoft.VisualBasic.Strings.Asc(loXchar);
            int hiX = Microsoft.VisualBasic.Strings.Asc(hiXchar);
            int loY = Microsoft.VisualBasic.Strings.Asc(loYchar);
            int hiY = Microsoft.VisualBasic.Strings.Asc(hiYchar);

            // NOTE: all values are unsigned
            x = (hiX << 8) | loX; // same as: hiX * 256 + loX;
            y = (hiY << 8) | loY; // same as: hiY * 256 + loY;

            if ((x == 0xffff) && (y == 0xffff))
            {   // End point
                x = -1;
                y = -1;
            }

            Point thePoint = new Point(x, y);
            
            return thePoint;
        }

        /// <summary>
        /// Determine if the point is an end-point
        /// </summary>
        /// <param name="testPoint">The test point</param>
        /// <returns></returns>
        private static bool IsEndPoint(Point testPoint)
        {
            return ((testPoint.X == -1) && (testPoint.Y == -1));
        }

        #endregion


    }
}
