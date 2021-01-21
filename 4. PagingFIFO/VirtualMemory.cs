using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4.PagingFIFO
{
    public class VirtualMemory
    {
        public event Action<int, int, int> VirtualMemoryUpdatedEvent;

        Page[] _pages;
        int _sizeOfVirtualMemory;

        static readonly object _object = new object();

        public VirtualMemory(int capacity)
        {
            _sizeOfVirtualMemory = capacity;
            _pages = new Page[_sizeOfVirtualMemory];
        }

        public void InsertPage(Page pageToInsert)
        {
            lock (_object)
            {
                for (int i = 0; i < _sizeOfVirtualMemory; ++i)
                {
                    if (_pages[i] == null)
                    {
                        _pages[i] = pageToInsert;
                        VirtualMemoryUpdatedEvent?.Invoke(i, pageToInsert.ProcessID, pageToInsert.PageNumber);

                        return;
                    }
                }

                throw new OutOfMemoryException();
            }            
        }

        public Page ExtractPage(int processID, int pageNumber)
        {
            lock (_object)
            {
                for (int i = 0; i < _sizeOfVirtualMemory; ++i)
                {
                    if (_pages[i] != null && _pages[i].ProcessID == processID && _pages[i].PageNumber == pageNumber)
                    {
                        Page resPage = _pages[i];
                        _pages[i] = null;

                        VirtualMemoryUpdatedEvent?.Invoke(i, -1, -1);

                        return resPage;
                    }
                }

                return null;
            }            
        }
    }
}
