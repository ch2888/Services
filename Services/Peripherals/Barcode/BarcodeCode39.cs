/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/

namespace Microsoft.Dynamics.Retail.Pos.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Dynamics.Retail.Diagnostics;
    using System.Drawing;

    /// <summary>
    /// Encapsulates the barcode Code 39 string encoding
    /// </summary>
    public sealed class BarcodeCode39 : Barcode
    {

        #region Fields

        private const char START_END_MARKER = '*';
        private const string FONT_NAME = "BC C39 2 to 1 Narrow";
        private const int FONT_SIZE = 30;

        private readonly List<int> supportedCharacters = new List<int>(100);

        /// <summary>
        /// Gets the font name.
        /// </summary>
        public override string FontName { get { return FONT_NAME; } }

        /// <summary>
        /// Gets the font size.
        /// </summary>
        public override int FontSize { get { return FONT_SIZE; } }

        #endregion

        #region Methods

        /// <summary>
        ///  Creates a new instance of the <see cref="BarcodeCode39"/> class.
        /// </summary>
        public BarcodeCode39()
        {
            supportedCharacters.AddRange(Enumerable.Range(32, 60).Where(n => (
                    n == 32 // Space
                    || (n >= 36 && n <= 38) //$%&
                    || (n >= 43 && n <= 57) // +,./ 0 - 9
                    || (n >= 65 && n <= 90) // A - Z
                    )));
        }

        /// <summary>
        /// Encodes the text to for Code39
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Encoded string.</returns>
        public override string Encode(string text)
        {
            NetTracer.Information("Peripheral [BarcodeCode39] - Encode");

            if (text == null)
                throw new ArgumentNullException("text");

            StringBuilder result = new StringBuilder(text.Length + 2);

            // Add start character
            result.Append(START_END_MARKER);

            foreach (char ch in text)
            {
                if (supportedCharacters.BinarySearch(ch) >= 0)
                {
                    result.Append(ch);
                }
                else
                {
                    NetTracer.Warning("Peripheral [BarcodeCode39] - Unsupported character '{0}' elided.", ch);
                }
            }

            // Replace Space with comma
            result.Replace(' ', ',');

            // Add stop character
            result.Append(START_END_MARKER);

            return result.ToString();
        }

        #endregion

    }
}
