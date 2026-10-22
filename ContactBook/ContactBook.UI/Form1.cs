using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ContactBook.Core.Interfaces;
using ContactBook.Core.Models;

namespace ContactBook.UI
{
    public partial class Form1 : Form
    {
        private readonly IContactRepository _repository;
        private readonly IContactDataService _dataService;

        private int? _selectedContactId;
        private IReadOnlyList<Group> _groups = Array.Empty<Group>();
        private bool _isFilterUpdating;

        public Form1(IContactRepository repository, IContactDataService dataService)
        {
            InitializeComponent();
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));

            ConfigureGridColumns();
        }

        private void ConfigureGridColumns()
        {
            dgvContacts.AutoGenerateColumns = false;
            dgvContacts.Columns.Clear();

            dgvContacts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ContactViewModel.Id),
                HeaderText = "ID",
                Width = 45,
                Visible = false
            });

            dgvContacts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ContactViewModel.FirstName),
                HeaderText = "Имя",
                FillWeight = 22
            });

            dgvContacts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ContactViewModel.LastName),
                HeaderText = "Фамилия",
                FillWeight = 22
            });

            dgvContacts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ContactViewModel.PhoneNumber),
                HeaderText = "Телефон",
                FillWeight = 24
            });

            dgvContacts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ContactViewModel.Email),
                HeaderText = "Email",
                FillWeight = 22
            });

            dgvContacts.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(ContactViewModel.GroupName),
                HeaderText = "Группа",
                FillWeight = 16
            });
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await LoadGroupsAsync();
            await LoadContactsAsync();
            UpdateActionButtonsState();
        }

        private async Task LoadGroupsAsync()
        {
            try
            {
                _isFilterUpdating = true;
                _groups = await _repository.GetAllGroupsAsync();

                var filterList = new List<GroupItem> { new(0, "[Все группы]") };
                filterList.AddRange(_groups.Select(g => new GroupItem(g.Id, g.Name)));

                cmbFilterGroup.DataSource = filterList;
                cmbFilterGroup.DisplayMember = nameof(GroupItem.Name);
                cmbFilterGroup.ValueMember = nameof(GroupItem.Id);
                cmbFilterGroup.SelectedIndex = 0;

                var editList = new List<GroupItem> { new(0, "[Без группы]") };
                editList.AddRange(_groups.Select(g => new GroupItem(g.Id, g.Name)));

                cmbGroup.DataSource = editList;
                cmbGroup.DisplayMember = nameof(GroupItem.Name);
                cmbGroup.ValueMember = nameof(GroupItem.Id);
                cmbGroup.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке групп: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isFilterUpdating = false;
            }
        }

        private async Task LoadContactsAsync(Func<Task<IReadOnlyList<Contact>>>? loader = null)
        {
            SetLoadingState(true, "Загрузка списка контактов...");
            try
            {
                var contacts = loader != null ? await loader() : await _repository.GetAllAsync();

                var viewModels = contacts.Select(c => new ContactViewModel
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email ?? string.Empty,
                    GroupName = c.Group?.Name ?? (_groups.FirstOrDefault(g => g.Id == c.GroupId)?.Name ?? "—"),
                    GroupId = c.GroupId
                }).ToList();

                dgvContacts.DataSource = viewModels;
                lblStatus.Text = $"Загружено контактов: {viewModels.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке контактов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Ошибка загрузки контактов.";
            }
            finally
            {
                SetLoadingState(false);
                UpdateActionButtonsState();
            }
        }

        private void dgvContacts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvContacts.SelectedRows.Count > 0 &&
                dgvContacts.SelectedRows[0].DataBoundItem is ContactViewModel item)
            {
                _selectedContactId = item.Id;
                txtFirstName.Text = item.FirstName;
                txtLastName.Text = item.LastName;
                txtPhone.Text = item.PhoneNumber;
                txtEmail.Text = item.Email;
                cmbGroup.SelectedValue = item.GroupId ?? 0;
            }
            else
            {
                _selectedContactId = null;
            }

            UpdateActionButtonsState();
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            var newContact = new Contact
            {
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                PhoneNumber = txtPhone.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                GroupId = cmbGroup.SelectedValue is int gId && gId > 0 ? gId : null
            };

            SetLoadingState(true, "Добавление нового контакта...");
            try
            {
                await _repository.AddAsync(newContact);
                ClearForm();
                await LoadContactsAsync();
                MessageBox.Show("Контакт успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении контакта: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private async void btnEdit_Click(object sender, EventArgs e)
        {
            if (_selectedContactId == null)
            {
                MessageBox.Show("Выберите контакт для редактирования в таблице!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidateInput()) return;

            var updatedContact = new Contact
            {
                Id = _selectedContactId.Value,
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                PhoneNumber = txtPhone.Text.Trim(),
                Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
                GroupId = cmbGroup.SelectedValue is int gId && gId > 0 ? gId : null
            };

            SetLoadingState(true, "Сохранение изменений...");
            try
            {
                await _repository.UpdateAsync(updatedContact);
                ClearForm();
                await LoadContactsAsync();
                MessageBox.Show("Контакт успешно обновлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении контакта: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (_selectedContactId == null)
            {
                MessageBox.Show("Выберите строку для удаления в таблице!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show(
                $"Вы уверены, что хотите удалить выбранный контакт (ID: {_selectedContactId.Value})?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes) return;

            SetLoadingState(true, "Удаление контакта...");
            try
            {
                await _repository.DeleteAsync(_selectedContactId.Value);
                ClearForm();
                await LoadContactsAsync();
                MessageBox.Show("Контакт удален.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении контакта: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
            dgvContacts.ClearSelection();
            _selectedContactId = null;
            UpdateActionButtonsState();
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            string query = txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                await LoadContactsAsync();
                return;
            }

            await LoadContactsAsync(() => _repository.SearchAsync(query));
        }

        private async void btnResetSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            _isFilterUpdating = true;
            cmbFilterGroup.SelectedIndex = 0;
            _isFilterUpdating = false;

            await LoadContactsAsync();
        }

        private async void cmbFilterGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isFilterUpdating) return;

            if (cmbFilterGroup.SelectedValue is int groupId && groupId > 0)
            {
                await LoadContactsAsync(() => _repository.GetByGroupAsync(groupId));
            }
            else
            {
                await LoadContactsAsync();
            }
        }

        private async void btnExport_Click(object sender, EventArgs e)
        {
            using var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                FileName = "contacts_export.json"
            };

            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

            SetLoadingState(true, "Экспорт контактов в JSON...");
            try
            {
                var contacts = await _repository.GetAllAsync();
                await _dataService.ExportToJsonAsync(saveFileDialog.FileName, contacts);
                MessageBox.Show($"Успешно экспортировано {contacts.Count} контактов в JSON!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в JSON: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private async void btnExportCsv_Click(object sender, EventArgs e)
        {
            using var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = "contacts_export.csv"
            };

            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

            SetLoadingState(true, "Экспорт контактов в CSV...");
            try
            {
                var contacts = await _repository.GetAllAsync();
                await _dataService.ExportToCsvAsync(saveFileDialog.FileName, contacts);
                MessageBox.Show($"Успешно экспортировано {contacts.Count} контактов в CSV!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в CSV: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private async void btnImport_Click(object sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json"
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            SetLoadingState(true, "Импорт контактов из JSON...");
            try
            {
                var importedContacts = await _dataService.ImportFromJsonAsync(openFileDialog.FileName);
                if (importedContacts.Count == 0)
                {
                    MessageBox.Show("Файл не содержит записей для импорта.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int addedCount = 0;
                foreach (var c in importedContacts)
                {
                    // Создаем новые записи, очищая исходный ID для автоинкремента SQLite
                    c.Id = 0;
                    await _repository.AddAsync(c);
                    addedCount++;
                }

                await LoadContactsAsync();
                MessageBox.Show($"Успешно импортировано контактов: {addedCount}", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте из JSON: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private async void btnImportCsv_Click(object sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK) return;

            SetLoadingState(true, "Импорт контактов из CSV...");
            try
            {
                var importedContacts = await _dataService.ImportFromCsvAsync(openFileDialog.FileName);
                if (importedContacts.Count == 0)
                {
                    MessageBox.Show("Файл не содержит записей для импорта.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int addedCount = 0;
                foreach (var c in importedContacts)
                {
                    c.Id = 0;
                    await _repository.AddAsync(c);
                    addedCount++;
                }

                await LoadContactsAsync();
                MessageBox.Show($"Успешно импортировано контактов: {addedCount}", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте из CSV: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private bool ValidateInput()
        {
            var (isValid, errorMessage) = ContactBook.Core.Validators.ContactInputValidator.Validate(
                txtFirstName.Text,
                txtPhone.Text,
                txtEmail.Text);

            if (!isValid)
            {
                MessageBox.Show(errorMessage, "������ ���������", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            txtFirstName.Clear();
            txtLastName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            if (cmbGroup.Items.Count > 0)
            {
                cmbGroup.SelectedIndex = 0;
            }
            _selectedContactId = null;
            UpdateActionButtonsState();
        }

        private void UpdateActionButtonsState()
        {
            bool hasSelection = _selectedContactId != null;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
        }

        private void SetLoadingState(bool isLoading, string statusMessage = "")
        {
            Cursor = isLoading ? Cursors.WaitCursor : Cursors.Default;
            btnAdd.Enabled = !isLoading;
            btnEdit.Enabled = !isLoading && _selectedContactId != null;
            btnDelete.Enabled = !isLoading && _selectedContactId != null;
            btnClear.Enabled = !isLoading;
            btnSearch.Enabled = !isLoading;
            btnResetSearch.Enabled = !isLoading;
            btnExport.Enabled = !isLoading;
            btnImport.Enabled = !isLoading;
            btnExportCsv.Enabled = !isLoading;
            btnImportCsv.Enabled = !isLoading;

            if (!string.IsNullOrWhiteSpace(statusMessage))
            {
                lblStatus.Text = statusMessage;
            }
        }

        private sealed record GroupItem(int Id, string Name);

        private sealed class ContactViewModel
        {
            public int Id { get; init; }
            public string FirstName { get; init; } = string.Empty;
            public string LastName { get; init; } = string.Empty;
            public string PhoneNumber { get; init; } = string.Empty;
            public string Email { get; init; } = string.Empty;
            public string GroupName { get; init; } = string.Empty;
            public int? GroupId { get; init; }
        }
    }
}


