using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DofusMiniTabber
{
    public partial class LayoutSelectorForm : Form
    {
        private readonly ListBox _layoutsListBox = new();
        private readonly TextBox _nameTextBox = new();
        private readonly TextBox _descriptionTextBox = new();
        private readonly Button _saveButton = new();
        private readonly Button _loadButton = new();
        private readonly Button _deleteButton = new();
        private readonly Button _newButton = new();
        private readonly Button _cancelButton = new();
        private readonly Label _titleLabel = new();
        private readonly Label _nameLabel = new();
        private readonly Label _descriptionLabel = new();

        public string? SelectedLayout { get; private set; }
        public bool ShouldLoad { get; private set; }

        public LayoutSelectorForm()
        {
            InitializeComponents();
            LoadLayouts();
        }

        private void InitializeComponents()
        {
            Text = "Gestor de Layouts - Wintabber Dofus";
            Size = new Size(550, 450);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(0x0F, 0x19, 0x23);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // Title
            _titleLabel.Text = "Gestionar Layouts de Ventanas";
            _titleLabel.ForeColor = Color.White;
            _titleLabel.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            _titleLabel.Location = new Point(20, 15);
            _titleLabel.Size = new Size(400, 30);

            // Instructions
            var instructionLabel = new Label
            {
                Text = "Selecciona un layout de la lista y elige una acción:",
                ForeColor = Color.FromArgb(0x6C, 0x75, 0x7D),
                Location = new Point(20, 50),
                Size = new Size(400, 20),
                Font = new Font("Segoe UI", 9F)
            };

            // Layouts List
            _layoutsListBox.Location = new Point(20, 80);
            _layoutsListBox.Size = new Size(300, 250);
            _layoutsListBox.BackColor = Color.FromArgb(0x1E, 0x2A, 0x38);
            _layoutsListBox.ForeColor = Color.White;
            _layoutsListBox.BorderStyle = BorderStyle.FixedSingle;
            _layoutsListBox.Font = new Font("Segoe UI", 9F);
            _layoutsListBox.SelectedIndexChanged += LayoutsListBox_SelectedIndexChanged;

            // Selection indicator
            var selectionLabel = new Label
            {
                Text = "Layout seleccionado:",
                ForeColor = Color.White,
                Location = new Point(340, 80),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            // Name Label
            _nameLabel.Text = "Nombre:";
            _nameLabel.ForeColor = Color.White;
            _nameLabel.Location = new Point(340, 110);
            _nameLabel.Size = new Size(60, 20);

            // Name TextBox
            _nameTextBox.Location = new Point(340, 135);
            _nameTextBox.Size = new Size(180, 30);
            _nameTextBox.BackColor = Color.FromArgb(0x1E, 0x2A, 0x38);
            _nameTextBox.ForeColor = Color.White;
            _nameTextBox.BorderStyle = BorderStyle.FixedSingle;
            _nameTextBox.Font = new Font("Segoe UI", 10F);

            // Description Label
            _descriptionLabel.Text = "Descripción:";
            _descriptionLabel.ForeColor = Color.White;
            _descriptionLabel.Location = new Point(340, 175);
            _descriptionLabel.Size = new Size(80, 20);

            // Description TextBox
            _descriptionTextBox.Location = new Point(340, 200);
            _descriptionTextBox.Size = new Size(180, 60);
            _descriptionTextBox.BackColor = Color.FromArgb(0x1E, 0x2A, 0x38);
            _descriptionTextBox.ForeColor = Color.White;
            _descriptionTextBox.BorderStyle = BorderStyle.FixedSingle;
            _descriptionTextBox.Multiline = true;
            _descriptionTextBox.Font = new Font("Segoe UI", 9F);

            // Buttons
            _newButton.Text = "NUEVO";
            _newButton.Location = new Point(20, 350);
            _newButton.Size = new Size(90, 45);
            _newButton.BackColor = Color.FromArgb(0x17, 0xA2, 0xB8);
            _newButton.ForeColor = Color.White;
            _newButton.FlatStyle = FlatStyle.Flat;
            _newButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _newButton.FlatAppearance.BorderSize = 0;
            _newButton.Click += NewButton_Click;

            _saveButton.Text = "GUARDAR";
            _saveButton.Location = new Point(120, 350);
            _saveButton.Size = new Size(90, 45);
            _saveButton.BackColor = Color.FromArgb(0x28, 0xA7, 0x45);
            _saveButton.ForeColor = Color.White;
            _saveButton.FlatStyle = FlatStyle.Flat;
            _saveButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _saveButton.FlatAppearance.BorderSize = 0;
            _saveButton.Click += SaveButton_Click;

            _loadButton.Text = "CARGAR";
            _loadButton.Location = new Point(220, 350);
            _loadButton.Size = new Size(90, 45);
            _loadButton.BackColor = Color.FromArgb(0x00, 0x7A, 0xCC);
            _loadButton.ForeColor = Color.White;
            _loadButton.FlatStyle = FlatStyle.Flat;
            _loadButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _loadButton.FlatAppearance.BorderSize = 0;
            _loadButton.Click += LoadButton_Click;

            _deleteButton.Text = "ELIMINAR";
            _deleteButton.Location = new Point(320, 350);
            _deleteButton.Size = new Size(90, 45);
            _deleteButton.BackColor = Color.FromArgb(0xDC, 0x35, 0x45);
            _deleteButton.ForeColor = Color.White;
            _deleteButton.FlatStyle = FlatStyle.Flat;
            _deleteButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _deleteButton.FlatAppearance.BorderSize = 0;
            _deleteButton.Click += DeleteButton_Click;

            _cancelButton.Text = "CERRAR";
            _cancelButton.Location = new Point(420, 350);
            _cancelButton.Size = new Size(90, 45);
            _cancelButton.BackColor = Color.FromArgb(0x6C, 0x75, 0x7D);
            _cancelButton.ForeColor = Color.White;
            _cancelButton.FlatStyle = FlatStyle.Flat;
            _cancelButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.Click += CancelButton_Click;

            // Add controls
            Controls.AddRange(new Control[] {
                _titleLabel, instructionLabel, _layoutsListBox, selectionLabel,
                _nameLabel, _nameTextBox, _descriptionLabel, _descriptionTextBox,
                _newButton, _saveButton, _loadButton, _deleteButton, _cancelButton
            });
        }

        private void LoadLayouts()
        {
            _layoutsListBox.Items.Clear();
            var configurations = WindowPositionManager.GetAllConfigurations();
            
            foreach (var config in configurations.OrderByDescending(c => c.CreatedAt))
            {
                string displayText = string.IsNullOrEmpty(config.Description) 
                    ? $"{config.Name} ({config.CreatedAt:dd/MM/yyyy HH:mm})"
                    : $"{config.Name} - {config.Description} ({config.CreatedAt:dd/MM/yyyy HH:mm})";
                _layoutsListBox.Items.Add(new LayoutItem(config.Name, displayText));
            }
        }

        private void LayoutsListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_layoutsListBox.SelectedItem is LayoutItem item)
            {
                var config = WindowPositionManager.LoadConfiguration(item.Name);
                if (config != null)
                {
                    _nameTextBox.Text = config.Name;
                    _descriptionTextBox.Text = config.Description;
                }
            }
        }

        private void NewButton_Click(object? sender, EventArgs e)
        {
            _nameTextBox.Text = "";
            _descriptionTextBox.Text = "";
            _nameTextBox.Focus();
        }

        private void SaveButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MessageBox.Show("Por favor ingrese un nombre para el layout.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // This will be handled by the main form
                SelectedLayout = _nameTextBox.Text;
                ShouldLoad = false;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar layout: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadButton_Click(object? sender, EventArgs e)
        {
            if (_layoutsListBox.SelectedItem is LayoutItem item)
            {
                SelectedLayout = item.Name;
                ShouldLoad = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Por favor selecciona un layout de la lista para cargar.", 
                    "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteButton_Click(object? sender, EventArgs e)
        {
            if (_layoutsListBox.SelectedItem is LayoutItem item)
            {
                var result = MessageBox.Show($"¿Estás seguro que deseas eliminar el layout '{item.Name}'?\n\nEsta acción no se puede deshacer.", 
                    "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        WindowPositionManager.DeleteConfiguration(item.Name);
                        LoadLayouts();
                        _nameTextBox.Text = "";
                        _descriptionTextBox.Text = "";
                        MessageBox.Show($"Layout '{item.Name}' eliminado correctamente.", 
                            "Eliminación Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar layout: {ex.Message}", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor selecciona un layout de la lista para eliminar.", 
                    "Selección Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private class LayoutItem
        {
            public string Name { get; }
            public string DisplayText { get; }

            public LayoutItem(string name, string displayText)
            {
                Name = name;
                DisplayText = displayText;
            }

            public override string ToString() => DisplayText;
        }
    }
}
