/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Microsoft.Dynamics.Retail.Pos.Printing
{
    public enum FormPart { Header = 0, Line = 1, Footer = 2 };

    public enum valign { left, center, right };

    /// <summary>
    /// Size of the font.
    /// </summary>
    public enum FormFontSize { Regular, Large };

    public class FormInfo
    {
        #region Member variables

        private bool printAsSlip; // = false;
        private int printBehaviour; // = 0;                 // Always print, do not print, ask user?
        private string header = "";
        private string details = "";
        private string footer = "";
        private int headerLines; // = 0;
        private bool reprint; // = false;

        private int detailLines; // = 0;
        private int footerLines; // = 0;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets property for HeaderTemplate.
        /// </summary>
        public DataSet HeaderTemplate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets  property for DetailsTemplate.
        /// </summary>
        public DataSet DetailsTemplate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets  property for FooterTemplate.
        /// </summary>
        public DataSet FooterTemplate
        {
            get;
            set;
        }

        /// <summary>
        /// Get set property for Header.
        /// </summary>
        public string Header
        {
            get { return header; }
            set { header = value; }
        }

        /// <summary>
        /// Get set property for details.
        /// </summary>
        public string Details
        {
            get { return details; }
            set { details = value; }
        }

        /// <summary>
        /// Get set property for Footer.
        /// </summary>
        public string Footer
        {
            get { return footer; }
            set { footer = value; }
        }

        /// <summary>
        /// Get set property for Headerlines.
        /// </summary>
        public int HeaderLines
        {
            get { return headerLines; }
            set { headerLines = value; }
        }

        /// <summary>
        /// Get set property for Reprint.
        /// </summary>
        public bool Reprint
        {
            get { return reprint; }
            set { reprint = value; }
        }

        /// <summary>
        /// Get set property for DetailLines.
        /// </summary>
        public int DetailLines
        {
            get { return detailLines; }
            set { detailLines = value; }
        }

        /// <summary>
        /// Get set property for FooterLines.
        /// </summary>
        public int FooterLines
        {
            get { return footerLines; }
            set { footerLines = value; }
        }


        /// <summary>
        /// Get set property for PrintAsSlip.
        /// </summary>
        public bool PrintAsSlip
        {
            get { return printAsSlip; }
            set { printAsSlip = value; }
        }

        /// <summary>
        /// Get set property for PrintBehaviour.
        /// </summary>
        public int PrintBehaviour
        {
            get { return printBehaviour; }
            set { printBehaviour = value; }
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="printAsSlip"></param>
        /// <param name="printBehaviour"></param>
        public FormInfo(bool printAsSlip, int printBehaviour)
        {
            this.printAsSlip = printAsSlip;
            this.printBehaviour = printBehaviour;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public FormInfo()
        {
        }
    }
}
