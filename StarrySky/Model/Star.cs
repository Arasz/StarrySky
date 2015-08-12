using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace StarrySky.Model
{
    class Star
    {
        public Point Location { get; set; }
        public bool Rotating { get; set; }

        public Star(Point location, bool rotating = false)
        {
            Location = location;
            Rotating = rotating;
        }

    }
}
