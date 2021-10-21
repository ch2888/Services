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
    using System.Drawing;
    using Microsoft.Dynamics.Retail.Diagnostics;

    /// <summary>
    /// An abstract base class the provides functionality for various barcode encoder classes.
    /// </summary>
    public abstract class Barcode
    {
        protected const float DEFAULT_DPI = 96f;
        protected const string TEXT_FONT_NAME = "Courier New";
        protected const int TEXT_FONT_SIZE =  10;

        /// <summary>
        /// Gets the font name.
        /// </summary>
        public abstract string FontName { get; }

        /// <summary>
        /// Gets the font size.
        /// </summary>
        public abstract int FontSize { get; }

        /// <summary>
        /// Encode the text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>Encoded string.</returns>
        public abstract string Encode(string text);

        /// <summary>
        /// Creates a barcode image.
        /// </summary>
        /// <param name="text">The text to be used to create the barcode image.</param>        
        /// <returns>The barcode image. Null if barcode is not created.</returns>
        /// <remarks>Creates the barcode image with a default DPI.</remarks>
        public virtual Image Create(string text)
        {
            return Create(text, DEFAULT_DPI, DEFAULT_DPI);
        }

        /// <summary>
        /// Creates a barcode image.
        /// </summary>
        /// <param name="text">The text to be used to create the barcode image.</param>        
        /// <param name="xDpi">Horizontal resolution.</param>
        /// <param name="yDpi">Vertical resolution.</param>
        /// <returns>Barcode image. Null if barcode is not created.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing returned object")]
        public virtual Image Create(string text, float xDpi, float yDpi)
        {
            NetTracer.Information("Peripheral [{0}] - Create", this);

            Bitmap barcodeImage = null;

            using (Font barcodeFont = new Font(FontName, FontSize))
            {
                if (barcodeFont.Name.Equals(barcodeFont.OriginalFontName, StringComparison.Ordinal)) // If font installed.
                {
                    using (Font barcodeTextFont = new Font(TEXT_FONT_NAME, TEXT_FONT_SIZE)) // Text font
                    {
                        try
                        {
                            text = Encode(text);

                            SizeF barcodeSizeF = GetTextSizeF(text, barcodeFont, xDpi, yDpi);
                            float barcodeTextHeight = barcodeTextFont.GetHeight(yDpi);

                            barcodeImage = new Bitmap((int)barcodeSizeF.Width, (int)(barcodeSizeF.Height + barcodeTextHeight));
                            barcodeImage.SetResolution(xDpi, yDpi);

                            using (Graphics graphic = Graphics.FromImage(barcodeImage))
                            {
                                // Calculate left/right margin for drawing barcode considering dpi being used.
                                float XYWithMargin = (xDpi / DEFAULT_DPI) * 5;

                                // Draw barcode
                                graphic.DrawString(text, barcodeFont, Brushes.Black, XYWithMargin, XYWithMargin);

                                // Draw text below barcode in center
                                RectangleF textRect = new RectangleF(0, barcodeSizeF.Height, barcodeSizeF.Width, barcodeTextHeight);
                                using(StringFormat textFormat = new StringFormat(StringFormatFlags.NoClip) { Alignment = StringAlignment.Center })
                                {
                                    graphic.DrawString(text, barcodeTextFont, Brushes.Black, textRect, textFormat);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (barcodeImage != null)
                            {
                                barcodeImage.Dispose();
                            }

                            NetTracer.Error(ex, "Peripheral [{0}] - Exception during barcode creation.", this);
                        }
                    }
                }
                else
                {
                    NetTracer.Error("Peripheral [{0}] - Barcode creation failed. Font {1} in not installed.", this, FontName);
                }
            }

            return barcodeImage;
        }

        /// <summary>
        /// Gets the sizeF of a text with given font.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="font">The font.</param>
        /// <param name="xDpi">X dpi for target.</param>
        /// <param name="yDpi">Y dpi for target.</param>
        /// <returns>Size float.</returns>
        private static SizeF GetTextSizeF(string text, Font font, float xDpi, float yDpi)
        {
            SizeF sizeF;

            // Create temporary graphics and calculate the height/width
            using (Bitmap bitmap = new Bitmap(1, 1))
            {
                bitmap.SetResolution(xDpi, yDpi);
                using (Graphics graphic = Graphics.FromImage(bitmap))
                {
                    sizeF = graphic.MeasureString(text, font);
                }
            }

            return sizeF;
        }
    }
}
