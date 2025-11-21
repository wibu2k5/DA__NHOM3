using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DOAN_Nhom3
{
    public partial class Form1 : Form
    {
        // ====== Undo/Redo state ======
        private readonly Stack<string> undoStack = new();
        private readonly Stack<string> redoStack = new();
        private bool isInternalChange = false;   // chặn vòng lặp TextChanged khi set Text bằng code
        private string previousText = string.Empty;
        private string? currentFilePath = null;  // đường dẫn file hiện tại (nếu đã lưu)

        // Placeholder cho rtbEditor
        private Label? lblPlaceholder;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            previousText = rtbEditor.Text;
            UpdateUndoRedoButtons();

            rtbEditor.KeyDown += RtbEditorKeyDown;

            rtbEditor.ReadOnly = false;
            rtbEditor.Enabled = true;
            rtbEditor.BringToFront();
            rtbEditor.Focus();

            // Placeholder

            var placeholder = new Label
            {
                Text = "Nhập tại đây...",
                ForeColor = Color.Gray,
                BackColor = rtbEditor.BackColor,
                AutoSize = true,
                Cursor = Cursors.IBeam
            };
            // gán vào field để các hàm khác (UpdatePlaceholder/PositionPlaceholder) dùng
            lblPlaceholder = placeholder;

            // Lấy container an toàn: nếu Parent đang null thì dùng chính Form
            Control container = rtbEditor.Parent ?? this;

            // Thêm và đưa lên trên
            container.Controls.Add(placeholder);
            PositionPlaceholder();
            placeholder.BringToFront();

            // Sự kiện
            placeholder.Click += (s, e2) => rtbEditor.Focus();
            rtbEditor.LocationChanged += (s, e2) => PositionPlaceholder();
            rtbEditor.SizeChanged += (s, e2) => PositionPlaceholder();
            this.Resize += (s, e2) => PositionPlaceholder();
        }

        private void RtbEditorTextChanged(object? sender, EventArgs e)
        {
            if (isInternalChange)
            {
                UpdatePlaceholder();
                return;
            }

            undoStack.Push(previousText);
            redoStack.Clear();
            previousText = rtbEditor.Text;

            UpdateUndoRedoButtons();
            UpdatePlaceholder();
        }

        private void BtnUndoClick(object? sender, EventArgs e) => Undo();
        private void BtnRedoClick(object? sender, EventArgs e) => Redo();
        private void BtnSaveClick(object? sender, EventArgs e) => SaveToFile();

        private void RtbEditorKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z) { Undo(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.Y) { Redo(); e.SuppressKeyPress = true; }
            else if (e.Control && e.KeyCode == Keys.S) { SaveToFile(); e.SuppressKeyPress = true; }
        }

        private void Undo()
        {
            if (undoStack.Count == 0) return;

            isInternalChange = true;
            redoStack.Push(rtbEditor.Text);
            rtbEditor.Text = undoStack.Pop();
            isInternalChange = false;

            previousText = rtbEditor.Text;
            rtbEditor.SelectionStart = rtbEditor.TextLength;
            rtbEditor.SelectionLength = 0;

            UpdateUndoRedoButtons();
            UpdatePlaceholder();
        }

        private void Redo()
        {
            if (redoStack.Count == 0) return;

            isInternalChange = true;
            undoStack.Push(rtbEditor.Text);
            rtbEditor.Text = redoStack.Pop();
            isInternalChange = false;

            previousText = rtbEditor.Text;
            rtbEditor.SelectionStart = rtbEditor.TextLength;
            rtbEditor.SelectionLength = 0;

            UpdateUndoRedoButtons();
            UpdatePlaceholder();
        }

        private void SaveToFile()
        {
            try
            {
                if (!string.IsNullOrEmpty(currentFilePath))
                {
                    File.WriteAllText(currentFilePath!, rtbEditor.Text, Encoding.UTF8);
                    MessageBox.Show("Đã lưu: " + currentFilePath, "Lưu file", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    using var sfd = new SaveFileDialog
                    {
                        Title = "Lưu văn bản",
                        Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                        DefaultExt = "txt",
                        AddExtension = true,
                        FileName = "document.txt"
                    };
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(sfd.FileName, rtbEditor.Text, Encoding.UTF8);
                        currentFilePath = sfd.FileName;
                        this.Text = "Trình soạn thảo - " + Path.GetFileName(currentFilePath);
                        MessageBox.Show("Đã lưu: " + currentFilePath, "Lưu file", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu file:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateUndoRedoButtons()
        {
            btnUndo.Enabled = undoStack.Count > 0;
            btnRedo.Enabled = redoStack.Count > 0;
        }

        private void UpdatePlaceholder()
        {
            if (lblPlaceholder == null) return;
            // Hiện placeholder khi Text trống (kể cả đang focus)
            lblPlaceholder.Visible = string.IsNullOrEmpty(rtbEditor.Text);
        }

        private void PositionPlaceholder()
        {
            if (lblPlaceholder == null) return;
            lblPlaceholder.Location = new Point(rtbEditor.Left + 6, rtbEditor.Top + 6);
        }
    }
}