using AlarmWorkflow.Shared.Core;
using AlarmWorkflow.Windows.UIContracts.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using System.Net.Sockets;

namespace AlarmWorkflow.TestOperation
{
    class ViewModel : ViewModelBase
    {
        #region Fields
        private MainWindow _window;
        private Operation _currentOperation;
        private string _server;
        #endregion

        #region Properties

        public ObservableCollection<String> VehicleCollection { get; set; }

        /// <summary>
        /// Gets or sets the current <see cref="Operation"/>.
        /// </summary>
        public Operation CurrentOperation
        {
            get { return _currentOperation; }
            set
            {
                _currentOperation = value;
                OnPropertyChanged("CurrentOperation");
            }
        }

        /// <summary>
        /// Gets or sets the remote Server
        /// </summary>
        public string Server
        {
            get { return _server; }
            set
            {
                _server = value;
                OnPropertyChanged("Server");
            }
        }


        #endregion

        #region Commands
        #region Buttons
        /// <summary>
        /// The command assigned to the send button
        /// </summary>
        public ICommand SendCommand { get; set; }

        private async void SendCommand_Execute(object param)
        {
            // The City-text often contains a dash after which the administrative city appears multiple times (like "City A - City A City A").
            // However we can (at least with google maps) omit this information without problems!
            string oldEinsatzortCity = CurrentOperation.Einsatzort.City;
            if (!String.IsNullOrEmpty(oldEinsatzortCity))
            {
                int dashIndex = oldEinsatzortCity.IndexOf(" - ");
                if (dashIndex != -1)
                {
                    CurrentOperation.Einsatzort.City = CurrentOperation.Einsatzort.City.Substring(0, dashIndex).Trim();
                }
            }

            //Send operation
            try
            {
                Helper.SendOperation(Server, CurrentOperation);

                await _window.ShowMessageAsync("Erfolgreich", "Operation wurde gesendet");
            }
            catch (Exception e)
            {
                await _window.ShowMessageAsync("Error", "Fehler beim senden der Operation");
            }
            

            //Back to the good old things
            CurrentOperation.Einsatzort.City = oldEinsatzortCity;
        }


        /// <summary>
        /// The command assigned to the generate pdf button
        /// </summary>
        public ICommand GeneratePDFCommand { get; set; }

        private void GeneratePDFCommand_Execute(object param)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF file (*.pdf)|*.pdf";
            if (saveFileDialog.ShowDialog() == true)
            {
                string file = saveFileDialog.FileName;
                Helper.GeneratePDF(CreateOperation(), file);
                Process.Start(file);
            }
        }

        /// <summary>
        /// The command assigned to the add vehicle button
        /// </summary>
        public ICommand AddVehicleCommand { get; set; }

        private async void AddVehicleCommand_Execute(object param)
        {
            var response = await _window.ShowInputAsync("Name", "Name des Fahrzeuges");
            if (!String.IsNullOrEmpty(response))
            {
                VehicleCollection.Add(response);
            }
        }

        #endregion

        #region MenuCommands
        public ICommand MenuSaveCommand { get; set; }
        public ICommand MenuNewCommand { get; set; }
        public ICommand MenuOpenCommand { get; set; }
        public ICommand MenuExitCommand { get; set; }

        private void MenuSaveCommand_Execute(object param)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "AlarmWorkflow JSON file (*.json)|*.json";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (saveFileDialog.ShowDialog() == true)
            {
                OperationSerializer.Serialize(CreateOperation(), saveFileDialog.FileName);
            }
        }
        private void MenuNewCommand_Execute(object param)
        {
            CurrentOperation = new Operation();
        }
        private void MenuExitCommand_Execute(object param)
        {
            Application.Current.Shutdown();
        }
        private void MenuOpenCommand_Execute(object param)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "AlarmWorkflow JSON (*.json)|*.json";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                CurrentOperation = OperationSerializer.Deserialize(openFileDialog.FileName);

                VehicleCollection.Clear();
                foreach (var item in CurrentOperation.Resources)
                {
                    VehicleCollection.Add(item.FullName);
                }
            }
        }
        #endregion

        #endregion

        #region Constructors

        internal ViewModel(MainWindow window)
            : base()
        {
            _window = window;
            VehicleCollection = new ObservableCollection<string>();
            CurrentOperation = new Operation();

            if (String.IsNullOrEmpty(Server))
                Server = "localhost:" + Helper.DEFAULT_PORT;
        }

        #endregion

        #region Methods
        private Operation CreateOperation()
        {
            CurrentOperation.Timestamp = CurrentOperation.TimestampIncome = DateTime.Now;

            Random rnd = new Random();
            string number = DateTime.Now.ToString("yyMMdd");
            string randomEinsatz = "T 1.1 " + number + " " + rnd.Next(1000, 9999);
            CurrentOperation.OperationNumber = randomEinsatz;

            //Vehicle
            CurrentOperation.Resources.Clear();
            foreach (var item in VehicleCollection)
            {
                CurrentOperation.Resources.Add(new OperationResource() {
                    FullName = item,
                    Timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                });
            }

            return CurrentOperation;
        }
        #endregion
    }
}
