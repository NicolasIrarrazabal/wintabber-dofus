using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DofusMiniTabber
{
    public partial class Form1 : Form
    {
        private readonly ToolStrip _toolbar = new();
        private readonly ToolStrip _floatingToolbar = new();
        private readonly ToolStripButton _captureButton = new("⚡ CAPTURAR VENTANASxx");
        private readonly ToolStripButton _savePositionButton = new("💾 GUARDAR LAYOUT");
        private readonly ToolStripButton _restorePositionButton = new("🔄 CARGAR LAYOUT");
        private readonly ToolStripButton _manageLayoutsButton = new("📋 GESTIONAR LAYOUTS");
        private readonly ToolStripButton _hideMenuButton = new("👁️ OCULTAR MENÚ");
        private readonly ToolStripLabel _hotkeysLabel = new("[F1/F2] Ant/Sig | [F3] Menú | [F4] Guardar | [F5] Cargar | [F6] Gestionar | [Ctrl+Alt+1..9] Directo");
        private readonly TabControl _tabs = new();
        private readonly ContextMenuStrip _tabMenu = new();
        private readonly System.Windows.Forms.Timer _resizeDebounceTimer = new();
        private readonly System.Windows.Forms.Timer _updateTitleTimer = new();
        private readonly Dictionary<IntPtr, EmbeddedWindowInfo> _embeddedByHwnd = new();
        private readonly NotifyIcon _trayIcon = new();
        private readonly ContextMenuStrip _trayMenu = new();
        private bool _menuVisible = true;
        private bool _isCapturing;
        private TabPage? _previousTab;
        private TabPage? _draggedTab;
        private string _lastSavedConfigurationName = "default";

        // IDs de Hotkeys
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID_PREV = 1;
        private const int HOTKEY_ID_NEXT = 2;
        private const int HOTKEY_ID_TOGGLE_MENU = 3;
        private const int HOTKEY_ID_SAVE_POSITION = 4;
        private const int HOTKEY_ID_RESTORE_POSITION = 5;
        private const int HOTKEY_ID_MANAGE_LAYOUTS = 6;
        private const int HOTKEY_ID_NUM_START = 10;

        // Modificadores
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_NOREPEAT = 0x4000;

        // Estilos de Ventana
        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        private const int WS_BORDER = 0x00800000;
        private const int WS_DLGFRAME = 0x00400000;
        private const int SW_SHOW = 5;
        private const int SW_HIDE = 0;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_FRAMECHANGED = 0x0020;
        private const int SWP_NOREDRAW = 0x0008;
        private const int SWP_NOCOPYBITS = 0x0100;

        // RedrawWindow flags
        private const uint RDW_FRAME = 0x0400;
        private const uint RDW_INVALIDATE = 0x0001;
        private const uint RDW_UPDATENOW = 0x0100;
        private const uint RDW_ALLCHILDREN = 0x0080;

        // Process access
        private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
        private const string TARGET_PROCESS_NAME = "Dofus Retro.exe";

        [DllImport("user32.dll")] private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")] private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);
        [DllImport("user32.dll")] private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);
        private const int SM_CXFRAME = 32;
        private const int SM_CYFRAME = 33;
        private const int SM_CYCAPTION = 4;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        public Form1()
        {
            ConfigureForm();
            BuildUi();
            SetupTrayIcon();
            RegisterBaseHotkeys();
            RegisterNumberHotkeys();

            _updateTitleTimer.Interval = 300;
            _updateTitleTimer.Tick += (_, _) => UpdateDynamicTitles();
            _updateTitleTimer.Start();
        }

        private void ConfigureForm()
        {
            Text = "Wintabber Dofus";
            BackColor = Color.FromArgb(0x0F, 0x19, 0x23);
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            FormClosing += OnFormClosingRestoreWindows;
            TopMost = false;
        }

        private void SetupTrayIcon()
        {
            // El tray icon existe solo para el mecanismo de instancia única.
            // Minimizar funciona con el comportamiento normal de Windows.
            _trayIcon.Icon = Icon ?? CreateFallbackIcon();
            _trayIcon.Text = "Wintabber Dofus";
            _trayIcon.Visible = true;

            var restoreItem   = new ToolStripMenuItem("🖥️ Restaurar",        null, (_, _) => RestoreWindow());
            var captureItem   = new ToolStripMenuItem("⚡ Capturar ventanas", null, (_, _) => CaptureWindows());
            var separatorItem = new ToolStripSeparator();
            var exitItem      = new ToolStripMenuItem("❌ Salir",             null, (_, _) => ExitApplication());

            _trayMenu.Items.Add(restoreItem);
            _trayMenu.Items.Add(captureItem);
            _trayMenu.Items.Add(separatorItem);
            _trayMenu.Items.Add(exitItem);
            _trayMenu.BackColor = Color.FromArgb(0x1E, 0x2A, 0x38);
            _trayMenu.ForeColor = Color.White;
            _trayMenu.RenderMode = ToolStripRenderMode.System;

            _trayIcon.ContextMenuStrip = _trayMenu;
            _trayIcon.DoubleClick += (_, _) => RestoreWindow();
        }

        private static Icon CreateFallbackIcon()
        {
            using var bmp = new Bitmap(16, 16);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(0x1E, 0x2A, 0x38));
            using var font = new Font("Arial", 8f, FontStyle.Bold);
            g.DrawString("W", font, Brushes.White, 1f, 1f);
            return Icon.FromHandle(bmp.GetHicon());
        }

        /// Trae la ventana al frente (desde minimizado o desde otra instancia).
        private void RestoreWindow()
        {
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Maximized;
            Activate();
        }

        // Alias para compatibilidad con el menú tray (restaurar desde hidden no aplica ya)
        private void RestoreFromTray() => RestoreWindow();

        private void ExitApplication() => Close();

        private void BuildUi()
        {
            SuspendLayout();

            _toolbar.Dock    = DockStyle.Top;
            _toolbar.Height  = 1;
            _toolbar.Visible = false;

            _floatingToolbar.Dock        = DockStyle.Top;
            _floatingToolbar.BackColor   = Color.FromArgb(0x1E, 0x2A, 0x38);
            _floatingToolbar.ForeColor   = Color.White;
            _floatingToolbar.GripStyle   = ToolStripGripStyle.Hidden;
            _floatingToolbar.CanOverflow = false;
            _floatingToolbar.Stretch     = true;

            _captureButton.Click         += (_, _) => CaptureWindows();
            _savePositionButton.Click    += (_, _) => SaveCurrentPositions();
            _restorePositionButton.Click += (_, _) => QuickRestoreLayout();
            _manageLayoutsButton.Click   += (_, _) => OpenLayoutManager();
            _hideMenuButton.Click        += (_, _) => ToggleFloatingMenu();

            _hotkeysLabel.Alignment = ToolStripItemAlignment.Right;

            _floatingToolbar.Items.Add(_captureButton);
            _floatingToolbar.Items.Add(new ToolStripSeparator());
            _floatingToolbar.Items.Add(_savePositionButton);
            _floatingToolbar.Items.Add(_restorePositionButton);
            _floatingToolbar.Items.Add(_manageLayoutsButton);
            _floatingToolbar.Items.Add(new ToolStripSeparator());
            _floatingToolbar.Items.Add(_hideMenuButton);
            _floatingToolbar.Items.Add(new ToolStripSeparator());
            _floatingToolbar.Items.Add(_hotkeysLabel);

            _tabs.Dock = DockStyle.Fill;
            _tabs.SelectedIndexChanged += (_, _) => OnTabChanged();
            _tabs.MouseDown += Tabs_MouseDown;
            _tabs.MouseMove += Tabs_MouseMove;
            _tabs.MouseUp   += Tabs_MouseUp;

            var liberarItem = new ToolStripMenuItem("Liberar", null, (_, _) => ReleaseCurrentTab());
            var cerrarItem  = new ToolStripMenuItem("Cerrar",  null, (_, _) => CloseCurrentTab());
            _tabMenu.Items.Add(liberarItem);
            _tabMenu.Items.Add(cerrarItem);

            Controls.Add(_tabs);
            Controls.Add(_floatingToolbar);
            Controls.Add(_toolbar);

            _resizeDebounceTimer.Interval = 60;
            _resizeDebounceTimer.Tick += (_, _) =>
            {
                _resizeDebounceTimer.Stop();
                ResizeActiveEmbeddedWindow();
            };

            ResumeLayout(true);
        }

        private void ToggleFloatingMenu()
        {
            _menuVisible = !_menuVisible;
            _floatingToolbar.Visible = _menuVisible;
            _hideMenuButton.Text = _menuVisible ? "👁️ OCULTAR MENÚ" : "👁️‍🗨️ MOSTRAR MENÚ";
            ScheduleResizeActiveTab();
        }

        // Sin override de OnResize: comportamiento estándar de Windows al minimizar.

        // ── Drag & drop tabs ──────────────────────────────────────────────
        private void Tabs_MouseDown(object? sender, MouseEventArgs e)
        {
            for (int i = 0; i < _tabs.TabCount; i++)
            {
                if (_tabs.GetTabRect(i).Contains(e.Location))
                {
                    _draggedTab = _tabs.TabPages[i];
                    break;
                }
            }
        }

        private void Tabs_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_draggedTab == null || e.Button != MouseButtons.Left) return;

            for (int i = 0; i < _tabs.TabCount; i++)
            {
                if (_tabs.GetTabRect(i).Contains(e.Location))
                {
                    var targetTab = _tabs.TabPages[i];
                    if (targetTab == _draggedTab) return;
                    _tabs.TabPages.Remove(_draggedTab);
                    _tabs.TabPages.Insert(i, _draggedTab);
                    _tabs.SelectedTab = _draggedTab;
                    break;
                }
            }
        }

        private void Tabs_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int i = 0; i < _tabs.TabCount; i++)
                {
                    if (_tabs.GetTabRect(i).Contains(e.Location))
                    {
                        _tabs.SelectedIndex = i;
                        _tabMenu.Show(_tabs, e.Location);
                        break;
                    }
                }
            }
            _draggedTab = null;
        }

        // ── Release / close tab ───────────────────────────────────────────
        private void ReleaseCurrentTab()
        {
            if (_tabs.SelectedTab == null) return;
            var tab  = _tabs.SelectedTab;
            var item = _embeddedByHwnd.Values.FirstOrDefault(v => v.TabPage == tab);
            if (item == null) return;

            SetParent(item.Hwnd, IntPtr.Zero);
            SetWindowLong(item.Hwnd, GWL_STYLE, item.OriginalStyle);
            SetWindowPos(item.Hwnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
            ShowWindow(item.Hwnd, SW_SHOW);

            _embeddedByHwnd.Remove(item.Hwnd);
            _tabs.TabPages.Remove(tab);
            ReorderTabs();
        }

        private void CloseCurrentTab()
        {
            if (_tabs.SelectedTab == null) return;
            var tab  = _tabs.SelectedTab;
            var item = _embeddedByHwnd.Values.FirstOrDefault(v => v.TabPage == tab);
            if (item != null)
            {
                SetParent(item.Hwnd, IntPtr.Zero);
                SetWindowLong(item.Hwnd, GWL_STYLE, item.OriginalStyle);
                SetWindowPos(item.Hwnd, IntPtr.Zero, 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
                ShowWindow(item.Hwnd, SW_SHOW);
                _embeddedByHwnd.Remove(item.Hwnd);
            }

            _tabs.TabPages.Remove(tab);
            ReorderTabs();
        }

        private void ReorderTabs()
        {
            for (int i = 0; i < _tabs.TabCount; i++)
            {
                var page = _tabs.TabPages[i];
                var info = _embeddedByHwnd.Values.FirstOrDefault(v => v.TabPage == page);
                string title = info != null
                    ? (GetWindowTitle(info.Hwnd).Trim() is { Length: > 0 } t ? t : TARGET_PROCESS_NAME)
                    : page.Text;
                page.Text = $"{i + 1}. {title}";
            }
        }

        // ── Hotkeys ───────────────────────────────────────────────────────
        private void RegisterBaseHotkeys()
        {
            RegisterHotKey(Handle, HOTKEY_ID_PREV,             MOD_NOREPEAT, (uint)Keys.F1);
            RegisterHotKey(Handle, HOTKEY_ID_NEXT,             MOD_NOREPEAT, (uint)Keys.F2);
            RegisterHotKey(Handle, HOTKEY_ID_TOGGLE_MENU,      MOD_NOREPEAT, (uint)Keys.F3);
            RegisterHotKey(Handle, HOTKEY_ID_SAVE_POSITION,    MOD_NOREPEAT, (uint)Keys.F4);
            RegisterHotKey(Handle, HOTKEY_ID_RESTORE_POSITION, MOD_NOREPEAT, (uint)Keys.F5);
            RegisterHotKey(Handle, HOTKEY_ID_MANAGE_LAYOUTS,   MOD_NOREPEAT, (uint)Keys.F6);
        }

        private void RegisterNumberHotkeys()
        {
            for (int i = 1; i <= 9; i++)
                RegisterHotKey(Handle, HOTKEY_ID_NUM_START + i - 1,
                               MOD_CONTROL | MOD_ALT, (uint)(Keys.D0 + i));
        }

        // ── WndProc: single-instance + hotkeys ───────────────────────────
        protected override void WndProc(ref Message m)
        {
            // ── Instancia única: otra instancia nos pide salir al frente ──
            if (m.Msg != 0 && (uint)m.Msg == Program.WM_BRING_TO_FRONT)
            {
                RestoreWindow();
                return;
            }

            // ── Hotkeys ───────────────────────────────────────────────────
            if (m.Msg == WM_HOTKEY)
            {
                var id = m.WParam.ToInt32();
                if      (id == HOTKEY_ID_PREV)             PrevTab();
                else if (id == HOTKEY_ID_NEXT)             NextTab();
                else if (id == HOTKEY_ID_TOGGLE_MENU)      ToggleMenu();
                else if (id == HOTKEY_ID_SAVE_POSITION)    SaveCurrentPositions();
                else if (id == HOTKEY_ID_RESTORE_POSITION) QuickRestoreLayout();
                else if (id == HOTKEY_ID_MANAGE_LAYOUTS)   OpenLayoutManager();
                else if (id >= HOTKEY_ID_NUM_START && id <= HOTKEY_ID_NUM_START + 8)
                    JumpToTab(id - HOTKEY_ID_NUM_START);
            }

            base.WndProc(ref m);
        }

        private void JumpToTab(int index)
        {
            if (index >= 0 && index < _tabs.TabCount)
                _tabs.SelectedIndex = index;
        }

        private void ToggleMenu() => ToggleFloatingMenu();

        private void NextTab()
        {
            if (_tabs.TabCount == 0) return;
            _tabs.SelectedIndex = (_tabs.SelectedIndex + 1) % _tabs.TabCount;
        }

        private void PrevTab()
        {
            if (_tabs.TabCount == 0) return;
            _tabs.SelectedIndex = (_tabs.SelectedIndex - 1 + _tabs.TabCount) % _tabs.TabCount;
        }

        // ── Capture ───────────────────────────────────────────────────────
        private void CaptureWindows()
        {
            if (_isCapturing) return;
            _isCapturing = true;
            try
            {
                EnumWindows((hwnd, _) =>
                {
                    if (!IsWindowVisible(hwnd)) return true;
                    if (_embeddedByHwnd.ContainsKey(hwnd)) return true;
                    if (!IsDofusRetroProcess(hwnd)) return true;

                    var title = GetWindowTitle(hwnd);
                    EmbedWindow(hwnd, string.IsNullOrWhiteSpace(title) ? TARGET_PROCESS_NAME : title);
                    return true;
                }, IntPtr.Zero);
            }
            finally
            {
                _isCapturing = false;
            }
        }

        private static bool IsDofusRetroProcess(IntPtr hwnd)
        {
            GetWindowThreadProcessId(hwnd, out uint pid);
            if (pid == 0) return false;

            var hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
            if (hProcess == IntPtr.Zero) return false;

            try
            {
                var sb    = new StringBuilder(1024);
                uint size = (uint)sb.Capacity;
                if (!QueryFullProcessImageName(hProcess, 0, sb, ref size)) return false;
                return System.IO.Path.GetFileName(sb.ToString())
                    .Equals(TARGET_PROCESS_NAME, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        private void EmbedWindow(IntPtr hwnd, string title)
        {
            var panel     = new Panel { Dock = DockStyle.Fill, BackColor = Color.Black };
            string cleanTitle = string.IsNullOrWhiteSpace(title) ? TARGET_PROCESS_NAME : title.Trim();
            var tab       = new TabPage($"{_tabs.TabCount + 1}. {cleanTitle}");
            tab.Controls.Add(panel);
            _tabs.TabPages.Add(tab);

            panel.CreateControl();

            var originalStyle = GetWindowLong(hwnd, GWL_STYLE);
            var stripped      = originalStyle & ~WS_CAPTION & ~WS_THICKFRAME & ~WS_BORDER & ~WS_DLGFRAME;
            SetWindowLong(hwnd, GWL_STYLE, stripped);

            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

            SetParent(hwnd, panel.Handle);
            ShowWindow(hwnd, SW_SHOW);

            _embeddedByHwnd[hwnd] = new EmbeddedWindowInfo(hwnd, panel, tab, originalStyle);

            panel.Resize += (_, _) =>
            {
                var info = _embeddedByHwnd.Values.FirstOrDefault(v => v.HostPanel == panel);
                if (info != null) ResizeWindowIfNeeded(info);
            };

            BeginInvoke(() =>
            {
                if (_embeddedByHwnd.TryGetValue(hwnd, out var info))
                {
                    info.LastKnownSize = Size.Empty;
                    ResizeWindowIfNeeded(info);
                }
            });

            ScheduleResizeActiveTab();
        }

        private void ResizeWindowIfNeeded(EmbeddedWindowInfo info)
        {
            var size = info.HostPanel.ClientSize;
            if (size.Width <= 0 || size.Height <= 0) return;
            if (size == info.LastKnownSize) return;

            MoveWindow(info.Hwnd, 0, 0, size.Width, size.Height, true);
            SetWindowPos(info.Hwnd, IntPtr.Zero, 0, 0, size.Width, size.Height,
                SWP_NOZORDER | SWP_FRAMECHANGED);
            RedrawWindow(info.Hwnd, IntPtr.Zero, IntPtr.Zero,
                RDW_FRAME | RDW_INVALIDATE | RDW_UPDATENOW | RDW_ALLCHILDREN);

            info.LastKnownSize = size;
        }

        // ── Tab switching ─────────────────────────────────────────────────
        private void OnTabChanged()
        {
            var currentTab = _tabs.SelectedTab;
            if (currentTab == null) return;

            if (_previousTab != null && _previousTab != currentTab)
            {
                var prev = _embeddedByHwnd.Values.FirstOrDefault(v => v.TabPage == _previousTab);
                if (prev != null)
                    SetWindowPos(prev.Hwnd, IntPtr.Zero, 0, 0, 0, 0,
                        SWP_NOREDRAW | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER);
            }

            var active = _embeddedByHwnd.Values.FirstOrDefault(v => v.TabPage == currentTab);
            if (active != null)
            {
                active.LastKnownSize = Size.Empty;
                ResizeWindowIfNeeded(active);
            }

            _previousTab = currentTab;
        }

        private void ScheduleResizeActiveTab()
        {
            _resizeDebounceTimer.Stop();
            _resizeDebounceTimer.Start();
        }

        private void ResizeActiveEmbeddedWindow()
        {
            if (_tabs.SelectedTab is null) return;
            var active = _embeddedByHwnd.Values.FirstOrDefault(v => v.TabPage == _tabs.SelectedTab);
            if (active is null) return;
            active.LastKnownSize = Size.Empty;
            ResizeWindowIfNeeded(active);
        }

        // ── Dynamic title update ──────────────────────────────────────────
        private void UpdateDynamicTitles()
        {
            foreach (var info in _embeddedByHwnd.Values)
            {
                string currentTitle = GetWindowTitle(info.Hwnd).Trim();
                if (string.IsNullOrWhiteSpace(currentTitle))
                    currentTitle = TARGET_PROCESS_NAME;

                int    index    = _tabs.TabPages.IndexOf(info.TabPage) + 1;
                string expected = $"{index}. {currentTitle}";
                if (info.TabPage.Text != expected)
                    info.TabPage.Text = expected;
            }
        }

        // ── Window Position Management ────────────────────────────────────
        private void SaveCurrentPositions()
        {
            try
            {
                using var nameDialog = new Form();
                nameDialog.Text            = "Guardar Layout";
                nameDialog.Size            = new Size(420, 250);
                nameDialog.StartPosition   = FormStartPosition.CenterParent;
                nameDialog.BackColor       = Color.FromArgb(0x0F, 0x19, 0x23);
                nameDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                nameDialog.MaximizeBox     = false;
                nameDialog.MinimizeBox     = false;
                nameDialog.Font            = new Font("Segoe UI", 9F);

                var titleLabel = new Label
                {
                    Text      = " Guardar Configuración Actual",
                    ForeColor = Color.White,
                    Location  = new Point(20, 15),
                    Size      = new Size(350, 30),
                    Font      = new Font("Segoe UI", 12F, FontStyle.Bold)
                };
                var nameLabel = new Label
                {
                    Text      = "Nombre del Layout:",
                    ForeColor = Color.White,
                    Location  = new Point(20, 60),
                    Size      = new Size(120, 20)
                };
                var nameTextBox = new TextBox
                {
                    Location    = new Point(20, 85),
                    Size        = new Size(360, 30),
                    BackColor   = Color.FromArgb(0x1E, 0x2A, 0x38),
                    ForeColor   = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font        = new Font("Segoe UI", 10F)
                };
                var descLabel = new Label
                {
                    Text      = "Descripción (opcional):",
                    ForeColor = Color.White,
                    Location  = new Point(20, 125),
                    Size      = new Size(150, 20)
                };
                var descTextBox = new TextBox
                {
                    Location    = new Point(20, 150),
                    Size        = new Size(360, 30),
                    BackColor   = Color.FromArgb(0x1E, 0x2A, 0x38),
                    ForeColor   = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font        = new Font("Segoe UI", 10F)
                };
                var saveButton = new Button
                {
                    Text             = " GUARDAR LAYOUT",
                    Location         = new Point(100, 195),
                    Size             = new Size(100, 35),
                    BackColor        = Color.FromArgb(0x28, 0xA7, 0x45),
                    ForeColor        = Color.White,
                    FlatStyle        = FlatStyle.Flat,
                    Font             = new Font("Segoe UI", 10F, FontStyle.Bold),
                    DialogResult     = DialogResult.OK,
                    UseVisualStyleBackColor = false
                };
                saveButton.FlatAppearance.BorderSize = 0;
                var cancelButton = new Button
                {
                    Text             = " CANCELAR",
                    Location         = new Point(220, 195),
                    Size             = new Size(100, 35),
                    BackColor        = Color.FromArgb(0xDC, 0x35, 0x45),
                    ForeColor        = Color.White,
                    FlatStyle        = FlatStyle.Flat,
                    Font             = new Font("Segoe UI", 10F, FontStyle.Bold),
                    DialogResult     = DialogResult.Cancel,
                    UseVisualStyleBackColor = false
                };
                cancelButton.FlatAppearance.BorderSize = 0;

                nameDialog.Controls.AddRange(new Control[]
                    { titleLabel, nameLabel, nameTextBox, descLabel, descTextBox, saveButton, cancelButton });
                nameDialog.AcceptButton = saveButton;
                nameDialog.CancelButton = cancelButton;

                if (nameDialog.ShowDialog(this) == DialogResult.OK &&
                    !string.IsNullOrWhiteSpace(nameTextBox.Text))
                    SaveLayoutWithName(nameTextBox.Text, descTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al mostrar diálogo de guardado: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveLayoutWithName(string layoutName, string description)
        {
            try
            {
                var positions = new List<WindowPositionManager.WindowPosition>();
                for (int i = 0; i < _tabs.TabCount; i++)
                {
                    var tabPage = _tabs.TabPages[i];
                    var info    = _embeddedByHwnd.Values.FirstOrDefault(v => v.TabPage == tabPage);
                    if (info != null)
                    {
                        string windowName = GetWindowTitle(info.Hwnd).Trim();
                        if (string.IsNullOrWhiteSpace(windowName)) windowName = TARGET_PROCESS_NAME;
                        positions.Add(new WindowPositionManager.WindowPosition
                        {
                            WindowName = windowName,
                            Position   = i
                        });
                    }
                }
                WindowPositionManager.SaveConfiguration(layoutName, positions, description);
                MessageBox.Show($"Layout '{layoutName}' guardado con {positions.Count} ventanas.",
                    "Guardado Exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar layout: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void QuickRestoreLayout()
        {
            try
            {
                var layouts = WindowPositionManager.GetConfigurationNames();
                if (layouts.Count == 0)
                {
                    MessageBox.Show("No hay layouts guardados.", "Sin Layouts",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                OpenLayoutManager();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al restaurar layout: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenLayoutManager()
        {
            try
            {
                using var layoutManager = new LayoutSelectorForm();
                layoutManager.ShowDialog(this);

                if (layoutManager.DialogResult == DialogResult.OK)
                {
                    if (layoutManager.ShouldLoad)
                        RestoreLayout(layoutManager.SelectedLayout!);
                    else
                        SaveLayout(layoutManager.SelectedLayout!);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir gestor de layouts: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveLayout(string layoutName)
        {
            try
            {
                var positions = new List<WindowPositionManager.WindowPosition>();
                for (int i = 0; i < _tabs.TabCount; i++)
                {
                    var tabPage = _tabs.TabPages[i];
                    var info    = _embeddedByHwnd.Values.FirstOrDefault(v => v.TabPage == tabPage);
                    if (info != null)
                    {
                        string windowName = GetWindowTitle(info.Hwnd).Trim();
                        if (string.IsNullOrWhiteSpace(windowName)) windowName = TARGET_PROCESS_NAME;
                        positions.Add(new WindowPositionManager.WindowPosition
                        {
                            WindowName = windowName,
                            Position   = i
                        });
                    }
                }
                WindowPositionManager.SaveConfiguration(layoutName, positions);
                MessageBox.Show($"Layout '{layoutName}' guardado con {positions.Count} ventanas.",
                    "Guardado Exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar layout: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestoreLayout(string layoutName)
        {
            try
            {
                var config = WindowPositionManager.LoadConfiguration(layoutName);
                if (config == null)
                {
                    MessageBox.Show($"No se encontró el layout '{layoutName}'.",
                        "Layout No Encontrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var currentWindows = new Dictionary<string, TabPage>();
                for (int i = 0; i < _tabs.TabCount; i++)
                {
                    var tabPage = _tabs.TabPages[i];
                    var info    = _embeddedByHwnd.Values.FirstOrDefault(v => v.TabPage == tabPage);
                    if (info != null)
                    {
                        string windowName = GetWindowTitle(info.Hwnd).Trim();
                        if (string.IsNullOrWhiteSpace(windowName)) windowName = TARGET_PROCESS_NAME;
                        currentWindows[windowName] = tabPage;
                    }
                }

                var orderedTabs = new List<TabPage>();
                foreach (var position in config.Positions.OrderBy(p => p.Position))
                {
                    if (currentWindows.TryGetValue(position.WindowName, out var tabPage))
                    {
                        orderedTabs.Add(tabPage);
                        currentWindows.Remove(position.WindowName);
                    }
                }
                foreach (var remainingTab in currentWindows.Values)
                    orderedTabs.Add(remainingTab);

                _tabs.TabPages.Clear();
                foreach (var tab in orderedTabs)
                    _tabs.TabPages.Add(tab);

                ReorderTabs();
                MessageBox.Show($"Layout '{layoutName}' restaurado exitosamente.",
                    "Restauración Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al restaurar layout: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestoreSavedPositions() => RestoreLayout(_lastSavedConfigurationName);

        // ── Cleanup ───────────────────────────────────────────────────────
        private void OnFormClosingRestoreWindows(object? sender, FormClosingEventArgs e)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();

            foreach (var item in _embeddedByHwnd.Values)
            {
                SetParent(item.Hwnd, IntPtr.Zero);
                SetWindowLong(item.Hwnd, GWL_STYLE, item.OriginalStyle);
                SetWindowPos(item.Hwnd, IntPtr.Zero, 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
                ShowWindow(item.Hwnd, SW_SHOW);
            }

            UnregisterHotKey(Handle, HOTKEY_ID_PREV);
            UnregisterHotKey(Handle, HOTKEY_ID_NEXT);
            UnregisterHotKey(Handle, HOTKEY_ID_TOGGLE_MENU);
            UnregisterHotKey(Handle, HOTKEY_ID_SAVE_POSITION);
            UnregisterHotKey(Handle, HOTKEY_ID_RESTORE_POSITION);
            UnregisterHotKey(Handle, HOTKEY_ID_MANAGE_LAYOUTS);
            for (int i = 0; i < 9; i++) UnregisterHotKey(Handle, HOTKEY_ID_NUM_START + i);
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private static string GetWindowTitle(IntPtr hwnd)
        {
            var sb = new StringBuilder(512);
            GetWindowText(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }

        private sealed class EmbeddedWindowInfo
        {
            public IntPtr  Hwnd          { get; }
            public Panel   HostPanel     { get; }
            public TabPage TabPage       { get; }
            public int     OriginalStyle { get; }
            public Size    LastKnownSize { get; set; } = Size.Empty;

            public EmbeddedWindowInfo(IntPtr hwnd, Panel hostPanel, TabPage tabPage, int originalStyle)
            {
                Hwnd          = hwnd;
                HostPanel     = hostPanel;
                TabPage       = tabPage;
                OriginalStyle = originalStyle;
            }
        }

        private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", EntryPoint = "GetWindowRect")] private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")] private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")] private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool repaint);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongW")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")] private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lpRectUpdate, IntPtr hrgnUpdate, uint flags);
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("kernel32.dll")] private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
        [DllImport("kernel32.dll")] private static extern bool CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref uint lpdwSize);
    }
}