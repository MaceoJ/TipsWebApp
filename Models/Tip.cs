using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TipsTricksWebApp.Models
{
    public class Tip
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Game { get; set; }

        public string Description { get; set; }

        public string User { get; set; }
        public Tip()
        {

        }
    }
}
