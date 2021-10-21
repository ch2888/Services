/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using LSRetailPosis.Settings.FunctionalityProfiles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Dynamics.Retail.Diagnostics;

namespace Microsoft.Dynamics.Retail.Pos.Printing
{
    public class FormItemInfo
    {
        private int length;
        private int sizeFactor = 1;

        /// <summary>
        /// Get property for SizeFactor.
        /// </summary>
        public int SizeFactor
        {
            get
            {
                return sizeFactor;
            }
        }

        /// <summary>
        /// Get Image Id of the logo
        /// </summary>
        public int ImageId
        {
            get; set;
        }

        /// <summary>
        /// Get/set property for Variable.
        /// </summary>
        public string Variable { get; set; }        

        /// <summary>
        /// Get/set property for IsVariable.
        /// </summary>        
        public bool IsVariable { get; set; }

        /// <summary>
        /// Get/set property for LineIndex.
        /// </summary>
        public int LineIndex { get; set; }

        /// <summary>
        /// Get/set property for CharIndex.
        /// </summary>
        public int CharIndex { get; set; }   

        /// <summary>
        /// Get/set property for ValueString.
        /// </summary>
        public string ValueString { get; set; }
      
        /// <summary>
        /// Get/set property for VertAlign.
        /// </summary>
        public valign VertAlign { get; set; }     

        /// <summary>
        /// Get/set property for Fill.
        /// </summary>
        public char Fill { get; set; }
        
        /// <summary>
        /// Get/set property for Length.
        /// </summary>
        public int Length
        {
            get
            {
                // if font is bold then letters occupy more space and therefor have more length
                return length / sizeFactor;
            }
            set
            {
                if (length == value)
                    return;
                length = value;
            }
        }

        /// <summary>
        /// Get/set property for Prefix.
        /// </summary>
        public string Prefix { get; set; }
      
        /// <summary>
        /// Get/set property for FontStyle.
        /// </summary>
        public System.Drawing.FontStyle FontStyle { get; set; }

        /// <summary>
        /// Get/set property for FontSize.
        /// </summary>
        public FormFontSize FontSize { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="formItem"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Grandfather")]
        public FormItemInfo(DataRow formItem)
        {
            const int normalTextSizeFactor = 1;
            const int doubleSizeTextSizeFactor = 2;

            try
            {
                this.CharIndex = Convert.ToInt16(formItem["nr"]);
                this.ValueString = formItem["value"].ToString();
                if (formItem["valign"].ToString() == "right")
                    this.VertAlign = valign.right;
                else if (formItem["valign"].ToString() == "left")
                    this.VertAlign = valign.left;
                else if (formItem["valign"].ToString() == "center")
                    this.VertAlign = valign.center;
                this.Fill = Convert.ToChar(string.Concat(formItem["fill"]));
                this.Variable = formItem["variable"].ToString();
                this.IsVariable = formItem["variable"].ToString().Length != 0;
                this.Prefix = formItem["prefix"].ToString();

                //
                // imageId may not exist for report formats that are 
                // upgraded from a previous release.
                //
                if (formItem.Table.Columns.Contains("imageId") &&
                   !DBNull.Value.Equals(formItem["imageId"]))
                {
                    this.ImageId = Convert.ToInt32(formItem["imageId"].ToString());
                }
                else
                {
                    this.ImageId = 0;
                }

                this.FontStyle = (System.Drawing.FontStyle)Convert.ToInt32(formItem["FontStyle"].ToString());

                // For backward compability with old report version
                this.FontSize = formItem.Table.Columns.Contains("FontSize") ? (FormFontSize)Convert.ToInt32(formItem["FontSize"].ToString())
                                       : FormFontSize.Regular;

                this.sizeFactor = normalTextSizeFactor;

                if (SupportedCountryRegion.BR != Functions.CountryRegion && (this.FontStyle == System.Drawing.FontStyle.Bold || this.FontSize == FormFontSize.Large))
                {
                    this.sizeFactor = doubleSizeTextSizeFactor;
                }

                this.length = Convert.ToInt16(formItem["length"]) + (this.Prefix.Length * this.sizeFactor);
            }
            catch (Exception ex)
            {
                NetTracer.Error(ex, "FormItemInfo::FormItemInfo failed");
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public FormItemInfo()
        {
        }
    }
}
