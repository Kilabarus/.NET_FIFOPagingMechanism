using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace _4.PagingFIFO
{
    public class Process
    {
        public event Action<int> ProcessFinishedWorkEvent;

        Random _rnd;

        Thread _thread;
        Page[] _pages;
        int _processID;

        MemoryManagementUnit _MMU;        

        public Process(int processID, MemoryManagementUnit MMU)
        {
            _processID = processID;
            _MMU = MMU;

            _rnd = new Random();
            
            int numOfPages = _rnd.Next(2, 6);
            _pages = new Page[numOfPages];

            for (int i = 0; i < numOfPages; ++i)
            {
                _pages[i] = new Page(_processID, i);
            }

            _MMU.LoadPagesOfProcessIntoMemory(this);

            _thread = new Thread(Work);
            _thread.Start();
        }      

        public int ProcessID { get => _processID; }

        public Page[] PagesOfProcess { get => _pages; }
        
        public void Work()
        {
            Page neededPage;
            int numOfNeededPage;

            int pagesToGet = _rnd.Next(2, _pages.Length);
            int pagesHandled = 0;

            while (pagesHandled < pagesToGet)
            {
                numOfNeededPage = _rnd.Next(_pages.Length);
                Thread.Sleep(_rnd.Next(2000, 3000));
                neededPage = _MMU.GetPage(_processID, numOfNeededPage);
                Thread.Sleep(_rnd.Next(6000, 9000));

                ++pagesHandled;
            }

            ProcessFinishedWorkEvent.Invoke(_processID);
            _thread.Abort();
        }
    }
}
