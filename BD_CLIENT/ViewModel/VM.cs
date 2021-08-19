using System;
using System.Collections.Generic;
using System.Text;
using BD_CLIENT.Infrastructure;
using BD_CLIENT.Models;

namespace BD_CLIENT.ViewModel
{
    class VM: PropertyChangedClass
    {
        public VM()
        {
            Model.Client5000.ClientEvent += UpdateMessage;
            Model.Client5001.ClientEvent += UpdateMessage;
        }
        Model Model { get; set; } = new Model();
        #region Команды

        #region Команда "Закрыть все соединения"
        RelayCommand _closeAllCommCommand;
        public RelayCommand CloseAllCommCommand         
        {
            get
            { 
                if(_closeAllCommCommand==null) _closeAllCommCommand = new RelayCommand((p) => Model?.StopAll(), p => true);
                return _closeAllCommCommand;
            }
            
        }
        #endregion

        #region Команда "Коннект-дисконнект 5000
        RelayCommand _connect5000Command;
        public RelayCommand Connect5000Command
        {
            get
            {
                if (_connect5000Command == null) _connect5000Command = new RelayCommand((p) => Model.ConnectDisconnect5000(), p => true);
                return _connect5000Command;
            }
            
        }
        #endregion

        #region Команда "Коннект-дисконнект 5001
        RelayCommand _connect5001Command;
        public RelayCommand Connect5001Command
        {
            get
            {
                if (_connect5001Command == null) _connect5001Command = new RelayCommand((p) => Model.ConnectDisconnect5001(), p => true);
                return _connect5001Command;
            }

        }
        #endregion

        #region Команда "Старт непрерывного сканирования"
        RelayCommand _startConstScanCommand;
        public RelayCommand StartConstScanCommand
        {
            get => _startConstScanCommand ?? (_startConstScanCommand = new RelayCommand(p => Model.StartConstScan(), p => true));
        }
        #endregion

        #endregion

        #region Адрес платы
        string ip;
        public string IP
        {
            get => Model.IP;
            set
            {
                Model.IP = value;                
                Set(ref ip, Model.IP);
            }
        }
        #endregion

        #region Строка состояния
        string eventMessage;
        public string EventMessage { get => eventMessage; set => Set(ref eventMessage, value); }
        #endregion

        #region Статусы соединения
        bool _connected5000;
        /// <summary>
        /// Статус соединения с портом 5000
        /// </summary>
        public bool Connected5000 { get => _connected5000; set { Set(ref _connected5000, value); } }
        bool _connected5001;
        /// <summary>
        /// Статус соединения с портом 5001
        /// </summary>
        public bool Connected5001 { get => _connected5001;set { Set(ref _connected5001, value); } }        
        #endregion

        #region Обновить строку состояния
        void UpdateMessage(string message)
        {
            EventMessage = EventMessage + DateTime.Now.ToString("T") + " : "  + message + "\r" + "\n";
            Connected5000 = Model.Client5000.Connected;
            Connected5001 = Model.Client5001.Connected;
        } 
        #endregion

        
    }
}
