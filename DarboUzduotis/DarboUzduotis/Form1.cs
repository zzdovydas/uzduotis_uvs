using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Data.OleDb;

namespace DarboUzduotis
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        class ColumnsData
        {
            public string columnName;
            public int columnWidth;

            public ColumnsData(string name, int width)
            {
                columnName = name;
                columnWidth = width;
            }
        }

        class RowData
        {
            public int rowThreadId;
            public string rowData;

            public RowData(int threadId, string data)
            {
                rowThreadId = threadId;
                rowData = data;
            }

        }

        List<Thread> threadList;

        bool threadActive = true;

        List<RowData> rowList;

        public const int rowLimit = 20;
        OleDbConnection conn = new OleDbConnection();

        private List<ColumnsData> CreateColumns()
        {
            List<ColumnsData> cDList = new List<ColumnsData>();

            ColumnsData cD = new ColumnsData("Thread ID", 100);
            cDList.Add(cD);

            cD = new ColumnsData("Generated data", 200);
            cDList.Add(cD);

            return cDList;
        }

        private void DatabaseConnect()
        {
            conn.ConnectionString = @"Provider = Microsoft.JET.OLEDB.4.0;Data Source=db.mdb;Persist Security Info=False;";
            conn.Open();
        }

        private void DatabaseInsertRow(int threadID, string data)
        {
                var date = DateTime.Now;
                string query = @"INSERT INTO Lentelė1 (ThreadID, TimeGenerated, GeneratedData) VALUES (?, ?, ?);";
                OleDbCommand command = new OleDbCommand();
                command.Connection = conn;
                command.CommandText = query;
                command.Parameters.AddWithValue("@ThreadID", threadID);
                command.Parameters.AddWithValue("@TimeGenerated", string.Format("{0}:{1}:{2}", date.Hour, date.Minute, date.Second));
                command.Parameters.AddWithValue("@GeneratedData", data);
                command.ExecuteNonQuery();
        }

        private void InitializeListView()
        {
            listView.View = View.Details;

            List<ColumnsData> columnList = CreateColumns();

            for (int i = 0; i < columnList.Count; i++)
                listView.Columns.Add(columnList[i].columnName, columnList[i].columnWidth);

        }

        private void AddListViewItem(ListViewItem item) => listView.Items.Add(item);

        private void InsertItem(int limit, int threadId, string data)
        {
            RowData rD = new RowData(threadId, data);

            rowList.Add(rD);

            int elimination = rowList.Count - limit;

            if (elimination > 0)
            {
                for (int i = 0; i < elimination; i++)
                    rowList.RemoveAt(i);
            }

        }

        private void UpdateListView()
        {

            listView.Items.Clear();

            for (int i = 0; i < rowList.Count; i++)
            {
                string[] row = new string[] { rowList[i].rowThreadId.ToString(), rowList[i].rowData };
                ListViewItem item = new ListViewItem(row);
                listView.Items.Add(item);
            }
        }

        private void GenerateThreads(int threadSize, List<Thread> threadList)
        {

            threadList = new List<Thread>();

            for (int i = 0; i < threadSize; i++)
            {

                Thread thrd = new Thread(new ThreadStart(() =>
                {
                    int threadID = i;

                    Random delayRnd = new Random();

                    int delay = delayRnd.Next(500, 2000);

                    Random lineLengthRnd = new Random();
                    Random symbolRnd = new Random();

                    while (threadActive)
                    {

                        Thread.Sleep(delay);

                        int lineLength = lineLengthRnd.Next(5, 10);
                        string line = "";

                        for (int j = 0; j < lineLength; j++)
                        {
                            int symbolIndex = symbolRnd.Next(41, 126);

                            line += (char)symbolIndex;
                        }

                        if (threadActive)
                        {
                            Invoke(new Action(() =>
                            {
                                InsertItem(rowLimit, threadID, line);
                                UpdateListView();
                                DatabaseInsertRow(threadID, line);
                            }));
                        }
                    }

                }));

                threadList.Add(thrd);
                Thread.Sleep(50);
                threadList[i].Start();
            }

        }

        private void TerminateThreads()
        {

            threadActive = false;

            Thread.Sleep(50);

            for (int i = 0; i < threadList.Count; i++)
                threadList[i].Abort();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            threadList = new List<Thread>();
            rowList = new List<RowData>();

            InitializeListView();
            DatabaseConnect();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            int threadNumber = int.Parse(textBox1.Text);
            GenerateThreads(threadNumber, threadList);
            stopButton.Enabled = true;
            startButton.Enabled = false;
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            TerminateThreads();

            conn.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int number = int.Parse(textBox1.Text);

                if (number >= 2 && number <= 15)
                    startButton.Enabled = true;
                else
                    startButton.Enabled = false;
            }
            catch
            {
                startButton.Enabled = false;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            TerminateThreads();
        }
    }
}
