using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4.PagingFIFO
{
    public struct PageInfo
    {
        public int ProcessID;
        public int PageNumber; 
    }

    public class MemoryManagementUnit
    {
        public event Action<int, int> PageTableRequestEvent;
        public event Action<int, int, int> PageTableAnswerEvent;

        public event Action<int, int> PageFaultOccuredEvent;
        public event Action<bool, int, int, int> PageFaultHandledEvent;

        public event Action<int, int> PageAddedToPhysicalMemoryEvent;
        public event Action<int, int, int, int> PageReplacedEvent;

        public event Action<int> ProcessFinishedWorkEvent;

        PhysicalMemory _physicalMemory;
        VirtualMemory _virtualMemory;

        Dictionary<int, PageTable> _pageTables;

        List<PageInfo> _addedPagesInfo;

        static readonly object _object = new object();

        public MemoryManagementUnit(PhysicalMemory physicalMemory, VirtualMemory virtualMemory)
        {
            _physicalMemory = physicalMemory;
            _virtualMemory = virtualMemory;

            _pageTables = new Dictionary<int, PageTable>();

            _addedPagesInfo = new List<PageInfo>();
        }

        public void LoadPagesOfProcessIntoMemory(Process processWithPagesToLoad)
        {
            Page[] pagesToLoad = processWithPagesToLoad.PagesOfProcess;

            int loadingProcessID = processWithPagesToLoad.ProcessID;
            int numOfPagesToLoad = pagesToLoad.Length;

            _pageTables.Add(loadingProcessID, new PageTable(numOfPagesToLoad));

            PageTable pageTableOfProcess = _pageTables[loadingProcessID];

            int i = 0;
            while (!_physicalMemory.IsFull && i < numOfPagesToLoad)
            {
                pageTableOfProcess.ChangeFrameNumberForPageNumber(
                    pagesToLoad[i].PageNumber, _physicalMemory.InsertPage(pagesToLoad[i]));

                _addedPagesInfo.Add(
                    new PageInfo() { ProcessID = loadingProcessID, PageNumber = pagesToLoad[i].PageNumber });                
                PageAddedToPhysicalMemoryEvent?.Invoke(loadingProcessID, pagesToLoad[i].PageNumber);

                ++i;
            }

            while (i < numOfPagesToLoad)
            {
                _virtualMemory.InsertPage(pagesToLoad[i++]);
            }

            processWithPagesToLoad.ProcessFinishedWorkEvent += RemovePages;
        }        

        public Page GetPage(int processID, int pageNumber)
        {
            PageTableRequestEvent?.Invoke(processID, pageNumber);

            int frameNumber = _pageTables[processID].FindFrameNumber(pageNumber);

            if (frameNumber != -1)
            {
                PageTableAnswerEvent?.Invoke(frameNumber, processID, pageNumber);
                return _physicalMemory.GetPage(frameNumber);
            }
            else
            {
                PageFaultOccuredEvent?.Invoke(processID, pageNumber);
                return HandlePageFault(processID, pageNumber);
            }
        }

        private Page HandlePageFault(int processID, int pageNumber)
        {            
            Page askedPage = _virtualMemory.ExtractPage(processID, pageNumber);            

            if (_physicalMemory.IsFull)
            {
                ReplacePageFIFO(askedPage);
            }
            else
            {                
                int newFrameNumber = _physicalMemory.InsertPage(askedPage);
                _pageTables[processID].ChangeFrameNumberForPageNumber(pageNumber, newFrameNumber);

                _addedPagesInfo.Add(
                    new PageInfo() { ProcessID = processID, PageNumber = pageNumber });                

                PageFaultHandledEvent?.Invoke(false, newFrameNumber, processID, pageNumber);
                PageTableAnswerEvent?.Invoke(newFrameNumber, processID, pageNumber);
            }
           
            return askedPage;
        }

        private void ReplacePageFIFO(Page newPage)
        {                        
            // Узнаем информацию о Page, которую будем заменять
            PageInfo oldPageInfo = _addedPagesInfo[0];
            _addedPagesInfo.RemoveAt(0);                       

            // Находим её Frame Number в Оперативной Памяти
            int frameNumber = _pageTables[oldPageInfo.ProcessID].FindFrameNumber(oldPageInfo.PageNumber);

            // Заменяем Frame в Оперативной Памяти
            Page oldPage = _physicalMemory.SwapPages(newPage, frameNumber);

            // Вставляем "вынутую" из Оперативной Памяти Page куда-нибудь в Виртуальную Память
            _virtualMemory.InsertPage(oldPage);

            // Указываем в таблице для процесса вынутой Page, что теперь она находится НЕ в Оперативной Памяти
            _pageTables[oldPageInfo.ProcessID].ChangeFrameNumberForPageNumber(oldPageInfo.PageNumber, -1);

            // Указываем в таблице для процесса вставленной Page её адрес в Оперативной Памяти
            _pageTables[newPage.ProcessID].ChangeFrameNumberForPageNumber(newPage.PageNumber, frameNumber);

            _addedPagesInfo.Add(
                new PageInfo() { ProcessID = newPage.ProcessID, PageNumber = newPage.PageNumber });            
            PageReplacedEvent?.Invoke(oldPageInfo.ProcessID, oldPageInfo.PageNumber, newPage.ProcessID, newPage.PageNumber);

            PageFaultHandledEvent?.Invoke(true, frameNumber, newPage.ProcessID, newPage.PageNumber);
            PageTableAnswerEvent?.Invoke(frameNumber, newPage.ProcessID, newPage.PageNumber);
        }

        private void RemovePages(int finishedProcessID)
        {
            ProcessFinishedWorkEvent?.Invoke(finishedProcessID);

            PageTable pageTable = _pageTables[finishedProcessID];
            int frameNumber;

            for (int i = 0; i < pageTable.NumOfPages; ++i)
            {
                frameNumber = pageTable.FindFrameNumber(i);

                if (frameNumber != -1)
                {
                    _physicalMemory.ExtractPage(frameNumber);
                }
                else
                {
                    _virtualMemory.ExtractPage(finishedProcessID, i);
                }

                lock (_object)
                {
                    int j = _addedPagesInfo.FindIndex(pI =>
                    {
                        if (pI.ProcessID == finishedProcessID && pI.PageNumber == i)
                        {
                            return true;
                        }

                        return false;
                    });

                    if (j != -1)
                    {
                        _addedPagesInfo.RemoveAt(j);
                    }
                }                                
            }

            _pageTables.Remove(finishedProcessID);
        }
    }
}