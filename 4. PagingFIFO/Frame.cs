using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4.PagingFIFO
{
    public class Frame
    {
        Page _page;

        public Page StoredPage { get => _page; set => _page = value; }

        public Page ExtractStoredPage()
        {
            Page resPage = _page;
            _page = null;

            return resPage;
        }        
    }
}
