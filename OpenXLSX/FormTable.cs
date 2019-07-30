using System;
using System.Data;
using System.Windows.Forms;

namespace OpenXLSX
{
    public partial class FormTable : Form
    {
        public DataTable dataTable = new DataTable();
        public FormTable()
        {
            InitializeComponent();
        }
        private void FormTable_Load(object sender, EventArgs e)
        {
            
            dataGridView1.DataSource = dataTable;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.Columns[0].Width = 150;
            dataGridView1.Columns[1].Width = 200;
            dataGridView1.Columns[2].Width = 100;
            dataGridView1.Columns[3].Width = 500;
            dataGridView1.Columns["SheetID"].HeaderText = "Рабочий лист";
            dataGridView1.Columns["Name"].HeaderText = "Фамилия";
            dataGridView1.Columns["Equipment"].HeaderText = "Наименование";
            dataGridView1.Columns["Repairs"].HeaderText = "Ремонт";
            dataGridView1.Columns["DateInDateTime"].HeaderText = "Дата";
        }

        private void FormTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            dataTable.Clear();
        }
    }
}
