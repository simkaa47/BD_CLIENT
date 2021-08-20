using BD_CLIENT.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BD_CLIENT.Models
{
    class Model
    {
        bool processing;
        public event ClientEventHandler Port5000AnswerEvent;
        public Action<int> ReceiveDetHandler;
        
        public ClientBase Client5000 { get; set; }
        public ClientBase Client5001 { get; set; }

        #region IP адрес платы
        string _ip="192.168.1.177";
        /// <summary>
        /// IP адрес платы
        /// </summary>
        public string IP
        {
            get => _ip;
            set
            {
                if (CheckIp(value))_ip = value;
            }
        }
        #endregion

        #region Ответ от порта 5000
        /// <summary>
        /// Ответ от порта 5000
        /// </summary>
        public string Port5000Answer { get; set; }
        #endregion

        #region Данные детекторов
        public DataPoint[] Detectors { get; set; } = new DataPoint[1000];
        #endregion

        #region Количество сенсоров
        int numSens = 32;
        /// <summary>
        /// Количество сенсоров
        /// </summary>
        public int NumSens
        {
            get => numSens;
            set
            {
                if (value >= 32 && value % 4 == 0 && value<988) numSens = value;
            }
        }
        #endregion

        #region Делитель усиления
        int divGain = 1;
        public int DivGain
        {
            get => divGain;
            set
            {
                if (value >= 1 && value <= 7) divGain = value;
            }
        }
        #endregion

        #region Задержка экспозиции
        int expDelay = 10;
        public int ExpDelay
        {
            get => expDelay;
            set { if (value >= 10 && value <= 100) expDelay = value; }
        }
        #endregion
        public async  void ConnectDisconnect5000()
        {
            if (!Client5000.Connected) await Task.Run(() => { Client5000.Connect(5000,IP); });
            else Client5000.Disconnect();
        }

        public async void ConnectDisconnect5001()
        {
            if (!Client5001.Connected) await Task.Run(() => { Client5001.Connect(5001,IP); });
            else Client5001.Disconnect();
        }

        public void StopAll()
        {
            Client5000.Disconnect();
            Client5001.Disconnect();
        }
        #region Записать делитель усиления
        public void SetDivGain()
        {
            if (Client5000.Connected)
            {
                Client5000.Write(GetCommand("$1r" + DivGain.ToString()));
            }
        }
        #endregion

        #region Записать количество детекторов
        public void SetNumSens()
        {
            if (Client5000.Connected)
            {
                Client5000.Write(GetCommand("$1s" + NumSens.ToString()));
            }
        }
        #endregion

        #region Записать задержку экспозиции
        public void SetExpDelay()
        {
            if (Client5000.Connected)
            {
                Client5000.Write(GetCommand("$1d" + ExpDelay.ToString()));
            }
        }
        #endregion

        #region Метод проверки корректности ip
        /// <summary>
        /// Проверка корректности ip
        /// </summary>
        /// <param name="ip">Проверяемая строка</param>
        /// <returns>true, если ip корректно</returns>
        bool CheckIp(string ip)
        {
            var arrStr = ip.Split(".");
            int temp = 0;
            if (arrStr.Length != 4) return false;
            foreach (var item in arrStr)
            {
                if (!int.TryParse(item, out temp)) return false;
                if ((temp < 0) || (temp > 255)) return false;
            }
            return true;
        }
        #endregion

        #region Старт непрервыного сканирования
        public void StartConstScan()
        {
            if (Client5000.Connected)
            {
                Client5000.Write(GetCommand("$cc"));
            }
        }
        #endregion

        #region Стоп непрервыного сканирования
        public void StopConstScan()
        {
            if (Client5000.Connected)
            {
                Client5000.Write(GetCommand("@"));
            }
        }
        #endregion

        #region Стоп непрервыного сканирования
        public void SingleScan()
        {
            if (Client5000.Connected)
            {
                Client5000.Write(GetCommand("$cs"));
            }
        }
        #endregion

        #region Получить ответ от порта 5000
        void GetAnswer(int bytesNum)
        {
            Port5000Answer = Encoding.ASCII.GetString(Client5000.InBuf, 0, bytesNum);
            Port5000AnswerEvent?.Invoke(Port5000Answer);        
        }
        #endregion

        #region Получить ответ от порта 5001
         void   GetDetectors(int numBytes)
        {
            if (!processing)
            {
                
                    processing = true;
                    bool markOk = true;
                    for (int i = 0; i < 4; i++) markOk = markOk && Client5001.InBuf[i] == 0;
                    for (int i = 4; i < 8; i++) markOk = markOk && Client5001.InBuf[i] == 255;
                    if (markOk)
                    {
                        int i = 0;
                        var bytes = new byte[2];                        
                        for ( i = 8; i < Client5001.InBuf.Length; i += 2)
                        {
                            if (Client5001.InBuf[i] == 0)
                            {
                                if (i != 72)
                                {
                                    Thread.Sleep(1);
                                }
                                break;
                            }
                            bytes[0] = Client5001.InBuf[i + 1];
                            bytes[1] = Client5001.InBuf[i];
                            var dp = new DataPoint { x = (i - 8) / 2, y = BitConverter.ToUInt16(bytes, 0) };
                            Detectors[(i-8)/2] = dp;
                        }
                        ReceiveDetHandler?.Invoke(NumSens);
                    }
                    processing = false;
                                        
            }
        }
        #endregion

        byte[] GetCommand(string str)
        {
            string cmd = str + "\r\n";
            return Encoding.ASCII.GetBytes(cmd);
        }

        #region Конструктор
        public Model()
        {
            Client5000 = new ClientBase();
            Client5001 = new ClientBase();
            Client5000.RecognizeInputBytes = GetAnswer;
            Client5001.RecognizeInputBytes = GetDetectors;
        } 
        #endregion
    }
}
