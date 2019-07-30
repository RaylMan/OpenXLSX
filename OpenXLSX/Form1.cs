using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace OpenXLSX
{
    public partial class Form1 : Form
    {
        private ReadXlsx xlsx = new ReadXlsx();
        private List<WorkLine> list = new List<WorkLine>();//список всего листа xlsx
        List<SheetsID> sheetsID = new List<SheetsID>();// список листов файла для checkedListBox
        private List<WorkLine> listSelection = new List<WorkLine>();// список с выборкой по дате
        private List<NamesCounter> firstNames = new List<NamesCounter>();//список фамилий и количество сделанного оборудования на основе listSelection
        private List<ListEquipCount> listEquipCount = new List<ListEquipCount>();// список одинакового оборудования с подсчетов на основе listSelection
        private List<RepairsTable> repairsList = new List<RepairsTable>();// список ремонта по фамилии
        private List<ListEquipCount> sampleNames = new List<ListEquipCount>();//список оборудования по фамилии
        private DateTime firstDate = DateTime.Now.Date;
        private DateTime secondDate = DateTime.Now.Date;
        private DataTable table = new DataTable(); //таблица для вывода в  dataGridView1
        private DataTable formTable = new DataTable(); // Таблица проделанного ремонта
        private string selectedState; // индекс выбранного листа
        private string path; // путь рабочего файла
        private string selectedMenuItem;
        private string selectedMenuItem1;// выделенный элемент lisbox1
        private readonly ContextMenuStrip collectionRoundMenuStrip; //контекстное меню для listbox1


        public Form1()
        {
            InitializeComponent();
            // контекстное меню для listbox1
            var toolStripMenuItem1 = new ToolStripMenuItem { Text = "Проделанная работа" };
            toolStripMenuItem1.Click += toolStripMenuItem1_Click;
            collectionRoundMenuStrip = new ContextMenuStrip();
            collectionRoundMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1 });
            listBox1.MouseDown += ListBox1_MouseDown;
            label3.Text = "Перед загрузкой файла в программу, откройте его в Excel и сохраните без изменений. \nИначе не будут выводиться данные!!!";

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            formTable.Clear();
            repairsList.Clear();
            FormTable ft = new FormTable();
            Clipboard.SetText(selectedMenuItem);
            foreach (var item in listSelection)
            {
                if (item.Name == selectedMenuItem1)
                    repairsList.Add(new RepairsTable
                    {
                        SheetID = item.SheetID,
                        Repairs = item.Repairs,
                        Equipment = item.Equipment,
                        Name = item.Name,
                        DateInDateTime = item.DateInDateTime
                    });
            }
            formTable = ToData.ToDataTable<RepairsTable>(repairsList);
            ft.dataTable = formTable;
            ft.Show();
        }


        private void BtnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Excel Worksheets|*.xlsx";
            file.ShowDialog();
            if (file.FileName != "")
                path = file.FileName;
            //Вывод всех названий и rId листа, для выгрузки в combobox списка листов в файле Excel
            if (path != "" && path != null)
            {
                try
                {
                    using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(path, false))
                    {
                        WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
                        IEnumerable<Sheet> sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                        List<SheetsID> listSheets = new List<SheetsID>();//список всех листов файла 

                        foreach (var sheet in sheets)
                        {
                            listSheets.Add(new SheetsID() { SheetID = sheet.Id, SheetName = sheet.Name });
                        }
                        checkedListBox1.DataSource = listSheets;
                        checkedListBox1.DisplayMember = "SheetName";
                        checkedListBox1.ValueMember = "SheetID";
                    }
                }
                catch (System.IO.IOException)
                {
                    if (path != "")
                        MessageBox.Show("Закройте рабочий файл в Excel!", "Ошибка");
                }
                btnCount.Enabled = true;
            }
        }
        //Поиск и вывод дубликатов в списке, с их подсчетом и выборкой по дате
        private void BtnCount_Click(object sender, EventArgs e)
        {
            if (path != "" && path != null)
            {
                try
                {
                    list.Clear();
                    sheetsID.Clear();
                    listSelection.Clear();
                    listEquipCount.Clear();
                    firstNames = null;
                    firstNames = new List<NamesCounter>();
                    try
                    {
                        foreach (SheetsID item in checkedListBox1.CheckedItems)
                        {
                            sheetsID.Add(new SheetsID {SheetID = item.SheetID, SheetName = item.SheetName });
                            
                        }
                        //Запись колонок ФИО Мастера и Дата в список, для последующего подсчета
                        for (int i = 0; i < sheetsID.Count; i++)
                        {
                            foreach (var rw in xlsx.ReadAsDataTable(path, sheetsID[i].SheetID).AsEnumerable())
                            {
                                list.Add(new WorkLine
                                {
                                    SheetID = sheetsID[i].SheetName,
                                    Repairs = Convert.ToString(rw["Проделанный ремонт"]),
                                    Equipment = Convert.ToString(rw["Наименование оборудования"]),
                                    Name = Convert.ToString(rw["ФИО Мастера"]).Split(' ').First(),
                                    DateInDateTime = xlsx.ConvToDate(Convert.ToString(rw["Дата"]))
                                });
                            }
                        }
                    }
                    catch (System.ArgumentException)
                    {
                        MessageBox.Show("Строка с фамилиями мастеров должна называтся: ФИО Мастера\n" +
                            "Строка с датой ремона: Дата\nИначе не будет рабоать!!!(без пробелов и дополнительных симовлов", "Ошибка");
                    }


                    if (list.Count != 0)
                    {
                        // выборка в спосок по дате
                        listSelection = (from workLine in list
                                         where workLine.DateInDateTime >= firstDate && workLine.DateInDateTime <= secondDate
                                         select new WorkLine
                                         {
                                             SheetID = workLine.SheetID,
                                             Repairs = workLine.Repairs,
                                             Equipment = workLine.Equipment,
                                             Name = workLine.Name,
                                             DateInDateTime = workLine.DateInDateTime
                                         }).ToList();
                        //Поиск и вывод дубликатов в списке, с их подсчетом и выборкой по дате
                        var duplicateFirstName = from name in list
                                                 where name.DateInDateTime >= firstDate && name.DateInDateTime <= secondDate
                                                 group name by name.Name into g
                                                 select new { Name = g.Key, Count = g.Count() };
                        foreach (var group in duplicateFirstName)
                        {
                            firstNames.Add(new NamesCounter { Name = group.Name, NameWithCount = group.Name + " " + group.Count, Count = group.Count });
                        }
                        //выборка и подсчет одинакового оборудования
                        var duplicateEquip = from equip in listSelection
                                             group equip by new
                                             {
                                                 equip.Name,
                                                 equip.Equipment
                                             } into g
                                             select new { g.Key.Name, g.Key.Equipment, Count = g.Count() };

                        foreach (var group in duplicateEquip)
                        {
                            listEquipCount.Add(new ListEquipCount { Name = group.Name, Equipment = group.Equipment, Count = group.Count });
                        }
                        listBox1.DataSource = firstNames;
                        listBox1.DisplayMember = "NameWithCount";
                        listBox1.ValueMember = "Name";


                    }
                }
                catch (System.IO.IOException)
                {
                    if (path != "")
                        MessageBox.Show("Закройте рабочий файл в Excel!", "Ошибка");
                }
            }
            else
                MessageBox.Show("Загрузите файл!", "Ошибка");
        }

        // вывод в таблицу по фамилии
        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView1.AutoResizeColumns();
            dataGridView1.DataSource = null;
            sampleNames.Clear();
            table.Clear();

            foreach (var item in listEquipCount)
            {
                if (item.Name == listBox1.SelectedValue.ToString())
                    sampleNames.Add(new ListEquipCount { Name = item.Name, Equipment = item.Equipment, Count = item.Count });
            }


            table = ToData.ToDataTable<ListEquipCount>(sampleNames);
            dataGridView1.AutoResizeColumns();

            dataGridView1.DataSource = table;
            dataGridView1.AllowUserToAddRows = false;//DataGridViewButtonColumn
            dataGridView1.Columns[0].Width = 200;
            dataGridView1.Columns[1].Width = 100;
            dataGridView1.Columns[2].Width = 50;
            dataGridView1.Columns["Equipment"].HeaderText = "Наименование";
            dataGridView1.Columns["Name"].HeaderText = "Фамилия";
            dataGridView1.Columns["Count"].HeaderText = "Кол-во";
        }

        private void DateTimePicker1_ValueChanged_1(object sender, EventArgs e)
        {
            firstDate = dateTimePicker1.Value.Date;
        }

        private void DateTimePicker2_ValueChanged_1(object sender, EventArgs e)
        {
            secondDate = dateTimePicker2.Value.Date.AddHours(20);
        }

        private void ListBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var index = listBox1.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                selectedMenuItem = listBox1.Items[index].ToString();
                selectedMenuItem1 = listBox1.Text.Split(' ').First();
                collectionRoundMenuStrip.Show(Cursor.Position);
                collectionRoundMenuStrip.Visible = true;
            }
            else
            {
                collectionRoundMenuStrip.Visible = false;
            }
        }
    }
}