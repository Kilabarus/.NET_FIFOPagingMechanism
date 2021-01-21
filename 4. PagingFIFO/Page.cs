using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4.PagingFIFO
{
    public class Page
    {
        int _pageNumber;
        int _processID;

        public Page(int processID, int pageNumber)
        {            
            _processID = processID;
            _pageNumber = pageNumber;
        }

        public int PageNumber { get => _pageNumber; }
        public int ProcessID { get => _processID; }
    }    
}
