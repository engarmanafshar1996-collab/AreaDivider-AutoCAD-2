using System;
using System.Windows.Forms;
using System.Drawing;

namespace AreaDivider
{
    public partial class AreaDividerForm : Form
    {
        public double NumberOfParts { get; private set; }
        public string CuttingMethod { get; private set; }
        public double FrontDimension { get; private set; }

        public AreaDividerForm()
        {
            InitializeComponent();
            this.CuttingMethod = "EqualArea"; // Default method
            this.Font = new Font("Segoe UI", 9); // Use a modern font
            this.BackColor = SystemColors.Control; // Standard Windows background
        }

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtNumParts = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbRotationCut = new System.Windows.Forms.RadioButton();
            this.rbFrontDimension = new System.Windows.Forms.RadioButton();
            this.rbEqualArea = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.txtFrontDimension = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Number of Equal Parts:";
            // 
            // txtNumParts
            // 
            this.txtNumParts.Location = new System.Drawing.Point(140, 12);
            this.txtNumParts.Name = "txtNumParts";
            this.txtNumParts.Size = new System.Drawing.Size(80, 23);
            this.txtNumParts.TabIndex = 1;
            this.txtNumParts.Text = "2";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(10, 10);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 28);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(120, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 28);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbRotationCut);
            this.groupBox1.Controls.Add(this.txtFrontDimension);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.rbFrontDimension);
            this.groupBox1.Controls.Add(this.rbEqualArea);
            this.groupBox1.Location = new System.Drawing.Point(10, 45);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(230, 150);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cutting Method";
            // 
            // rbRotationCut
            // 
            this.rbRotationCut.AutoSize = true;
            this.rbRotationCut.Location = new System.Drawing.Point(15, 75);
            this.rbRotationCut.Name = "rbRotationCut";
            this.rbRotationCut.Size = new System.Drawing.Size(95, 19);
            this.rbRotationCut.TabIndex = 4;
            this.rbRotationCut.Text = "Rotation Cut";
            this.rbRotationCut.UseVisualStyleBackColor = true;
            this.rbRotationCut.CheckedChanged += new System.EventHandler(this.rbRotationCut_CheckedChanged);
            // 
            // rbFrontDimension
            // 
            this.rbFrontDimension.AutoSize = true;
            this.rbFrontDimension.Location = new System.Drawing.Point(15, 50);
            this.rbFrontDimension.Name = "rbFrontDimension";
            this.rbFrontDimension.Size = new System.Drawing.Size(144, 19);
            this.rbFrontDimension.TabIndex = 1;
            this.rbFrontDimension.Text = "Fixed Front Dimension";
            this.rbFrontDimension.UseVisualStyleBackColor = true;
            this.rbFrontDimension.CheckedChanged += new System.EventHandler(this.rbFrontDimension_CheckedChanged);
            // 
            // rbEqualArea
            // 
            this.rbEqualArea.AutoSize = true;
            this.rbEqualArea.Checked = true;
            this.rbEqualArea.Location = new System.Drawing.Point(15, 25);
            this.rbEqualArea.Name = "rbEqualArea";
            this.rbEqualArea.Size = new System.Drawing.Size(87, 19);
            this.rbEqualArea.TabIndex = 0;
            this.rbEqualArea.TabStop = true;
            this.rbEqualArea.Text = "Equal Area";
            this.rbEqualArea.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 110);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Front Dimension:";
            // 
            // txtFrontDimension
            // 
            this.txtFrontDimension.Enabled = false;
            this.txtFrontDimension.Location = new System.Drawing.Point(140, 107);
            this.txtFrontDimension.Name = "txtFrontDimension";
            this.txtFrontDimension.Size = new System.Drawing.Size(80, 23);
            this.txtFrontDimension.TabIndex = 3;
            this.txtFrontDimension.Text = "10.0";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnOK);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 205);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(250, 48);
            this.panel1.TabIndex = 5;
            // 
            // AreaDividerForm
            // 
            this.ClientSize = new System.Drawing.Size(250, 253);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.txtNumParts);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AreaDividerForm";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Area Divider Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNumParts;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbRotationCut;
        private System.Windows.Forms.RadioButton rbFrontDimension;
        private System.Windows.Forms.RadioButton rbEqualArea;
        private System.Windows.Forms.TextBox txtFrontDimension;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (rbEqualArea.Checked || rbRotationCut.Checked)
            {
                if (double.TryParse(txtNumParts.Text, out double parts) && parts >= 2)
                {
                    this.NumberOfParts = parts;
                    this.CuttingMethod = rbRotationCut.Checked ? "RotationCut" : "EqualArea";
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please enter a valid number of parts (>= 2).", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (rbFrontDimension.Checked)
            {
                if (double.TryParse(txtFrontDimension.Text, out double dim) && dim > 0)
                {
                    this.FrontDimension = dim;
                    this.CuttingMethod = "FrontDimension";
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please enter a valid front dimension (> 0).", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void rbFrontDimension_CheckedChanged(object sender, EventArgs e)
        {
            txtFrontDimension.Enabled = rbFrontDimension.Checked;
            txtNumParts.Enabled = !rbFrontDimension.Checked;
        }
        
        private void rbRotationCut_CheckedChanged(object sender, EventArgs e)
        {
            txtNumParts.Enabled = rbRotationCut.Checked || rbEqualArea.Checked;
            txtFrontDimension.Enabled = rbFrontDimension.Checked;
        }
    }
}
