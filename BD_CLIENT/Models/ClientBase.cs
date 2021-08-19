using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BD_CLIENT.Models
{
    public delegate void ClientEventHandler(string message);
    class ClientBase
    {
        #region События и делегаты
        public Action<int> RecognizeInputBytes;        
        public event ClientEventHandler ClientEvent;
        public event ClientEventHandler ClientReadEvent;
        #endregion

        #region Статус соединения
        /// <summary>
        /// Статус соединения
        /// </summary>
        public bool Connected { get; private set; }
        #endregion

        bool connecting;

        #region Ip адрес
        string ip = "";
        public string Ip
        {
            get => ip;
            set { ip = value; }
        }
        #endregion

        #region Порт
        int _port;
        /// <summary>
        /// Порт
        /// </summary>
        public int Port { get => _port; set { if (value > 0) _port = value; } }
        #endregion

        #region Клиент
        TcpClient client;
        #endregion

        #region Буффер входящих данных
        /// <summary>
        /// Буффер входящих данных
        /// </summary>
        public byte[] InBuf { get; private set; } = new byte[2000];
        #endregion        

        NetworkStream stream;

        #region Конструктор
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port">Номер порта, по которому надо подключиться</param>
        
        #endregion

        #region Методы
        #region Соединение
        public void Connect(int port, string ip)
        {
            try
            {
                Ip = ip;
                Port = port;
                client = new TcpClient();
                connecting = true;
                ClientEvent?.Invoke($"Выполняется подключение к {Ip}:{Port}");
                client.Connect(Ip, Port);
                Connected = client.Connected;
                connecting = false;
                ClientEvent?.Invoke($"Произведено подключение к {Ip}:{Port}");
                stream = client.GetStream();
                while (Connected)
                {
                    Read();
                }

            }
            catch (Exception ex)
            {
                if (Connected || connecting)ClientEvent?.Invoke($"{Ip}:{Port}: {ex.Message}");
            }
            finally
            {
                client?.Close();
                Connected = false;
                connecting = false;
            }
        } 
        #endregion

        #region Чтение из потока (выполняется синхронно)
        void Read()
        {
            do
            {
                int num = stream.Read(InBuf, 0, InBuf.Length);
                if (num == 0)
                {
                    ClientEvent?.Invoke($"{Ip}:{Port}: удаленный сервер разорвал соединение.");
                    Connected = false;
                }
                else RecognizeInputBytes?.Invoke(num);
                
            } while (stream.DataAvailable);
        }
        #endregion

        #region Запись в поток
        public async void Write(byte[] buffer)
        {
            await Task.Run(()=>
            {
                try
                {
                    stream?.WriteAsync(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    ClientEvent?.Invoke($"{Ip}:{Port}: {ex.Message}");
                    Connected = false;
                    client.Close();
                }                
            });
        }
        #endregion        

        #region Дисконнект
        public void Disconnect()
        {
            if (Connected)
            {
                Connected = false;
                ClientEvent?.Invoke($"{Ip}:{Port}: соединение завершено пользователем");
                client.Close();
            }
        }
        #endregion

        #endregion
    }
}
