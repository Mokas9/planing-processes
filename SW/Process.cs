using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW
{
    public enum ProcessState
    {
        New,
        Ready,
        Running,
        Terminated
    }

    public class Process
    {
        private int _timeOfAppearance;
        private int _timeCPUBurst;

        public ProcessState State { get; set; }
        public int Pos { get; set; }
        public int Priority { get; set; } //Приоритет
        public int TimeOfAppearance
        {
            get
            {
                return _timeOfAppearance;
            }
            set
            {
                _timeOfAppearance = value;
                if (value == 0)
                    State = ProcessState.Ready;
            }
        } //Время появления в очереди
        public int TimeCPUBurst
        {
            get { return _timeCPUBurst; }
            set
            {
                _timeCPUBurst = value;
                if (value == 0)
                    State = ProcessState.Terminated;
            }
        } //Продолжительность очередного CPU burst
        public Process(int Pos, int Priority, int TimeOfAppearance, int TimeCPUBurst)
        {
            State = TimeOfAppearance > 0 ? ProcessState.New : ProcessState.Ready;
            this.Pos = Pos;
            this.Priority = Priority;
            this.TimeOfAppearance = TimeOfAppearance;
            this.TimeCPUBurst = TimeCPUBurst;
        }
        public override string ToString()
        {
            return "P(" + Pos + ")" + ": P_" + Priority + ", T_" + TimeOfAppearance + ", TCPUB_" + TimeCPUBurst;
        }
    }

    public class Characteristic
    {
        public float TotalTimeWait { get; set; }
        public float TotalTimeRun { get; set; }
    }
}
