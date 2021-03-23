
namespace SWToR_RUS
{
    partial class Form4
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form4));
            this.data_bans = new System.Windows.Forms.DataGridView();
            this.ok = new System.Windows.Forms.Button();
            this.translator = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.count = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.checker = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.data_bans)).BeginInit();
            this.SuspendLayout();
            // 
            // data_bans
            // 
            this.data_bans.AllowUserToAddRows = false;
            this.data_bans.AllowUserToDeleteRows = false;
            this.data_bans.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.data_bans.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.data_bans.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.data_bans.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.data_bans.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.translator,
            this.count,
            this.checker});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.data_bans.DefaultCellStyle = dataGridViewCellStyle1;
            this.data_bans.Location = new System.Drawing.Point(12, 10);
            this.data_bans.Name = "data_bans";
            this.data_bans.Size = new System.Drawing.Size(379, 344);
            this.data_bans.TabIndex = 1;
            // 
            // ok
            // 
            this.ok.Location = new System.Drawing.Point(12, 360);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(100, 45);
            this.ok.TabIndex = 2;
            this.ok.Text = "Применить";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // translator
            // 
            this.translator.HeaderText = "Переводчик";
            this.translator.Name = "translator";
            this.translator.ReadOnly = true;
            this.translator.Width = 93;
            // 
            // count
            // 
            this.count.HeaderText = "Количество строк";
            this.count.Name = "count";
            this.count.Width = 113;
            // 
            // checker
            // 
            this.checker.FalseValue = "F";
            this.checker.HeaderText = "Заблокировать";
            this.checker.Name = "checker";
            this.checker.TrueValue = "T";
            this.checker.Width = 91;
            // 
            // Form4
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(403, 417);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.data_bans);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form4";
            this.Text = "Черный Список Авторов";
            this.Load += new System.EventHandler(this.Form4_Load);
            ((System.ComponentModel.ISupportInitialize)(this.data_bans)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView data_bans;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.DataGridViewTextBoxColumn translator;
        private System.Windows.Forms.DataGridViewTextBoxColumn count;
        private System.Windows.Forms.DataGridViewCheckBoxColumn checker;
    }
}