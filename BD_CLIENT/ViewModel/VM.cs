using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
            Model.Port5000AnswerEvent += Port5000AnswerUpdate;
            Model.ReceiveDetHandler += UpdateDetectorsValue;
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

        #region Команда "Стоп непрерывного сканирования"
        RelayCommand _stopConstScanCommand;
        public RelayCommand StopConstScanCommand
        {
            get => _stopConstScanCommand ?? (_stopConstScanCommand = new RelayCommand(p => Model.StopConstScan(), p => true));
        }
        #endregion

        #region Команда "Одиноченое сканирование"
        RelayCommand _singleScanCommand;
        public RelayCommand SingleScanCommand
        {
            get => _singleScanCommand ?? (_singleScanCommand = new RelayCommand(p => Model.SingleScan(), p => true));
        }
        #endregion

        #region Команда установить кол-во сенсоров
        RelayCommand _setNumSensCommand;
        public RelayCommand SetNumSensCommand
        {
            get => _setNumSensCommand ?? (_setNumSensCommand = new RelayCommand(p => Model.SetNumSens(), p => true));
        }
        #endregion

        #region Команда "Установить делитель усиления"
        RelayCommand _setDivGainCommand;
        public RelayCommand SetDivGainCommand
        {
            get => _setDivGainCommand ?? (_setDivGainCommand = new RelayCommand(p => Model.SetDivGain(), p => true));
        }
        #endregion

        #region Команда "Установить задержку экспозиции"
        RelayCommand _setExpDelCommand;
        public RelayCommand SetExpDelCommand
        {
            get => _setExpDelCommand ?? (_setExpDelCommand = new RelayCommand(p => Model.SetExpDelay(), p => true));
        }
        #endregion

        #region Команда "Установить время экспозиции"
        RelayCommand _setExpTimeCommand;
        public RelayCommand SetExpTimeCommand
        {
            get => _setExpTimeCommand ?? (_setExpTimeCommand = new RelayCommand(p => Model.SetExpTime(), p => true));
        }
        #endregion

        #region Команда "Получить статус системы"
        RelayCommand _getSysStatusCommand;
        public RelayCommand GetSysStatusCommand
        {
            get => _getSysStatusCommand ?? (_getSysStatusCommand = new RelayCommand(p => Model.GetSysSytatus(), p => true));
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

        #region Строка ответов от порта 5000
        string _portAnswer5000="";
        public string PortAnswer5000
        {
            get => _portAnswer5000;
            set { Set(ref _portAnswer5000, value); }
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

        #region Данные детектров
        IEnumerable<DataPoint> _detectors;
        public IEnumerable<DataPoint> Detectors { get => _detectors; set => Set(ref _detectors, value); }
        #endregion

        #region Количеств детекторов
        int numDet;
        public int NumDet
        {
            get => Model.NumSens;
            set 
            {
                Model.NumSens = value;                
                Set(ref numDet, Model.NumSens); 
            }
        }
        #endregion

        #region Делитель усиления АЦП
        int divGain;
        public int DivGain
        {
            get => Model.DivGain;
            set {
                Model.DivGain = value;
                Set(ref divGain, Model.DivGain);
            }
        }
        #endregion

        #region Задержка экспозиции
        int expDelay;
        public int ExpDelay
        {
            get => Model.ExpDelay;
            set {
                Model.ExpDelay = value;
                Set(ref expDelay, Model.ExpDelay);
            }            
        }
        #endregion

        #region Время экспозиции
        int _expTime;
        public int ExpTime
        {
            get => Model.ExpTime;
            set
            {
                Model.ExpTime = value;
                Set(ref _expTime, Model.ExpTime);
            }
        }
        #endregion

        #region Обновить строку состояния
        void UpdateMessage(string message)
        {
            EventMessage = EventMessage + DateTime.Now.ToString("T") + " : "  + message + "\r" + "\n";
            Connected5000 = Model.Client5000.Connected;
            Connected5001 = Model.Client5001.Connected;
        }
        #endregion

        #region Обновить строку ответов от порта 5000
        void Port5000AnswerUpdate(string message)
        {
            PortAnswer5000 = PortAnswer5000 + DateTime.Now.ToString("T") + " : "+ message;
        }
        #endregion

        #region Обновить данные детекторов
        void UpdateDetectorsValue(int length)
        {
            var dataPoints = new List<DataPoint>(length);
            for (int i = 0; i < length; i++)
            {
                dataPoints.Add(Model.Detectors[i]);
            }                       
            Detectors = dataPoints;
        }
        #endregion



    }
}
