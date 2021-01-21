using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * Лабораторная №4
 * 
 * Вариант 16
 * Управление памятью: страничная организация, вытеснение FIFO
 *      
 * Данильченко Роман, 9 гр.
 * 
 */

namespace _4.PagingFIFO
{
    public partial class Form1 : Form
    {
        Thread subThread;
        CPU cpu;

        public Form1()
        {
            InitializeComponent();            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DataGridViewColumn frames = new DataGridViewColumn();
            frames.HeaderText = "Frame Number";
            frames.Width = 130;
            frames.CellTemplate = new DataGridViewTextBoxCell();

            DataGridViewColumn pagesInFrames = new DataGridViewColumn();
            pagesInFrames.HeaderText = "ProcessID + Page Number";
            pagesInFrames.Width = 220;
            pagesInFrames.CellTemplate = new DataGridViewTextBoxCell();

            dataGridView1.Columns.Add(frames);
            dataGridView1.Columns.Add(pagesInFrames);

            DataGridViewColumn virtualMemory = new DataGridViewColumn();
            virtualMemory.HeaderText = "Page Numbers in VM";
            virtualMemory.Width = 180;
            virtualMemory.CellTemplate = new DataGridViewTextBoxCell();

            dataGridView2.Columns.Add(virtualMemory);
        }        

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            int sizeOfPhysicalMemory = Convert.ToInt32(numericUpDown1.Value);
            PhysicalMemory physicalMemory = new PhysicalMemory(sizeOfPhysicalMemory);
            physicalMemory.PhysicalMemoryUpdatedEvent += OnPhysicalMemoryUpdated;

            for (int i = 0; i < sizeOfPhysicalMemory; ++i)
            {
                dataGridView1.Rows.Add();
                dataGridView1[0, i].Value = i;
                dataGridView1[1, i].Value = "Free";
            }

            int sizeOfVirtualMemory = Convert.ToInt32(numericUpDown2.Value);
            VirtualMemory virtualMemory = new VirtualMemory(sizeOfVirtualMemory);
            virtualMemory.VirtualMemoryUpdatedEvent += OnVirtualMemoryUpdated;

            for (int i = 0; i < sizeOfVirtualMemory; ++i)
            {
                dataGridView2.Rows.Add();                
                dataGridView2[0, i].Value = "Free";
            }

            MemoryManagementUnit MMU = new MemoryManagementUnit(physicalMemory, virtualMemory);
            MMU.PageTableRequestEvent += OnPageTableRequest;
            MMU.PageTableAnswerEvent += OnPageTableAnswer;
            MMU.PageFaultOccuredEvent += OnPageFaultOccured;
            MMU.PageFaultHandledEvent += OnPageFaultHandled;
            MMU.PageAddedToPhysicalMemoryEvent += OnPageAddedToPhysicalMemory;
            MMU.PageReplacedEvent += OnPageReplaced;
            MMU.ProcessFinishedWorkEvent += OnProcessFinishedWork;

            cpu = new CPU(Convert.ToInt32(numericUpDown3.Value), MMU);

            subThread = new Thread(Start);
            subThread.Start();            
        }

        private void Start()
        {
            Thread t = new Thread(cpu.StartProcesses);

            t.Start();
            t.Join();
        }

        private void OnPhysicalMemoryUpdated(int frameNumber, int processID, int pageNumber)
        {
            if (processID != -1)
            {
                dataGridView1.BeginInvoke(new Action(() => {
                    dataGridView1[1, frameNumber].Value = $"Pr.ID {processID} - Page Num. {pageNumber}";
                }));
            }
            else
            {
                dataGridView1.BeginInvoke(new Action(() => {
                    dataGridView1[1, frameNumber].Value = "Free";
                }));
            }
        }

        private void OnVirtualMemoryUpdated(int pageNumber, int processID, int updatedPageNumber)
        {
            if (processID != -1)
            {
                dataGridView2.BeginInvoke(new Action(() => {
                    dataGridView2[0, pageNumber].Value = $"Pr.ID {processID} - Page Num. {updatedPageNumber}";
                }));
            }
            else
            {
                dataGridView2.BeginInvoke(new Action(() => {
                    dataGridView2[0, pageNumber].Value = "Free";
                }));
            }
        }

        private void OnPageTableRequest(int processID, int pageNumber)
        {
            textBox1.BeginInvoke(new Action(() => {
                textBox1.Text += $"Процесс c ID {processID} запрашивает {pageNumber}-ю страницу" + Environment.NewLine;
            }));            
        }

        private void OnPageTableAnswer(int frameNumber, int processID, int pageNumber)
        {
            textBox1.BeginInvoke(new Action(() => {
                textBox1.Text += $"MMU вернул процессу с ID {processID} адрес его {pageNumber}-й страницы: {frameNumber}" + Environment.NewLine + Environment.NewLine;
            }));
        }

        private void OnPageFaultOccured(int processID, int pageNumber)
        {            
            textBox2.BeginInvoke(new Action(() => {
                textBox2.Text += $"MMU обратился к адресу {pageNumber}-й страницы процесса с ID {processID}, но её не оказалось в оперативной памяти" + Environment.NewLine;
            }));
        }

        private void OnPageFaultHandled(bool replaced, int frameNumber, int processID, int pageNumber)
        {
            string s = replaced 
                ? "Произошло вытеснение, "
                : "В оперативной памяти оказалось свободное место, ";

            textBox2.BeginInvoke(new Action(() => {
                textBox2.Text += $"{s}{processID}-я страница процесса с ID {processID} теперь в оперативной памяти по адресу {frameNumber}" + Environment.NewLine + Environment.NewLine;
            }));
        }

        private void OnPageAddedToPhysicalMemory(int processID, int pageNumber)
        {
            textBox3.BeginInvoke(new Action(() => {
                textBox3.Text += $"{processID} : {pageNumber}" + Environment.NewLine;
            }));
        }

        private void OnPageReplaced(int oldProcessID, int oldPageNumber, int newProcessID, int newPageNumber)
        {
            textBox4.BeginInvoke(new Action(() => {
                textBox4.Text += $"{oldProcessID} : {oldPageNumber} --> {newProcessID} : {newPageNumber}" + Environment.NewLine;
            }));
        }

        private void OnProcessFinishedWork(int finishedProcessID)
        {
            textBox5.BeginInvoke(new Action(() => {
                textBox5.Text += $"Процесс с ID {finishedProcessID} завершился" + Environment.NewLine;
            }));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.SelectionStart = textBox2.Text.Length;
            textBox2.ScrollToCaret();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            textBox5.SelectionStart = textBox5.Text.Length;
            textBox5.ScrollToCaret();
        }
    }
}
