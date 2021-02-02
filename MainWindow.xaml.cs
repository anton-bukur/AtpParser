using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace AtpParser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ICommand _manageCommand;
        private static bool _can = true;

        public ICommand ManageCommand => _manageCommand ?? (_manageCommand = new Command(t =>
        {
            switch (t.ToString())
            {
                case "Start":
                    //Task.Run(() => PressStart());
                    break;
                case "ChoseTournament":
                    ChoseTournament();
                    break;
                    //case "ChoseLink":
                    //    ChoseLink();
                    //    break;
            }
        }));
        private void ChoseTournament()
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                tournamentPath = openFileDialog.FileName;
            TrnmntPath.Text = tournamentPath;
            Properties.Settings.Default.LastTournamentPath = tournamentPath;
            Properties.Settings.Default.Save();
        }

        const string homeUrl = "https://www.tennisexplorer.com/";
        private string _tournamentPath;
        public string tournamentPath
        {
            get => _tournamentPath;
            set
            {
                _tournamentPath = value;
                OnPropertyChanged();
            }
        }

        private string _matchesPath;
        public string matchesPath
        {
            get => _matchesPath;
            set
            {
                _matchesPath = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow()
        {
            string Year = "2020";
            string url = homeUrl + "calendar/atp-men/" + Year + "/";
            List<string> listStat = new List<string>();

            List<string> listTournaments = proc.GetTournaments(url);
            foreach (string u in listTournaments)
            {
                List<string> listMatches = proc.GetMatches(homeUrl + u);
                if (listMatches == null)
                {
                    File.AppendAllText("log.txt", homeUrl + u + "|No matches found" + Environment.NewLine);
                    continue;
                }
                string Tournament = listMatches[listMatches.Count - 1];
                listMatches.RemoveAt(listMatches.Count - 1); // турнир теперь не нужен :)
                foreach (string s in listMatches)
                {
                    string result = proc.GetMatchStatistics(homeUrl + s, Tournament);
                    if (result != null) listStat.Add(result);
                }

                //break;
            }
            proc.Excel(listStat);
            //string result = proc.GetMatchStatistics("", "Abu Dhabi - exh. 2020 (UAE)");
            //listStat.Add(result);


            InitializeComponent();
        }

        private class NotifyPropertyChangedInvocatorAttribute : Attribute
        {
        }

        private class Command : ICommand
        {
            private Action<object> p;

            public Command(Action<object> p)
            {
                this.p = p;
            }

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            public bool CanExecute(object parameter)
            {
                throw new NotImplementedException();
            }

            public void Execute(object parameter)
            {
                throw new NotImplementedException();
            }
        }
    }
}
