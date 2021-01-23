using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtpParser.Models
{
    public class match
    {
        public string Atp { get; set; }
        public string Location { get; set; }
        public string Tournament { get; set; }
        public string Date;
        public string Series { get; set; }
        public string Court { get; set; }
        public string Surface { get; set; }
        public string Round { get; set; }
        public string BestOf { get; set; }
        public string Winner { get; set; }
        public string Loser { get; set; }
        public string WRank { get; set; }
        public string LRank { get; set; }
        public string WPts { get; set; }
        public string LPts { get; set; }
        public string[] W { get; set; }
        public string[] L { get; set; }
        public string[] WT { get; set; }
        public string[] LT { get; set; }
        public string Wsets { get; set; }
        public string Lsets { get; set; }
        public string Comment { get; set; }
        public List<string> HomeAway { get; set; }
        public string B365W { get; set; }
        public string B365L { get; set; }
        public string PSW { get; set; }
        public string PSL { get; set; }
        public string MaxW { get; set; }
        public string MaxL { get; set; }
        public string AvgW { get; set; }
        public string AvgL { get; set; }
    }
}
