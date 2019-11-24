using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSynchronizationFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            Connection connection = new Connection();
            connection.Client();
        }
    }
}
