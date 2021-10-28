using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Enums
{
    public enum  Octant 
    {
        O1 = 0x01,	// = 0b00000001
        O2 = 0x02,	// = 0b00000010
        O3 = 0x04,	// = 0b00000100
        O4 = 0x08,	// = 0b00001000
        O5 = 0x10,	// = 0b00010000
        O6 = 0x20,	// = 0b00100000
        O7 = 0x40,	// = 0b01000000
        O8 = 0x80	// = 0b10000000
    };
}
