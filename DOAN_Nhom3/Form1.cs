using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;            
using System.Runtime.InteropServices; // 

namespace DOAN_Nhom3
{
    public partial class Form1 : Form
    {
        // Lịch sử Undo/Redo
        private readonly Stack<string> undoStack = new();
        private readonly Stack<string> redoStack = new();
        private bool isInternalChange = false;     // chặn TextChanged khi set Text bằng code
        private string previousText = string.Empty;
        private string? currentFilePath = null;    // đường dẫn file đã lưu

        // Placeholder “Nhập tại đây…”
        private Label? lblPlaceholder;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            // Trạng thái ban đầu
            previousText = rtbEditor.Text;
            UpdateUndoRedoButtons();

            // Chặn Undo/Redo/Save mặc định của RichTextBox, để menu xử lý phím tắt
            rtbEditor.KeyDown += RtbEditorKeyDown;

            // Đảm bảo gõ được
            rtbEditor.ReadOnly = false;
            rtbEditor.Enabled = true;

            // Tạo placeholder
            var placeholder = new Label
            {
                Text = "Nhập tại đây...",
                ForeColor = Color.Gray,
                BackColor = rtbEditor.BackColor,
                AutoSize = true,
                Cursor = Cursors.IBeam
            };
            lblPlaceholder = placeholder;

            // Đặt label chồng lên rtbEditor
            Control container = rtbEditor.Parent ?? this;
            container.Controls.Add(placeholder);
            PositionPlaceholder();
            placeholder.BringToFront();
            placeholder.Click += (s, e2) => rtbEditor.Focus();
            rtbEditor.LocationChanged += (s, e2) => PositionPlaceholder();
            rtbEditor.SizeChanged += (s, e2) => PositionPlaceholder();
            this.Resize += (s, e2) => PositionPlaceholder();

            // Cập nhật placeholder theo trạng thái
            rtbEditor.GotFocus += (s, e2) => UpdatePlaceholder();
            rtbEditor.LostFocus += (s, e2) => UpdatePlaceholder();
            UpdatePlaceholder();

            // Ẩn thanh nút vì đã có MenuStrip (nếu muốn gỡ hẳn, xem RemoveButtonsBar ở dưới)
            RemoveButtonsBar();

            this.Text = "Trình soạn thảo cơ bản - Undo/Redo (Stack)";
            // Đăng ký file association .nh3 (per-user)
            EnsureFileAssociation();

            // Nếu app được mở bằng cách double-click file .nh3, nạp nội dung
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                string candidate = args[1];
                if (File.Exists(candidate))
                    LoadFromFile(candidate);
            }
        }
        private void RemoveButtonsBar()
        {
            if (btnUndo != null && !btnUndo.IsDisposed) panel1.Controls.Remove(btnUndo);
            if (btnRedo != null && !btnRedo.IsDisposed) panel1.Controls.Remove(btnRedo);
            if (btnSave != null && !btnSave.IsDisposed) panel1.Controls.Remove(btnSave);

            if (panel1 != null && !panel1.IsDisposed)
            {
                this.Controls.Remove(panel1);
                panel1.Dispose();
                // Không bắt buộc: panel1 = null!;
            }
        }

        // Text thay đổi do người dùng
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

        // Chỉ chặn phím để không chạy Undo/Redo mặc định của RichTextBox.
        // Hành động sẽ do Menu (ShortcutKeys) gọi qua Click.
        private void RtbEditorKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                Undo();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                Redo();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                SaveToFile();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.C)
            {
                // Fix: nếu đang Select All thì copy bản đã bỏ xuống dòng cuối
                if (IsAllSelected(rtbEditor))
                {
                    var trimmed = rtbEditor.Text.TrimEnd('\r', '\n');
                    if (string.IsNullOrEmpty(trimmed))
                        Clipboard.Clear();        // không copy kí tự xuống dòng “trống”
                    else
                        Clipboard.SetText(trimmed);

                    e.SuppressKeyPress = true;    // chặn Copy mặc định để không copy CR/LF cuối
                    e.Handled = true;
                }
                // nếu không phải Select All => để mặc định (KHÔNG suppress)
            }
        }

        // Nút và menu đều dùng các handler sau
        private void BtnUndoClick(object? sender, EventArgs e) => Undo();
        private void BtnRedoClick(object? sender, EventArgs e) => Redo();
        private void BtnSaveClick(object? sender, EventArgs e) => SaveToFile();
        private void ExitMenuClick(object? sender, EventArgs e) => Close();

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


        private void UpdateUndoRedoButtons()
        {
            bool canUndo = undoStack.Count > 0;
            bool canRedo = redoStack.Count > 0;

            // Nếu panel ẩn thì các nút vẫn tồn tại, nhưng ta check cho chắc
            if (btnUndo != null && !btnUndo.IsDisposed) btnUndo.Enabled = canUndo;
            if (btnRedo != null && !btnRedo.IsDisposed) btnRedo.Enabled = canRedo;

            // Đồng bộ menu
            if (undoToolStripMenuItem != null) undoToolStripMenuItem.Enabled = canUndo;
            if (redoToolStripMenuItem != null) redoToolStripMenuItem.Enabled = canRedo;
        }

        private void UpdatePlaceholder()
        {
            if (lblPlaceholder == null) return;
            // Hiện khi trống
            lblPlaceholder.Visible = string.IsNullOrEmpty(rtbEditor.Text);
        }

        private void PositionPlaceholder()
        {   
            if (lblPlaceholder == null) return;
            lblPlaceholder.Location = new Point(rtbEditor.Left + 6, rtbEditor.Top + 6);
        }
        // File association
        private const string CustomExt = ".nh3";                       // đuôi riêng
        private const string CustomProgId = "DOAN_Nhom3.Document";     // ProgID cho Windows

        [DllImport("Shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
        private const uint SHCNE_ASSOCCHANGED = 0x8000000;
        private const uint SHCNF_IDLIST = 0x0;
        // Đăng ký mở .nh3 bằng ứng dụng hiện tại (phạm vi user)

        private static void EnsureFileAssociation()
        {
            try
            {
                using var classes = Registry.CurrentUser.OpenSubKey(@"Software\Classes", writable: true)
                                   ?? Registry.CurrentUser.CreateSubKey(@"Software\Classes");

                // 1) .nh3 -> ProgID
                using (var extKey = classes.OpenSubKey(CustomExt, writable: true) ?? classes.CreateSubKey(CustomExt))
                {
                    var cur = extKey.GetValue(null) as string;
                    if (!string.Equals(cur, CustomProgId, StringComparison.OrdinalIgnoreCase))
                        extKey.SetValue(null, CustomProgId);

                    using var openWith = extKey.OpenSubKey("OpenWithProgids", writable: true) ?? extKey.CreateSubKey("OpenWithProgids");
                    openWith.SetValue(CustomProgId, string.Empty, RegistryValueKind.String);
                }

                // 2) ProgID -> lệnh mở + icon
                using (var progKey = classes.OpenSubKey(CustomProgId, writable: true) ?? classes.CreateSubKey(CustomProgId))
                {
                    progKey.SetValue(null, "Nhom3 Document"); // mô tả

                    using var iconKey = progKey.OpenSubKey("DefaultIcon", writable: true) ?? progKey.CreateSubKey("DefaultIcon");
                    iconKey.SetValue(null, $"\"{Application.ExecutablePath}\",0");

                    using var cmdKey = progKey.OpenSubKey(@"shell\open\command", writable: true) ?? progKey.CreateSubKey(@"shell\open\command");
                    cmdKey.SetValue(null, $"\"{Application.ExecutablePath}\" \"%1\"");
                }

                // 3) Báo cho Explorer biết association đã thay đổi
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
            }
            catch
            {
                // Bị chặn bởi policy hoặc lỗi registry -> bỏ qua để app vẫn chạy
            }
        }
        private void LoadFromFile(string path)
        {
            try
            {
                if (!File.Exists(path)) return;

                isInternalChange = true;
                rtbEditor.Text = File.ReadAllText(path, Encoding.UTF8);
                isInternalChange = false;

                previousText = rtbEditor.Text;
                currentFilePath = path;
                this.Text = "Trình soạn thảo - " + Path.GetFileName(currentFilePath);

                undoStack.Clear();
                redoStack.Clear();
                UpdateUndoRedoButtons();
                UpdatePlaceholder();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở file:\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SaveToFile()
        {
            try
            {
                if (!string.IsNullOrEmpty(currentFilePath))
                {
                    File.WriteAllText(currentFilePath!, rtbEditor.Text, Encoding.UTF8);
                    MessageBox.Show("Đã lưu: " + currentFilePath, "Lưu file",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    using var sfd = new SaveFileDialog
                    {
                        Title = "Lưu văn bản",
                        Filter = "Nhom3 Document (*.nh3)|*.nh3|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                        DefaultExt = "nh3",
                        AddExtension = true,
                        FileName = "document.nh3"
                    };
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllText(sfd.FileName, rtbEditor.Text, Encoding.UTF8);
                        currentFilePath = sfd.FileName;
                        this.Text = "Trình soạn thảo - " + Path.GetFileName(currentFilePath);
                        MessageBox.Show("Đã lưu: " + currentFilePath, "Lưu file",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu file:\n" + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
            private static bool IsAllSelected(RichTextBox r)
        {
            return r.SelectionStart == 0 && r.SelectionLength == r.TextLength;
        }
        private void CopySelectedText()
        {
            if (IsAllSelected(rtbEditor))
            {
                var trimmed = rtbEditor.Text.TrimEnd('\r', '\n');
                if (string.IsNullOrEmpty(trimmed)) Clipboard.Clear();
                else Clipboard.SetText(trimmed);
            }
            else
            {
                rtbEditor.Copy(); // copy mặc định cho lựa chọn thường
            }
        }

    }
}
