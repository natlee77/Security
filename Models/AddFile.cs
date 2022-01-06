using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Security.Models
{
    public class AddFile
    {
        public Guid Id { get; set; }
        public string NotrustedName { get; set; }
        public DateTime TimeStamp { get; set; }
        public long? Size { get; set; }  // storlek på filen

        public byte[] Content { get; set; } // innerhållet i filen  (0,1) bit-> (8 bit =1 byte)
    }
} 
 
