using System;
using System.Windows.Forms;
using ContactBook.Core;
using ContactBook.Data;

namespace ContactBook.UI
{
    public partial class Form1 : Form
    {
        private ContactRepository repository = new ContactRepository();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadContacts();
        }

        private void LoadContacts()
        {
            dgvContacts.DataSource = null;
            dgvContacts.DataSource = repository.GetAllContacts();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Заполните хотя бы Имя и Телефон!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newContact = new Contact
            {
                FirstName = txtFirstName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Category = "Общие"
            };

            repository.AddContact(newContact);
            LoadContacts();

            txtFirstName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            MessageBox.Show("Контакт успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvContacts.SelectedRows.Count > 0)
            {
                int contactId = Convert.ToInt32(dgvContacts.SelectedRows[0].Cells["Id"].Value);

                repository.DeleteContact(contactId);
                LoadContacts();
                MessageBox.Show("Контакт удален.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Выберите строку для удаления в таблице!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string query = txtSearch.Text.Trim();
            dgvContacts.DataSource = repository.SearchContacts(query);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json";
            saveFileDialog.FileName = "contacts_export.json";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                repository.ExportToJson(saveFileDialog.FileName);
                MessageBox.Show("Контакты успешно экспортированы!", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}