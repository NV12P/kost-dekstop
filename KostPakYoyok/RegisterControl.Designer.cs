namespace KostPakYoyok
{
    partial class RegisterControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnRegister = new Guna.UI2.WinForms.Guna2Button();
            this.linkLogin = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textPassword = new Guna.UI2.WinForms.Guna2TextBox();
            this.textUsername = new Guna.UI2.WinForms.Guna2TextBox();
            this.textPhone = new Guna.UI2.WinForms.Guna2TextBox();
            this.textConfirmPassword = new Guna.UI2.WinForms.Guna2TextBox();
            this.SuspendLayout();
            // 
            // btnRegister
            // 
            this.btnRegister.BorderRadius = 22;
            this.btnRegister.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btnRegister.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btnRegister.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btnRegister.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btnRegister.FillColor = System.Drawing.Color.White;
            this.btnRegister.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnRegister.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(18)))), ((int)(((byte)(101)))));
            this.btnRegister.Location = new System.Drawing.Point(77, 675);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.PressedColor = System.Drawing.SystemColors.ControlDark;
            this.btnRegister.Size = new System.Drawing.Size(659, 55);
            this.btnRegister.TabIndex = 23;
            this.btnRegister.Text = "Register";
            // 
            // linkLogin
            // 
            this.linkLogin.ActiveLinkColor = System.Drawing.SystemColors.ControlDark;
            this.linkLogin.AutoSize = true;
            this.linkLogin.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLogin.LinkColor = System.Drawing.SystemColors.ControlLightLight;
            this.linkLogin.Location = new System.Drawing.Point(262, 517);
            this.linkLogin.Name = "linkLogin";
            this.linkLogin.Size = new System.Drawing.Size(61, 28);
            this.linkLogin.TabIndex = 22;
            this.linkLogin.TabStop = true;
            this.linkLogin.Text = "Login";
            this.linkLogin.VisitedLinkColor = System.Drawing.SystemColors.ControlDark;
            this.linkLogin.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLogin_LinkClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.label2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label2.Location = new System.Drawing.Point(91, 517);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(165, 28);
            this.label2.TabIndex = 21;
            this.label2.Text = "Have an account?";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 30F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label1.Location = new System.Drawing.Point(81, 93);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(221, 67);
            this.label1.TabIndex = 20;
            this.label1.Text = "Register";
            // 
            // textPassword
            // 
            this.textPassword.BorderColor = System.Drawing.Color.White;
            this.textPassword.BorderRadius = 22;
            this.textPassword.BorderThickness = 2;
            this.textPassword.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textPassword.DefaultText = "";
            this.textPassword.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.textPassword.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.textPassword.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.textPassword.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.textPassword.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(18)))), ((int)(((byte)(101)))));
            this.textPassword.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.textPassword.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.textPassword.ForeColor = System.Drawing.Color.White;
            this.textPassword.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.textPassword.Location = new System.Drawing.Point(77, 359);
            this.textPassword.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.textPassword.Name = "textPassword";
            this.textPassword.PasswordChar = '●';
            this.textPassword.PlaceholderText = "Password";
            this.textPassword.SelectedText = "";
            this.textPassword.Size = new System.Drawing.Size(659, 59);
            this.textPassword.TabIndex = 19;
            this.textPassword.TextOffset = new System.Drawing.Point(14, -2);
            // 
            // textUsername
            // 
            this.textUsername.BorderColor = System.Drawing.Color.White;
            this.textUsername.BorderRadius = 22;
            this.textUsername.BorderThickness = 2;
            this.textUsername.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textUsername.DefaultText = "";
            this.textUsername.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.textUsername.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.textUsername.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.textUsername.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.textUsername.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(18)))), ((int)(((byte)(101)))));
            this.textUsername.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.textUsername.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.textUsername.ForeColor = System.Drawing.Color.White;
            this.textUsername.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.textUsername.Location = new System.Drawing.Point(77, 199);
            this.textUsername.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.textUsername.Name = "textUsername";
            this.textUsername.PlaceholderText = "Username";
            this.textUsername.SelectedText = "";
            this.textUsername.Size = new System.Drawing.Size(659, 59);
            this.textUsername.TabIndex = 18;
            this.textUsername.TextOffset = new System.Drawing.Point(14, -2);
            // 
            // textPhone
            // 
            this.textPhone.BorderColor = System.Drawing.Color.White;
            this.textPhone.BorderRadius = 22;
            this.textPhone.BorderThickness = 2;
            this.textPhone.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textPhone.DefaultText = "";
            this.textPhone.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.textPhone.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.textPhone.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.textPhone.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.textPhone.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(18)))), ((int)(((byte)(101)))));
            this.textPhone.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.textPhone.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.textPhone.ForeColor = System.Drawing.Color.White;
            this.textPhone.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.textPhone.Location = new System.Drawing.Point(77, 279);
            this.textPhone.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.textPhone.Name = "textPhone";
            this.textPhone.PlaceholderText = "Phone Number";
            this.textPhone.SelectedText = "";
            this.textPhone.Size = new System.Drawing.Size(659, 59);
            this.textPhone.TabIndex = 24;
            this.textPhone.TextOffset = new System.Drawing.Point(14, -2);
            // 
            // textConfirmPassword
            // 
            this.textConfirmPassword.BorderColor = System.Drawing.Color.White;
            this.textConfirmPassword.BorderRadius = 22;
            this.textConfirmPassword.BorderThickness = 2;
            this.textConfirmPassword.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textConfirmPassword.DefaultText = "";
            this.textConfirmPassword.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.textConfirmPassword.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.textConfirmPassword.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.textConfirmPassword.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.textConfirmPassword.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(18)))), ((int)(((byte)(101)))));
            this.textConfirmPassword.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.textConfirmPassword.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.textConfirmPassword.ForeColor = System.Drawing.Color.White;
            this.textConfirmPassword.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.textConfirmPassword.Location = new System.Drawing.Point(77, 439);
            this.textConfirmPassword.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.textConfirmPassword.Name = "textConfirmPassword";
            this.textConfirmPassword.PasswordChar = '●';
            this.textConfirmPassword.PlaceholderText = "Confirm Password";
            this.textConfirmPassword.SelectedText = "";
            this.textConfirmPassword.Size = new System.Drawing.Size(659, 59);
            this.textConfirmPassword.TabIndex = 25;
            this.textConfirmPassword.TextOffset = new System.Drawing.Point(14, -2);
            // 
            // RegisterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(18)))), ((int)(((byte)(101)))));
            this.Controls.Add(this.textConfirmPassword);
            this.Controls.Add(this.textPhone);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.linkLogin);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textPassword);
            this.Controls.Add(this.textUsername);
            this.Name = "RegisterControl";
            this.Size = new System.Drawing.Size(812, 909);
            this.Load += new System.EventHandler(this.RegisterControl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Guna.UI2.WinForms.Guna2Button btnRegister;
        private System.Windows.Forms.LinkLabel linkLogin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private Guna.UI2.WinForms.Guna2TextBox textPassword;
        private Guna.UI2.WinForms.Guna2TextBox textUsername;
        private Guna.UI2.WinForms.Guna2TextBox textPhone;
        private Guna.UI2.WinForms.Guna2TextBox textConfirmPassword;
    }
}
