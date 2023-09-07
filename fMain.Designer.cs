namespace WinReporter
{
    partial class fMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ctlTreeOriginal = new TreeView();
            ctlTreeText = new TextBox();
            ctlTreeCopy = new TreeView();
            btDemo = new Button();
            SuspendLayout();
            // 
            // ctlTreeOriginal
            // 
            ctlTreeOriginal.Location = new Point(12, 12);
            ctlTreeOriginal.Name = "ctlTreeOriginal";
            ctlTreeOriginal.Size = new Size(407, 519);
            ctlTreeOriginal.TabIndex = 0;
            // 
            // ctlTreeText
            // 
            ctlTreeText.Location = new Point(425, 12);
            ctlTreeText.Multiline = true;
            ctlTreeText.Name = "ctlTreeText";
            ctlTreeText.ScrollBars = ScrollBars.Both;
            ctlTreeText.Size = new Size(375, 519);
            ctlTreeText.TabIndex = 1;
            // 
            // ctlTreeCopy
            // 
            ctlTreeCopy.Location = new Point(806, 12);
            ctlTreeCopy.Name = "ctlTreeCopy";
            ctlTreeCopy.Size = new Size(407, 519);
            ctlTreeCopy.TabIndex = 2;
            // 
            // btDemo
            // 
            btDemo.Location = new Point(12, 537);
            btDemo.Name = "btDemo";
            btDemo.Size = new Size(75, 23);
            btDemo.TabIndex = 3;
            btDemo.Text = "Demo";
            btDemo.UseVisualStyleBackColor = true;
            btDemo.Click += btDemo_Click;
            // 
            // fMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1225, 576);
            Controls.Add(btDemo);
            Controls.Add(ctlTreeCopy);
            Controls.Add(ctlTreeText);
            Controls.Add(ctlTreeOriginal);
            Name = "fMain";
            Text = "Form1";
            Load += fMain_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TreeView ctlTreeOriginal;
        private TextBox ctlTreeText;
        private TreeView ctlTreeCopy;
        private Button btDemo;
    }
}