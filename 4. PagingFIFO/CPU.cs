using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _4.PagingFIFO
{
    public class CPU
    {
        int _numOfProcessesToCreate;
        MemoryManagementUnit _MMU;

        public CPU(int numOfProcessesToStart, MemoryManagementUnit MMU)
        {
            _numOfProcessesToCreate = numOfProcessesToStart;
            _MMU = MMU;
        }

        public void StartProcesses()
        {            
            for (int i = 0; i < _numOfProcessesToCreate; ++i)
            {
                new Process(i, _MMU);
                Thread.Sleep(2000);
            }
        }
    }
}
