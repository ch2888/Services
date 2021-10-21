/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/

namespace Microsoft.Dynamics.Retail.Pos.Printing
{
    using System;
    using LSRetailPosis.Settings.HardwareProfiles;
    using Microsoft.Dynamics.Retail.Pos.Contracts.Services;

    /// <summary>
    /// Defines a printer association, consisted of the printer, formulary information and the device type.
    /// </summary>
    internal sealed class PrinterAssociation
    {
        public IPrinter Printer { get; private set; }
        public FormInfo PrinterFormInfo { get; private set; }
        public DeviceTypes Type { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrinterAssociation"/> class.
        /// </summary>
        /// <param name="printer">The printer.</param>
        /// <param name="printerFormInfo">The printer form info.</param>
        public PrinterAssociation(IPrinter printer, FormInfo printerFormInfo, DeviceTypes type)
        {
            if (printer == null)
            {
                throw new ArgumentNullException("printer");
            }

            if (printerFormInfo == null)
            {
                throw new ArgumentNullException("receptProfileId");
            }

            this.Printer = printer;
            this.PrinterFormInfo = printerFormInfo;
            this.Type = type;
        }
    }
}
