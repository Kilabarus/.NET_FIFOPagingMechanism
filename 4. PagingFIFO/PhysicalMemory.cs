using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4.PagingFIFO
{
    public class PhysicalMemory
    {
        public event Action<int, int, int> PhysicalMemoryUpdatedEvent;

        Frame[] _frames;
        int _size, _filled = 0;

        static readonly object _object = new object();

        public PhysicalMemory(int size)
        {
            _size = size;
            _frames = new Frame[_size];

            for (int i = 0; i < _size; ++i)
            {
                _frames[i] = new Frame();
            }
        }

        public bool IsFull { get => _size == _filled; }

        public Page SwapPages(Page pageToInsert, int frameNumber)
        {
            lock (_object)
            {
                Page oldPage = _frames[frameNumber].StoredPage;
                _frames[frameNumber].StoredPage = pageToInsert;

                PhysicalMemoryUpdatedEvent?.Invoke(frameNumber, pageToInsert.ProcessID, pageToInsert.PageNumber);

                return oldPage;
            }            
        }

        public int InsertPage(Page pageToInsert)
        {
            lock (_object)
            {
                for (int frameNumber = 0; frameNumber < _frames.Length; ++frameNumber)
                {
                    if (_frames[frameNumber].StoredPage == null)
                    {
                        _frames[frameNumber].StoredPage = pageToInsert;

                        ++_filled;
                        PhysicalMemoryUpdatedEvent?.Invoke(frameNumber, pageToInsert.ProcessID, pageToInsert.PageNumber);

                        return frameNumber;
                    }
                }

                return -1;
            }            
        }

        public Page GetPage(int frameNumber)
        {
            lock (_object)
            {
                return _frames[frameNumber].StoredPage;
            }            
        }

        public Page ExtractPage(int frameNumber)
        {
            lock (_object)
            {
                --_filled;
                PhysicalMemoryUpdatedEvent?.Invoke(frameNumber, -1, -1);

                return _frames[frameNumber].ExtractStoredPage();
            }            
        }
    }
}
