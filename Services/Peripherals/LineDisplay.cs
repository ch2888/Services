/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using Interop.OposConstants;
using Interop.OposLineDisplay;
using LSRetailPosis;
using LSRetailPosis.Settings;
using LSRetailPosis.Settings.HardwareProfiles;
using Microsoft.Dynamics.Retail.Diagnostics;
using Microsoft.Dynamics.Retail.Notification.Contracts;
using Microsoft.Dynamics.Retail.Notification.Proxy;
using Microsoft.Dynamics.Retail.Pos.Contracts.DataEntity;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
    /// <summary>
    /// Class implements ILineDisplay interface.
    /// </summary>
    [Export(typeof(ILineDisplay))]
    public class LineDisplay : ILineDisplay
    {
        #region Fields

        private int DefaultLineDisplayWidth = 24; // Default line display width

        private IOPOSLineDisplay oposLineDisplay;
        private readonly int characterSet = Convert.ToInt32(LSRetailPosis.Settings.HardwareProfiles.LineDisplay.Characterset);
        private readonly string quantityFormat = ApplicationLocalizer.Language.Translate(6211);
        private INotificationCenter NotificationCenter { get; set; }

        private const char CharacterSetListSeparator = ',';

        #endregion

        #region Public Methods

        public LineDisplay()
        {
            this.DeviceName = LSRetailPosis.Settings.HardwareProfiles.LineDisplay.DeviceName;
            this.DeviceDescription = LSRetailPosis.Settings.HardwareProfiles.LineDisplay.DeviceDescription;
        }

        /// <summary>
        /// Load the device.
        /// </summary>
        /// <exception cref="IOException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public void Load()
        {
            NotificationCenter = NotificationProxy.GetNotificationCenter();

            if (LSRetailPosis.Settings.HardwareProfiles.LineDisplay.DeviceType != DeviceTypes.OPOS)
            {
                return;
            }

            NetTracer.Information("Peripheral [LineDisplay] - OPOS device loading: {0}", DeviceName ?? "<Undefined>");

            // If character set is not supported by OS, then error out.
            if (!Encoding.GetEncodings().Any(p => p.CodePage == characterSet))
            {
                throw new NotSupportedException(string.Format("Peripheral [LineDisplay] - Character set '{0}' is not supported by Windows OS", characterSet));
            }

            oposLineDisplay = new OPOSLineDisplayClass();

            //Open
            oposLineDisplay.Open(DeviceName);
            Peripherals.CheckResultCode(this, oposLineDisplay.ResultCode);

            // Claim
            oposLineDisplay.ClaimDevice(Peripherals.ClaimTimeOut);
            Peripherals.CheckResultCode(this, oposLineDisplay.ResultCode);

            // Enable/Configure
            oposLineDisplay.DeviceEnabled = true;

            // If character set is not supported by device, then disable and error out.
            if (!oposLineDisplay.CharacterSetList.Split(CharacterSetListSeparator).Any(p => p.Equals(characterSet.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                oposLineDisplay.ReleaseDevice();
                oposLineDisplay.Close();

                throw new NotSupportedException(string.Format("Peripheral [LineDisplay] - Character set '{0}' is not supported by device.", characterSet));
            }

            oposLineDisplay.CharacterSet = characterSet;
            IsActive = true;
        }

        /// <summary>
        /// Unload the device.
        /// </summary>
        public void Unload()
        {
            if (IsActive && oposLineDisplay != null)
            {
                NetTracer.Information("Peripheral [LineDisplay] - Device Released");

                oposLineDisplay.ReleaseDevice();
                oposLineDisplay.Close();
                IsActive = false;
            }
        }

        /// <summary>
        /// Clears the display.
        /// </summary>
        private void ClearDisplay()
        {
            if (IsActive)
            {
                oposLineDisplay.ClearText();
            }
        }

        /// <summary>
        /// Clear all text from line display.
        /// </summary>
        public void ClearText()
        {
            // Send notificaiton to clear text for LineDisplayEvent listners
            NotificationCenter.Notifications.GetEvent<LineDisplayEvent>().Publish(new LineDisplayEventData(ApplicationSettings.HostSessionId, true, new string[] { }));

            ClearDisplay();
        }

        /// <summary>
        /// Display one line on the line display.
        /// </summary>
        /// <param name="text"></param>
        public void DisplayText(string text)
        {
            // Send notificaiton to display text
            NotificationCenter.Notifications.GetEvent<LineDisplayEvent>().Publish(new LineDisplayEventData(ApplicationSettings.HostSessionId, true, new string[] { text }));

            if (IsActive)
            {
                ClearDisplay();
                DisplayTextAt(0, text);
            }
        }

        /// <summary>
        /// Display two lines on the line display.
        /// </summary>
        /// <param name="line1Text"></param>
        /// <param name="line2Text"></param>
        public void DisplayText(string line1Text, string line2Text)
        {
            // Send notificaiton to display text
            NotificationCenter.Notifications.GetEvent<LineDisplayEvent>().Publish(new LineDisplayEventData(ApplicationSettings.HostSessionId, true, new string[] { line1Text, line2Text }));

            if (IsActive)
            {
                ClearDisplay();
                DisplayTextAt(0, line1Text);
                DisplayTextAt(1, line2Text);
            }
        }

        /// <summary>
        /// Display sale line item on the line display.
        /// </summary>
        /// <param name="saleLineItem"></param>
        public void DisplayItem(ISaleLineItem saleLineItem)
        {
            if (saleLineItem == null)
                throw new ArgumentNullException("saleLineItem");

            decimal price = saleLineItem.NetAmount;
            string formattedQuantity = string.Format(quantityFormat,
                Peripherals.InternalApplication.Services.Rounding.RoundQuantity(saleLineItem.Quantity, saleLineItem.SalesOrderUnitOfMeasure),
                saleLineItem.SalesOrderUnitOfMeasure);
            string formattedPrice = Peripherals.InternalApplication.Services.Rounding.RoundForDisplay(price, true, false);

            DisplayText(
                saleLineItem.Description,
                AlignedText(formattedQuantity, formattedPrice));
        }

        /// <summary>
        /// Display sales total on the line display.
        /// </summary>
        /// <param name="amount"></param>
        public void DisplayTotal(string amount)
        {
            DisplayText(
                LSRetailPosis.Settings.HardwareProfiles.LineDisplay.TotalText,
                AlignedText(string.Empty, amount));
        }

        /// <summary>
        /// Display balance due on the line display.
        /// </summary>
        /// <param name="amount"></param>
        public void DisplayBalance(string amount)
        {
            DisplayText(
                LSRetailPosis.Settings.HardwareProfiles.LineDisplay.BalanceText,
                AlignedText(string.Empty, amount));
        }

        /// <summary>
        /// Display change due on the line display.
        /// </summary>
        /// <param name="amount"></param>
        public void DisplayChange(string amount)
        {
            DisplayText(
                ApplicationLocalizer.Language.Translate(2703), //Change
                AlignedText(string.Empty, amount));
        }

        #endregion

        #region Private Methods

        private void DisplayTextAt(int row, string textToDisplay)
        {
            if (LSRetailPosis.Settings.HardwareProfiles.LineDisplay.BinaryConversion)
            {
                oposLineDisplay.BinaryConversion = 2;  // OposBcDecimal
                textToDisplay = Peripherals.ConvertToBCD(textToDisplay, this.characterSet);
            }

            oposLineDisplay.DisplayTextAt(row, 0, textToDisplay, (int)OPOSLineDisplayConstants.DISP_DT_NORMAL);
            oposLineDisplay.BinaryConversion = 0;   // OposBcNone
        }

        private string AlignedText(string leftAlignedText, string rightAlignedText)
        {
            string spaceString = string.Empty;
            int lineLength;            
            int contentLength;

            if (IsActive)
            {
                lineLength = oposLineDisplay.Columns;
                contentLength =
                    Peripherals.GetByteCount(leftAlignedText, this.characterSet) +
                    Peripherals.GetByteCount(rightAlignedText, this.characterSet);
            }
            else
            {   // Still need to do computation for notificaiton message.
                lineLength = DefaultLineDisplayWidth; // Default to the # of standard # of char.  
                contentLength = leftAlignedText.Length + rightAlignedText.Length;
            }

            if (contentLength < lineLength)
            {
                spaceString = spaceString.PadRight(lineLength - contentLength);
            }

            return leftAlignedText + spaceString + rightAlignedText;
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
