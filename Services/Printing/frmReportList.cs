/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System;
using System.Collections.Generic;
using System.Drawing;
using LSRetailPosis;
using LSRetailPosis.POSControls.Touch;

namespace Microsoft.Dynamics.Retail.Pos.Printing
{
    public partial class frmReportList : frmTouchBase
    {
        private ICollection<Point> signaturePoints;

        protected frmReportList()
        {
            // Required for Windows Form Designer support
            InitializeComponent();

            printWindow.Properties.ReadOnly = true;
            printWindow.BackColor           = Color.White;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="reportLayout">Report layout string</param>
        public frmReportList(string reportLayout, ICollection<Point> signaturePoints)
            : this()
        {
            printWindow.Text = reportLayout;
            this.signaturePoints = signaturePoints;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!this.DesignMode)
            {
                //Set the Window Size as per Visual Profile
                if ((LSRetailPosis.Settings.ApplicationSettings.MainWindowWidth != 0) && (LSRetailPosis.Settings.ApplicationSettings.MainWindowHeight != 0))
                {
                    this.Bounds = new Rectangle(
                        LSRetailPosis.Settings.ApplicationSettings.MainWindowLeft,
                        LSRetailPosis.Settings.ApplicationSettings.MainWindowTop,
                        LSRetailPosis.Settings.ApplicationSettings.MainWindowWidth,
                        LSRetailPosis.Settings.ApplicationSettings.MainWindowHeight
                        );
                }

                //
                // Get all text through the Translation function in the ApplicationLocalizer
                //
                // TextID's are reserved at 53000 - 53199
                //
                this.Text = ApplicationLocalizer.Language.Translate(51310); // Print Preview
                btnPrint.Text = ApplicationLocalizer.Language.Translate(53000); // Print
                btnCancel.Text = ApplicationLocalizer.Language.Translate(53001); // Cancel
                lblHeader.Text = ApplicationLocalizer.Language.Translate(51310); // Print Preview

                if (this.signaturePoints != null)
                {
                    this.signatureViewer.DrawSignature(this.signaturePoints, 20);
                }
            }

            base.OnLoad(e);
        }
    }
}
