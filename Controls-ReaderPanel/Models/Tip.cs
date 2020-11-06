using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Richasy.Controls.Reader.Models
{
    public class Tip
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Tip()
        {

        }
        public Tip(string id, string title,string desc)
        {
            Id = id;
            Title = title;
            Description = desc;
        }

        public override bool Equals(object obj)
        {
            return obj is Tip tip &&
                   Id == tip.Id;
        }

        public override int GetHashCode()
        {
            return 2108858624 + EqualityComparer<string>.Default.GetHashCode(Id);
        }
    }
}
