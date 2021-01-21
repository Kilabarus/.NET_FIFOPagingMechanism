using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4.PagingFIFO
{    
    public class PageTable
    {        
        int[] _pageTable;

        public PageTable(int numOfPages)
        {
            _pageTable = new int[numOfPages]; 
            
            for (int i = 0; i < numOfPages; ++i)
            {
                _pageTable[i] = -1;
            }
        }

        public int NumOfPages { get => _pageTable.Length; }
     
        public void ChangeFrameNumberForPageNumber(int pageNumber, int frameNumber)
        {
            _pageTable[pageNumber] = frameNumber;
        }
        
        public int FindFrameNumber(int pageNumber)
        {
            return _pageTable[pageNumber];
        }
    }
}
