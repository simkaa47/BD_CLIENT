using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BD_CLIENT.Models
{
    class Model
    {
        
        public event ClientEventHandler EventModelMessage;
        
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

        public void StartConstScan()
        {
            if (Client5000.Connected)
            {
                Client5000.Write(GetCommand("$cc")); 
            }
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
            
        } 
        #endregion
    }
}
