
namespace SWToR_RUS
{
    partial class Form3
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form3));
            this.label1 = new System.Windows.Forms.Label();
            this.email = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.password = new System.Windows.Forms.TextBox();
            this.auth = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.name = new System.Windows.Forms.TextBox();
            this.Translate_Restrict = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Почта";
            // 
            // email
            // 
            this.email.Location = new System.Drawing.Point(123, 12);
            this.email.Name = "email";
            this.email.Size = new System.Drawing.Size(230, 20);
            this.email.TabIndex = 14;
            this.email.TextChanged += new System.EventHandler(this.Email_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Пароль";
            // 
            // password
            // 
            this.password.Location = new System.Drawing.Point(123, 37);
            this.password.Name = "password";
            this.password.PasswordChar = '*';
            this.password.Size = new System.Drawing.Size(230, 20);
            this.password.TabIndex = 16;
            this.password.TextChanged += new System.EventHandler(this.Password_TextChanged);
            // 
            // auth
            // 
            this.auth.Enabled = false;
            this.auth.Location = new System.Drawing.Point(182, 117);
            this.auth.Name = "auth";
            this.auth.Size = new System.Drawing.Size(171, 47);
            this.auth.TabIndex = 38;
            this.auth.Text = "Авторизация";
            this.auth.UseVisualStyleBackColor = true;
            this.auth.Click += new System.EventHandler(this.Auth_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 13);
            this.label3.TabIndex = 40;
            this.label3.Text = "Автор (Никнейм)";
            // 
            // name
            // 
            this.name.Location = new System.Drawing.Point(123, 63);
            this.name.Name = "name";
            this.name.Size = new System.Drawing.Size(230, 20);
            this.name.TabIndex = 18;
            this.name.TextChanged += new System.EventHandler(this.Name_TextChanged);
            // 
            // Translate_Restrict
            // 
            this.Translate_Restrict.AutoSize = true;
            this.Translate_Restrict.Enabled = false;
            this.Translate_Restrict.Location = new System.Drawing.Point(17, 95);
            this.Translate_Restrict.Name = "Translate_Restrict";
            this.Translate_Restrict.Size = new System.Drawing.Size(250, 17);
            this.Translate_Restrict.TabIndex = 41;
            this.Translate_Restrict.Text = "Запретить редактирование моих переводов";
            this.Translate_Restrict.UseVisualStyleBackColor = true;
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 176);
            this.Controls.Add(this.Translate_Restrict);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.name);
            this.Controls.Add(this.auth);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.password);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.email);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form3";
            this.Text = "Авторизация";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox email;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox password;
        private System.Windows.Forms.Button auth;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox name;
        private System.Windows.Forms.CheckBox Translate_Restrict;
    }
}