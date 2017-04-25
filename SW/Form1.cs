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

namespace SW
{
    public enum SchedulingAlgorithmType
    {
        FCFS,
        RR,
        SJF,
        PriorityScheduling
    }

    public enum SchedulingType
    {
        Preemptive,  //вытесняющий
        Nonpreemptive  //невытесняющий
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Processes = new List<Process>();
        }
        public int TimeWork = 0;
        public SchedulingType SchedulingType { get; set; }
        public SchedulingAlgorithmType AlgorithmType { get; set; }  //тип алгоритма
        public List<ProcessState[]> ListProcessesStates { get; set; } //sList процессов
        public bool UseKvant { get; set; }  //использование кванта
        public int Kvant { get; set; }  //Значение кванта
        public int CurrentKvant { get; set; }  //Текущее значение кванта
        public List<Process> Processes { get; set; }  //Все процессы
        public List<Process> TempProcesses { get; set; }
        public List<Process> TempWaitAppearance { get; set; }  //Список процессов ожиданий появления
        public List<Process> TempRunning { get; set; }  //Готовые процессы
        public List<Process> AddingProcesses { get; set; }  //добавление процессов в конец список
        public Process CurrentProcessRun { get; set; }  //Использующий процесс
        public Characteristic SaveCharacteristic { get; set; } //сохранённая характеристика

        private List<Process> GetTempProcesses(List<Process> processes)
        {
            List<Process> processestemp = new List<Process>();

            for (int i = 0; i < processes.Count; i++)
                processestemp.Add(new Process(processes[i].Pos, processes[i].Priority, processes[i].TimeOfAppearance, processes[i].TimeCPUBurst));
            return processestemp;
        }

        public void SetValues()
        {
            ListProcessesStates = new List<ProcessState[]>();
            TempWaitAppearance = new List<Process>();
            TempRunning = new List<Process>();
            AddingProcesses = new List<Process>();
            TempProcesses = GetTempProcesses(Processes);
            CurrentKvant = 0;

            foreach(Process p in TempProcesses)
            {
                if (p.State == ProcessState.New)
                    TempWaitAppearance.Add(p);
                else if (p.State == ProcessState.Ready)
                    TempRunning.Add(p);
            }
        }

        public void Algorithm()
        {
            switch(AlgorithmType)
            {
                case SchedulingAlgorithmType.FCFS:
                    if (AddingProcesses.Count != 0) //если есть новые появляющие процессы, то добавляем их в список
                    {
                        for (int i = 0; i < AddingProcesses.Count; i++)
                            TempRunning.Add(AddingProcesses[i]);
                        AddingProcesses.Clear();
                    }

                    if (CurrentProcessRun == null)
                    {
                        if (TempRunning.Count != 0)
                        {
                            CurrentProcessRun = TempRunning[0];
                            TempRunning.RemoveAt(0);
                            CurrentProcessRun.State = ProcessState.Running;
                        }
                    }
                    break;

                case SchedulingAlgorithmType.RR:
                    if (AddingProcesses.Count != 0) //если есть новые появляющие процессы, то добавляем их в список
                    {
                        for (int i = 0; i < AddingProcesses.Count; i++)
                            TempRunning.Add(AddingProcesses[i]);
                        AddingProcesses.Clear();
                    }
                    
                    if (UseKvant)
                    {
                        if (CurrentKvant == Kvant)
                        {
                            if (CurrentProcessRun != null)
                            {
                                CurrentProcessRun.State = ProcessState.Ready;
                                TempRunning.Add(CurrentProcessRun);
                                CurrentProcessRun = null;
                            }
                            CurrentKvant = 0;
                        }
                    }

                    if (CurrentProcessRun == null)
                    {
                        if (TempRunning.Count != 0)
                        {
                            CurrentProcessRun = TempRunning[0];
                            TempRunning.RemoveAt(0);
                            CurrentProcessRun.State = ProcessState.Running;
                        }
                    }
                    break;
                case SchedulingAlgorithmType.SJF:
                    switch(SchedulingType)
                    {
                        case SchedulingType.Nonpreemptive:
                            if (AddingProcesses.Count != 0) //если есть новые появляющие процессы, то добавляем их в список
                            {
                                for (int i = 0; i < AddingProcesses.Count; i++)
                                    TempRunning.Add(AddingProcesses[i]);
                                AddingProcesses.Clear();
                            }

                            if (CurrentProcessRun == null)
                            {
                                if (TempRunning.Count != 0)
                                {
                                    int index = 0;
                                    for(int i = 1; i < TempRunning.Count; i++)
                                    {
                                        if (TempRunning[index].TimeCPUBurst > TempRunning[i].TimeCPUBurst)
                                            index = i;
                                    }
                                    CurrentProcessRun = TempRunning[index];
                                    TempRunning.RemoveAt(index);
                                    CurrentProcessRun.State = ProcessState.Running;
                                }
                            }
                            break;
                        case SchedulingType.Preemptive:
                            if (AddingProcesses.Count != 0) //если есть новые появляющие процессы, то добавляем их в список
                            {
                                Process processmintime = AddingProcesses[0];
                                for (int i = 1; i < AddingProcesses.Count; i++)
                                {
                                    if (AddingProcesses[i - 1].TimeCPUBurst > AddingProcesses[i].TimeCPUBurst)
                                        processmintime = AddingProcesses[i];
                                }

                                for (int i = 0; i < AddingProcesses.Count; i++)
                                {
                                    TempRunning.Add(AddingProcesses[i]);
                                }

                                AddingProcesses.Clear();

                                if (CurrentProcessRun != null)
                                {
                                    if (CurrentProcessRun.TimeCPUBurst > processmintime.TimeCPUBurst)
                                    {
                                        TempRunning.Remove(processmintime);
                                        CurrentProcessRun.State = ProcessState.Ready;
                                        TempRunning.Add(CurrentProcessRun);
                                        CurrentProcessRun = processmintime;
                                        CurrentProcessRun.State = ProcessState.Running;
                                    }
                                }
                            }

                            if (CurrentProcessRun == null)
                            {
                                if (TempRunning.Count != 0)
                                {
                                    int index = 0;
                                    for (int i = 1; i < TempRunning.Count; i++)
                                    {
                                        if (TempRunning[index].TimeCPUBurst > TempRunning[i].TimeCPUBurst)
                                            index = i;
                                    }
                                    CurrentProcessRun = TempRunning[index];
                                    TempRunning.RemoveAt(index);
                                    CurrentProcessRun.State = ProcessState.Running;
                                }
                            }
                            break;
                    }
                    break;

                case SchedulingAlgorithmType.PriorityScheduling:
                    switch (SchedulingType)
                    {
                        case SchedulingType.Nonpreemptive:
                            if (AddingProcesses.Count != 0) //если есть новые появляющие процессы, то добавляем их в список
                            {
                                for (int i = 0; i < AddingProcesses.Count; i++)
                                    TempRunning.Add(AddingProcesses[i]);
                                AddingProcesses.Clear();
                            }

                            if (CurrentProcessRun == null)
                            {
                                if (TempRunning.Count != 0)
                                {
                                    int index = 0;
                                    for (int i = 1; i < TempRunning.Count; i++)
                                    {
                                        if (TempRunning[index].Priority > TempRunning[i].Priority)
                                            index = i;
                                    }
                                    CurrentProcessRun = TempRunning[index];
                                    TempRunning.RemoveAt(index);
                                    CurrentProcessRun.State = ProcessState.Running;
                                }
                            }
                            break;
                        case SchedulingType.Preemptive:
                            if (AddingProcesses.Count != 0) //если есть новые появляющие процессы, то добавляем их в список
                            {
                                Process processmintime = AddingProcesses[0];
                                for (int i = 1; i < AddingProcesses.Count; i++)
                                {
                                    if (AddingProcesses[i - 1].Priority > AddingProcesses[i].Priority)
                                        processmintime = AddingProcesses[i];
                                }

                                for (int i = 0; i < AddingProcesses.Count; i++)
                                {
                                    TempRunning.Add(AddingProcesses[i]);
                                }

                                AddingProcesses.Clear();

                                if (CurrentProcessRun != null)
                                {
                                    if (CurrentProcessRun.Priority > processmintime.Priority)
                                    {
                                        TempRunning.Remove(processmintime);
                                        CurrentProcessRun.State = ProcessState.Ready;
                                        TempRunning.Add(CurrentProcessRun);
                                        CurrentProcessRun = processmintime;
                                        CurrentProcessRun.State = ProcessState.Running;
                                    }
                                }
                            }

                            if (CurrentProcessRun == null)
                            {
                                if (TempRunning.Count != 0)
                                {
                                    int index = 0;
                                    for (int i = 1; i < TempRunning.Count; i++)
                                    {
                                        if (TempRunning[index].Priority > TempRunning[i].Priority)
                                            index = i;
                                    }
                                    CurrentProcessRun = TempRunning[index];
                                    TempRunning.RemoveAt(index);
                                    CurrentProcessRun.State = ProcessState.Running;
                                }
                            }
                            break;
                    }
                    break;
            } 
        }

        public void Run()
        {
            SetValues();

            TimeWork = 0;

            //for (int currenttime = 0; currenttime < time; currenttime++)
            while(AddingProcesses.Count != 0 || TempWaitAppearance.Count != 0 || TempRunning.Count != 0 || CurrentProcessRun != null)
            {
                Algorithm();
                ListProcessesStates.Add(new ProcessState[Processes.Count]);
                for (int i = 0; i < Processes.Count; i++)
                    ListProcessesStates[TimeWork][i] = TempProcesses[i].State;

                if (CurrentProcessRun != null)
                {
                    --CurrentProcessRun.TimeCPUBurst;
                    if (CurrentProcessRun.State == ProcessState.Terminated)
                    {
                        CurrentProcessRun = null;
                    }
                }

                if (UseKvant)
                {
                    if (CurrentProcessRun == null)
                        CurrentKvant = 0;
                    else
                        ++CurrentKvant;
                }

                if (TempWaitAppearance.Count != 0)
                    for (int i = 0; i < TempWaitAppearance.Count; i++)
                    {
                        --TempWaitAppearance[i].TimeOfAppearance;
                        if (TempWaitAppearance[i].State == ProcessState.Ready)
                        {
                            AddingProcesses.Add(TempWaitAppearance[i]);
                            TempWaitAppearance.Remove(TempWaitAppearance[i]);
                            --i;
                        }
                    }

                ++TimeWork;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (SetAlgoritm()) //если будет ошибка, то прерываем выполнение
                return;
            /*
            Processes.Clear();
            Processes.Add(new Process(0, 0, 20, 5));
            Processes.Add(new Process(0, 0, 6, 7));
            Processes.Add(new Process(0, 0, 0, 2));
            Processes.Add(new Process(0, 0, 1, 5));
            Processes.Add(new Process(0, 0, 0, 5));
            */
            Run();

            Characteristic c = CalculateCharacteristics();
            label12.Text = c.TotalTimeWait.ToString();
            label13.Text = c.TotalTimeRun.ToString();
            panel1.Invalidate();
        }

        private bool SetAlgoritm()
        {
            if (radioButton1.Checked)
                AlgorithmType = SchedulingAlgorithmType.FCFS;
            else if (radioButton2.Checked)
            {
                AlgorithmType = SchedulingAlgorithmType.RR;
                UseKvant = true;
                try
                {
                    int tempkvant = int.Parse(textBox4.Text);
                    if (tempkvant < 1)
                        throw new Exception("Неправильное значение кванта!");
                    Kvant = tempkvant;
                }
                catch
                {
                    return true;
                }
            }
            else if (radioButton3.Checked)
            {
                AlgorithmType = SchedulingAlgorithmType.SJF;
                if (trackBar1.Value == 0)
                    SchedulingType = SchedulingType.Preemptive;
                else
                    SchedulingType = SchedulingType.Nonpreemptive;
            }
            else
            {
                AlgorithmType = SchedulingAlgorithmType.PriorityScheduling;
                if (trackBar1.Value == 0)
                    SchedulingType = SchedulingType.Preemptive;
                else
                    SchedulingType = SchedulingType.Nonpreemptive;
            }

            return false;
        }

        private Characteristic CalculateCharacteristics()
        {
            Characteristic c = new Characteristic();

            for (int i = 0; i < ListProcessesStates.Count; i++)
                for (int j = 0; j < ListProcessesStates[i].Length; j++)
                {
                    if (ListProcessesStates[i][j] == ProcessState.Ready)
                        ++c.TotalTimeWait;
                    if (ListProcessesStates[i][j] == ProcessState.Ready || ListProcessesStates[i][j] == ProcessState.Running)
                        ++c.TotalTimeRun;
                }

            c.TotalTimeWait /= TempProcesses.Count;
            c.TotalTimeRun /= TempProcesses.Count;
            return c;
        }

        private void DrawStates(Graphics g)
        {
            for (int i = 0; i < ListProcessesStates.Count; i++)
            {
                for (int j = 0; j < ListProcessesStates[i].Length; j++)
                    g.FillRectangle((ListProcessesStates[i][j] == ProcessState.New) || (ListProcessesStates[i][j] == ProcessState.Terminated) ? Brushes.Gray : ListProcessesStates[i][j] == ProcessState.Ready ? Brushes.Yellow : ListProcessesStates[i][j] == ProcessState.Running ? Brushes.Green : Brushes.Gray, new RectangleF(i * panel1.Width / TimeWork, j * panel1.Height / Processes.Count, panel1.Width / TimeWork + 1, panel1.Height / Processes.Count + 1));
                Thread.Sleep(100);
            }

            if (checkBox1.Checked)
                for (int i = 0; i < Processes.Count; i++)
                    g.DrawLine(Pens.Black, new PointF(0, i * panel1.Height / Processes.Count), new PointF(panel1.Width, i * panel1.Height / Processes.Count));
            if (checkBox2.Checked)
                for (int i = 0; i < TimeWork; i++)
                    g.DrawLine(Pens.Black, new PointF(i * panel1.Width / TimeWork, 0), new PointF(i * panel1.Width / TimeWork, panel1.Height));
        }
        
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (ListProcessesStates == null)
                return;
            DrawStates(e.Graphics);
        }

        private void CreateProcess()
        {
            int priority;
            int timeOfAppearance;
            int timeCPUBurst;

            try
            {
                priority = int.Parse(textBox1.Text);
                timeOfAppearance = int.Parse(textBox2.Text);
                timeCPUBurst = int.Parse(textBox3.Text);
                Processes.Add(new Process(Processes.Count + 1, priority, timeOfAppearance, timeCPUBurst));
                UpdateList(listBox1, Processes);
            }
            catch
            {
                MessageBox.Show("Ошибка!");
                return;
            }
        }

        private void UpdateList(ListBox listbox, List<Process> processes)
        {
            listbox.Items.Clear();
            foreach (Process p in processes)
                listbox.Items.Add(p.ToString());
        }

        private void DeleteProcess(int index)
        {
            if (index == -1)
                return;

            int pos = Processes[index].Pos;
            Processes.RemoveAt(index);

            foreach (Process p in Processes)
                if (p.Pos - 1 >= pos)
                    p.Pos -= 1;

            UpdateList(listBox1, Processes);

            listBox1.SelectedIndex = index > listBox1.Items.Count - 1 ? index - 1 : index;
        }

        private void MoveProcessUp(int index)
        {
            if (index == -1 || index == 0)
                return;

            Process p = Processes[index];
            Processes.RemoveAt(index);
            Processes.Insert(index - 1, p);
            UpdateList(listBox1, Processes);
            listBox1.SelectedIndex = index - 1;
        }

        private void MoveProcessDown(int index)
        {
            if (index == listBox1.Items.Count - 1)
                return;

            Process p = Processes[index];
            Processes.RemoveAt(index);
            Processes.Insert(index + 1, p);
            UpdateList(listBox1, Processes);
            listBox1.SelectedIndex = index + 1;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MoveProcessUp(listBox1.SelectedIndex);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MoveProcessDown(listBox1.SelectedIndex);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DeleteProcess(listBox1.SelectedIndex);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CreateProcess();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == radioButton1)
            {
                label4.Visible = false;
                textBox4.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
                trackBar1.Visible = false;
            }
            else if (sender == radioButton2)
            {
                label4.Visible = true;
                textBox4.Visible = true;
                label5.Visible = false;
                label6.Visible = false;
                trackBar1.Visible = false;
            }
            else if (sender == radioButton3)
            {
                label4.Visible = false;
                textBox4.Visible = false;
                label5.Visible = true;
                label6.Visible = true;
                trackBar1.Visible = true;
            }
            else if (sender == radioButton4)
            {
                label4.Visible = false;
                textBox4.Visible = false;
                label5.Visible = true;
                label6.Visible = true;
                trackBar1.Visible = true;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            panel1.Size = new Size(this.Width - 195, this.Height - 222);
            if (ListProcessesStates != null)
                panel1.Invalidate();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Invalidate();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Characteristic c = CalculateCharacteristics();

            label14.Text = c.TotalTimeWait.ToString();
            label15.Text = c.TotalTimeRun.ToString();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Processes.Clear();
            Random rand = new Random();
            if (radioButton1.Checked)
                for (int i = 0; i < 6; i++)
                    Processes.Add(new Process(i, 0, 0, rand.Next(1,12)));
            else if (radioButton2.Checked)
            {
                for (int i = 0; i < 6; i++)
                    Processes.Add(new Process(i, 0, 0, rand.Next(1, 12)));
            }
            else if (radioButton3.Checked)
            {
                if (trackBar1.Value == 0)
                    for (int i = 0; i < 6; i++)
                        Processes.Add(new Process(i, 0, 0, rand.Next(1, 12)));
                else
                    for (int i = 0; i < 6; i++)
                        Processes.Add(new Process(i, 0, rand.Next(1,5), rand.Next(1, 12)));
            }
            else
            {
                if (trackBar1.Value == 0)
                    for (int i = 0; i < 6; i++)
                        Processes.Add(new Process(i, rand.Next(0, 4), rand.Next(1, 5), rand.Next(1, 12)));
                else
                    for (int i = 0; i < 6; i++)
                        Processes.Add(new Process(i, rand.Next(0, 4), rand.Next(1, 5), rand.Next(1, 12)));
            }

            UpdateList(listBox1, Processes);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Processes.Clear();
            listBox1.Items.Clear();
        }
    }
}
