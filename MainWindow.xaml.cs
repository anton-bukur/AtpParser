using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AtpParser
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string homeUrl = "https://www.tennisexplorer.com/";
        public MainWindow()
        {
            string Year = "2020";
            string url = homeUrl + "calendar/atp-men/"+Year+"/";
            List<string> listStat = new List<string>();

            //List<string> listTournaments = proc.GetTournaments(url);
            //foreach (string u in listTournaments)
            //{
            //    List<string> listMatches = proc.GetMatches(homeUrl + u);
            //    if (listMatches == null)
            //    {
            //        File.AppendAllText("log.txt", homeUrl + u + "|No matches found" + Environment.NewLine);
            //        continue;
            //    }
            //    string Tournament = listMatches[listMatches.Count - 1];
            //    listMatches.RemoveAt(listMatches.Count - 1);
            //    foreach (string s in listMatches)
            //    {
            //        string result = proc.GetMatchStatistics(homeUrl + s, Tournament);
            //        listStat.Add(result);
            //    }
            //    proc.Excel(listStat);
            //    break;
            //}
            string result = proc.GetMatchStatistics("", "Abu Dhabi - exh. 2020 (UAE)");
            listStat.Add(result);


            InitializeComponent();
        }
       
    }
}
