using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Richasy.Controls.Reader.Models
{
    public class History
    {
        public Chapter Chapter { get; set; }
        public int Start { get; set; }
        public DateTime Time { get; set; }
        public double Progress { get; set; }
        public History()
        {

        }
        public History(Chapter chapter, int start,double progress)
        {
            Chapter = chapter;
            Start = start;
            Time = DateTime.Now;
            Progress = progress;
        }

        public override bool Equals(object obj)
        {
            return obj is History history &&
                   Chapter == history.Chapter;
        }

        public override int GetHashCode()
        {
            return 2108858624 + EqualityComparer<Chapter>.Default.GetHashCode(Chapter);
        }
    }
}
