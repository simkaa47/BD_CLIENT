using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BD_CLIENT.Models
{
    class Model
    {
        
        public event ClientEventHandler Port5000AnswerEvent;
        
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
        } 
        #endregion
    }
}
