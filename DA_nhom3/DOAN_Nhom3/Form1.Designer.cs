namespace DOAN_Nhom3
{
    partial class Form1
    {

        private System.ComponentModel.IContainer components = null;

   
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code


        private void InitializeComponent()
        {
            rtbEditor = new RichTextBox();
            btnUndo = new Button();
            btnRedo = new Button();
            btnSave = new Button();
            panel1 = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // rtbEditor
            // 
            rtbEditor.BackColor = SystemColors.ControlLightLight;
            rtbEditor.Dock = DockStyle.Fill;
            rtbEditor.ForeColor = SystemColors.InactiveCaptionText;
            rtbEditor.Location = new Point(0, 0);
            rtbEditor.Name = "rtbEditor";
            rtbEditor.Size = new Size(1464, 842);
            rtbEditor.TabIndex = 0;
            rtbEditor.Text = "";
            rtbEditor.TextChanged += RtbEditorTextChanged; // khớp với Form1.cs
                                                           // 
                                                           // btnUndo
                                                           // 
            btnUndo.Location = new Point(101, 87);
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new Size(385, 119);
            btnUndo.TabIndex = 1;
            btnUndo.Text = "Undo (Ctrl+Z)";
            btnUndo.UseVisualStyleBackColor = true;
            btnUndo.Click += BtnUndoClick; // đổi tên cho khớp
                                           // 
                                           // btnRedo
                                           // 
            btnRedo.Location = new Point(566, 87);
            btnRedo.Name = "btnRedo";
            btnRedo.Size = new Size(385, 119);
            btnRedo.TabIndex = 2;
            btnRedo.Text = "Redo (Ctrl+Y)";
            btnRedo.UseVisualStyleBackColor = true;
            btnRedo.Click += BtnRedoClick; // đổi tên cho khớp
                                           // 
                                           // btnSave
                                           // 
            btnSave.Location = new Point(1025, 87);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(385, 119);
            btnSave.TabIndex = 3;
            btnSave.Text = "Save (Ctrl+S)";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += BtnSaveClick; // đổi tên cho khớp
                                           // 
                                           // panel1
                                           // 
            panel1.Controls.Add(btnUndo);
            panel1.Controls.Add(btnRedo);
            panel1.Controls.Add(btnSave);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1464, 260);
            panel1.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1464, 842);
            Controls.Add(rtbEditor);  // RichTextBox trước
            Controls.Add(panel1);     // Panel sau
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;       // khớp với Form1.cs
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }
        #endregion

        private RichTextBox rtbEditor;
        private Button btnUndo;
        private Button btnRedo;
        private Button btnSave;
        private Panel panel1;
    }
}