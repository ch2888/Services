using LSRetailPosis.POSControls.Touch;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.Dynamics.Retail.Pos.BlankOperations.Dialog
{
    public partial class frmAmountCode : frmTouchBase
    {
        public int Code
        {
         get { return int.Parse(this.numPad1.EnteredValue==""?"0":this.numPad1.EnteredValue); }
        }
        public frmAmountCode()
        {
            InitializeComponent();

        }

        private void lblHeading_Click(object sender, EventArgs e)
        {

        }
    }
}
