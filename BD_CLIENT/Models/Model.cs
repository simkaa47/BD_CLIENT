using BD_CLIENT.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public event Action<string> LogEvent;
        public ClientBase Client5000 { get; set; }
        public ClientBase Client5001 { get; set; }

        #region IP адрес платы
        string _ip = "192.168.1.177";
        /// <summary>
        /// IP адрес платы
        /// </summary>
        public string IP
        {
            get => _ip;
            set
            {
                if (CheckIp(value)) _ip = value;
            }
        }
        #endregion

        #region Ответ от порта 5000
        /// <summary>
        /// Ответ от порта 5000
        /// </summary>
        public string Port5000Answer { get; set; }
        #endregion

        #region Прошло времени между пакетами от БД
        /// <summary>
        /// Прошло времени между пакетами от БД
        /// </summary>
        public double ElapsedTime5001 { get; private set; }
        DateTime lastTime;
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
                if (value >= 32 && value % 4 == 0 && value < 988) numSens = value;
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

        #region Время экспозиции
        int _expTime = 20000;
        public int ExpTime
        {
            get => _expTime;
            set { if (value >= 500 && value <= 1000000) _expTime = value; }
        }
        #endregion

        #region Флаг текущего логирования
        public bool IsLoging { get; private set; }
        #endregion

        #region Период логирования данных
        int _logPeriod = 100;
        public int LogPeriod
        {
            get => _logPeriod;
            set { if (value >= 10) _logPeriod = value; }
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

        #region Записать время экспозиции
        public void SetExpTime()
        {
            if (Client5000.Connected)
            {
                Client5000.Write(GetCommand("$1e" + ExpTime.ToString()));
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
                        ElapsedTime5001 = (DateTime.Now - lastTime).TotalMilliseconds;
                        lastTime = DateTime.Now;
                        ReceiveDetHandler?.Invoke(NumSens);
                    }
                    processing = false;
                                        
            }
        }
        #endregion

        #region Получить статус системы
        public void GetSysSytatus()
        {
            if (Client5000.Connected)
            {
                Client5000.Write(GetCommand("$sa"));
            }
        }
        #endregion

        #region Логирование данных - старт
        async void StartLog(string path)
        {
            if (Client5001.Connected)
            {
                if (!File.Exists(path))
                {
                    LogEvent?.Invoke($"Файл {path} не найден!");
                    return;
                }
                await Task.Run(() =>
                {
                    var watch = new Stopwatch();
                    try
                    {                        
                        long elapsed = 0;
                        var strbldr = new StringBuilder();
                        IsLoging = true;
                        LogEvent?.Invoke("Старт логирования данных детекторов");                        
                        watch.Start();
                        while (IsLoging)
                        {
                            if (watch.ElapsedMilliseconds > LogPeriod)
                            {
                                GetDetStr(strbldr);
                                elapsed += watch.ElapsedMilliseconds;
                                watch.Restart();
                                if (elapsed > 2000)
                                {
                                    WriteLog(path, strbldr.ToString());
                                    elapsed = 0;
                                    strbldr.Clear();
                                }
                            }
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        IsLoging = false;
                        LogEvent?.Invoke(ex.Message);
                    }
                    finally
                    {
                        LogEvent?.Invoke("Стоп логирования данных детекторов");
                        watch.Stop();
                    }
                });
            }
        }
        #endregion

        #region Логирование данных  - стоп
        void StopLog()
        {
            IsLoging = false;
        }
        #endregion

        #region Старт-стоп логирования
        public void StartSopLog(string path)
        {
            if (IsLoging) StopLog();
            else StartLog(path);
        }
        #endregion

        async void WriteLog(string logPath, string str)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(logPath, true, System.Text.Encoding.Default))
                {
                    await sw.WriteAsync(str);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        void GetDetStr(StringBuilder builder)
        {
            builder.Append(DateTime.Now.ToString("hh:mm:ss:fff") + ";");
            for(int i = 0; i < NumSens; i++)
            {
                builder.Append(Detectors[i].y.ToString() + ";");
            }
            builder.AppendLine();
        }

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
            Client5001.ClientEvent += (message) => 
            { 
                ElapsedTime5001 = 0;
                lastTime = DateTime.Now;
                if (!Client5001.Connected) IsLoging = false;
            };            
        } 
        #endregion
    }
}
