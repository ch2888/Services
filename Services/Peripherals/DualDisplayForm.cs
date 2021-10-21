/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using LSRetailPosis;
using LSRetailPosis.DataAccess;
using LSRetailPosis.DataAccess.DataUtil;
using LSRetailPosis.DevUtilities;
using LSRetailPosis.POSProcesses;
using LSRetailPosis.Settings;
using LSRetailPosis.Transaction;
using Microsoft.Dynamics.Retail.Pos.Contracts.Services;

namespace Microsoft.Dynamics.Retail.Pos.Services
{
	internal delegate void ShowImageCallback();

	public partial class DualDisplayForm : frmTouchBase
	{

		#region Fields

		private FileSystemInfo[] fsi;
		private int imageCounter;
		private int rotationCounter;

		private int receiptWidthPercentage = 30;
		private System.Timers.Timer imageRotatorTimer = new System.Timers.Timer();

		#endregion

		#region Ctor

		/// <summary>
		/// Ctor
		/// </summary>
		public DualDisplayForm()
		{
			InitializeComponent();

			receipt.ButtonsVisible = false;
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// OnLoad
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLoad(EventArgs e)
		{
			if (!DesignMode)
			{
				// Remove first to ensure that we don’t create multiple subscriptions
				imageRotatorTimer.Elapsed -= new System.Timers.ElapsedEventHandler(imageRotatorTimer_Elapsed);
				imageRotatorTimer.Elapsed += new System.Timers.ElapsedEventHandler(imageRotatorTimer_Elapsed);

                this.receipt.InitCustomFields();
			}

			base.OnLoad(e);
		}

		/// <summary>
		/// Imgate rotator timer handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void imageRotatorTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			// Call ShowImage method on UI thread.
			if (this.InvokeRequired)
			{
				ShowImageCallback callback = new ShowImageCallback(ShowImage);
				this.Invoke(callback);
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Show form on the second display
		/// </summary>
		/// <exception cref="PosStartupException">Thrown, if second display is not found.</exception>
		internal void ShowFormOnDualDisplay()
		{
			Screen secondDisplay = null;

			foreach (Screen screen in Screen.AllScreens)
			{
				if (screen.Primary == false)
				{
					secondDisplay = screen;
					break;
				}
			}

			if (secondDisplay == null)
			{
				throw new PosStartupException("The only screen found is the primary display", null);
			}

			this.Show();
			this.Left = secondDisplay.WorkingArea.X;
			this.Top = secondDisplay.WorkingArea.Y;
			this.Width = secondDisplay.WorkingArea.Width;
			this.Height = secondDisplay.WorkingArea.Height;

			DisplayInitalState();
			LoadSettings();
            LoadTotalAmountDesign();
		}

		protected override bool ShowWithoutActivation
		{
			get { return true; }
		}

		/// <summary>
		/// Displays the initial state of the form before any settings regarding the layout of the form have been received from the POS.
		/// The initial state is a single line of text on an empty canvas.
		/// </summary>
		private void DisplayInitalState()
		{
			panelReceipt.Hide();

			// Advertisement panel
			panelAdvertisements.Hide();
			webBrowser1.Visible = false;
			picImageRotator.Visible = false;

			// Initial state panel
			panelInitialState.Show();
			panelInitialState.Location = new Point(0, 0);
			panelInitialState.Width = this.Width;
			panelInitialState.Height = this.Height;
			lblInitialText.Location = new Point((this.Width / 2) - (lblInitialText.Width / 2), this.Height / 2);

			this.Refresh();
		}

		/// <summary>
		/// Hide receipt control and expand advertisement panel to full screen
		/// </summary>
		private void HideReceiptControl()
		{
			panelReceipt.Hide();
			panelInitialState.Hide();
			panelAdvertisements.Show();
			panelAdvertisements.Location = new Point(0, 0);
			panelAdvertisements.Height = this.Height;
			panelAdvertisements.Width = this.Width;

			lblAdSettingsMissing.Location = new Point((panelAdvertisements.Width / 2) - (lblAdSettingsMissing.Width / 2), panelAdvertisements.Height / 2);

			this.Refresh();
		}

		/// <summary>
		/// Show receipt control
		/// </summary>
		private void ShowReceiptControl()
		{
			panelReceipt.Show();
			panelInitialState.Hide();
			panelAdvertisements.Show();

			double widthPercentage = (double)receiptWidthPercentage / (double)100;
			panelReceipt.Width = Convert.ToInt32(this.Width * widthPercentage);

			panelAdvertisements.Location = new Point(panelReceipt.Width, 0);
			panelAdvertisements.Width = this.Width - panelReceipt.Width;
			panelAdvertisements.Height = this.Height;

			lblAdSettingsMissing.Location = new Point((panelAdvertisements.Width / 2) - (lblAdSettingsMissing.Width / 2), panelAdvertisements.Height / 2);

			this.Refresh();
		}


		/// <summary>
		/// Show the next image from image path.
		/// </summary>
		private void ShowImage()
		{
			try
			{
				imageRotatorTimer.Enabled = false;
				Image oldImage = this.picImageRotator.Image;

				this.picImageRotator.Image = System.Drawing.Image.FromFile(GetNextImageName());
				this.picImageRotator.Refresh();

				if (oldImage != null)
					oldImage.Dispose();
			}
			catch (Exception x)
			{
				ApplicationLog.Log(this.Name, x.Message, LogTraceLevel.Error);
			}
			finally
			{
				imageRotatorTimer.Enabled = true;
			}
		}

		/// <summary>
		/// Display the transaction on Receipt control
		/// </summary>
		/// <param name="posTransaction"></param>
		public void ShowTransaction(PosTransaction posTransaction)
		{
			if (posTransaction == null)
			{
				// There is no transaction taking place.  Time to display advertisements in full screen mode
				HideReceiptControl();
				// Update settings after every transaction. (Head office may have pushed new settings)
				LoadSettings();
			}
			else
			{
				// Threre is a transaction taking place.  Now we display the receipt component and resize the advertisement panel.
				ShowReceiptControl();
				receipt.ShowTransaction(posTransaction);
                totalAmounts.ShowTransaction(posTransaction);
			}
		}

		/// <summary>
		/// Load setting 
		/// </summary>
		private void LoadSettings()
		{
			switch (LSRetailPosis.Settings.HardwareProfiles.DualDisplay.DisplayType)
			{
				case DualDisplayTypes.Logo:

					picImageLogo.Visible = true;
					picImageRotator.Visible = false;
					webBrowser1.Visible = false;
					lblAdSettingsMissing.Visible = false;

					break;

				case DualDisplayTypes.ImageRotator:

					picImageRotator.Visible = true;
					picImageLogo.Visible = false;
					webBrowser1.Visible = false;
					lblAdSettingsMissing.Visible = false;

					imageRotatorTimer.Enabled = false;
					if (LSRetailPosis.Settings.HardwareProfiles.DualDisplay.ImageRotatorInterval > 0)
					{
						imageRotatorTimer.Interval = LSRetailPosis.Settings.HardwareProfiles.DualDisplay.ImageRotatorInterval * 1000;
					}
					imageRotatorTimer.Enabled = true;

					break;

				case DualDisplayTypes.WebBrowser:

					picImageRotator.Visible = false;
					picImageLogo.Visible = false;
					webBrowser1.Visible = true;
					lblAdSettingsMissing.Visible = false;
					imageRotatorTimer.Enabled = false;

					if (LSRetailPosis.Settings.HardwareProfiles.DualDisplay.WebBrowserUrl.StartsWith("http"))
						webBrowser1.Url = new System.Uri(LSRetailPosis.Settings.HardwareProfiles.DualDisplay.WebBrowserUrl);
					else
						webBrowser1.Url = new System.Uri(@"http://" + LSRetailPosis.Settings.HardwareProfiles.DualDisplay.WebBrowserUrl);

					break;

				default:

					picImageRotator.Visible = false;
					picImageLogo.Visible = false;
					webBrowser1.Visible = false;
					lblAdSettingsMissing.Visible = true;

					break;
			}

			if (LSRetailPosis.Settings.HardwareProfiles.DualDisplay.ReceiptWidthPercentage != 0)
				receiptWidthPercentage = LSRetailPosis.Settings.HardwareProfiles.DualDisplay.ReceiptWidthPercentage;
		}

		#endregion

		#region Private Methods

		private void GetAllImages()
		{
			try
			{
				DirectoryInfo dir = new DirectoryInfo(LSRetailPosis.Settings.HardwareProfiles.DualDisplay.ImageRotatorPath);
				fsi = dir.GetFiles();
				imageCounter = fsi.Length;
			}
			catch (Exception)
			{
				imageCounter = 0;
			}
		}

		/// <summary>
		/// Returns next image name as string.
		/// </summary>
		/// <returns></returns>
		private string GetNextImageName()
		{
			string retVal = string.Empty;

			if (rotationCounter < imageCounter)
			{
				retVal = fsi[rotationCounter].FullName;
				rotationCounter++;
			}
			else
			{
				rotationCounter = 0;
				GetAllImages(); // At the end of each rotation we refresh the files since new might have been added.
				retVal = fsi[rotationCounter].FullName;
			}

			return retVal;
		}

        /// <summary>
        /// Load total amount design which could be set in AX
        /// </summary>
        private void LoadTotalAmountDesign()
        {
            LayoutData layoutData = new LayoutData(ApplicationSettings.Database.LocalConnection, ApplicationSettings.Database.DATAAREAID);
            string layout = layoutData.GetTotalsLayout(ApplicationSettings.Terminal.StorePrimaryId, ApplicationSettings.Terminal.TerminalPrimaryId, ApplicationSettings.Terminal.TerminalOperator.OperatorId);

            if (!string.IsNullOrWhiteSpace(layout))
            {
                try
                {
                    totalAmounts.SetLayout(Utility.GetStream(layout));
                    totalAmounts.HideHeader();
                    totalAmounts.Translate();
                }
                catch (Exception x)
                {
                    ApplicationExceptionHandler.HandleException(x.Message, x);
                }
            }
        }

		#endregion

	}
}
