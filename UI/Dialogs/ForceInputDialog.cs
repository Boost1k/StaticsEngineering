// UI/Dialogs/ForceInputDialog.cs
using System;
using System.Windows.Forms;

namespace StaticsEngineeringCAD.UI.Dialogs
{
    public partial class ForceInputDialog : Form
    {
        public float ForceMagnitude { get; private set; }
        public float ForceAngleDegrees { get; private set; }

        public ForceInputDialog(float defaultMagnitude = 100f, float defaultAngle = 270f)
        {
            InitializeComponent();

            txtMagnitude.Text = defaultMagnitude.ToString();
            txtAngle.Text = defaultAngle.ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (float.TryParse(txtMagnitude.Text, out float magnitude) &&
                float.TryParse(txtAngle.Text, out float angle))
            {
                ForceMagnitude = magnitude;
                ForceAngleDegrees = angle;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректные числовые значения.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Windows Form Designer generated code (если нет дизайнера)
        private System.Windows.Forms.Label lblMagnitude;
        private System.Windows.Forms.TextBox txtMagnitude;
        private System.Windows.Forms.Label lblAngle;
        private System.Windows.Forms.TextBox txtAngle;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;

        private void InitializeComponent()
        {
            this.lblMagnitude = new System.Windows.Forms.Label();
            this.txtMagnitude = new System.Windows.Forms.TextBox();
            this.lblAngle = new System.Windows.Forms.Label();
            this.txtAngle = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();

            //
            // lblMagnitude
            //

            this.lblMagnitude.AutoSize = true;
            this.lblMagnitude.Location = new System.Drawing.Point(12, 15);
            this.lblMagnitude.Name = "lblMagnitude";
            this.lblMagnitude.Size = new System.Drawing.Size(105, 13);
            this.lblMagnitude.TabIndex = 0;
            this.lblMagnitude.Text = "Величина силы, Н:";

            //
            // txtMagnitude
            //

            this.txtMagnitude.Location = new System.Drawing.Point(170, 12); // Увеличил отступ
            this.txtMagnitude.Name = "txtMagnitude";
            this.txtMagnitude.Size = new System.Drawing.Size(100, 20);
            this.txtMagnitude.TabIndex = 1;

            //
            // lblAngle
            //

            this.lblAngle.AutoSize = true;
            this.lblAngle.Location = new System.Drawing.Point(12, 41);
            this.lblAngle.Name = "lblAngle";
            this.lblAngle.Size = new System.Drawing.Size(152, 13);
            this.lblAngle.TabIndex = 2;
            this.lblAngle.Text = "Угол, ° (0-вправо, 90-вверх):";

            //
            // txtAngle
            //

            this.txtAngle.Location = new System.Drawing.Point(170, 38);
            this.txtAngle.Name = "txtAngle";
            this.txtAngle.Size = new System.Drawing.Size(100, 20);
            this.txtAngle.TabIndex = 3;

            //
            // btnOK
            //

            this.btnOK.Location = new System.Drawing.Point(114, 75);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);

            //
            // btnCancel
            //

            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(195, 75);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;

            //
            // ForceInputDialog
            //

            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(290, 115); // Немного увеличил
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtAngle);
            this.Controls.Add(this.lblAngle);
            this.Controls.Add(this.txtMagnitude);
            this.Controls.Add(this.lblMagnitude);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ForceInputDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Параметры силы";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion
    }
}