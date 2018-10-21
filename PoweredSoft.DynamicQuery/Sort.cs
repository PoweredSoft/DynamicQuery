using System;
using System.Collections.Generic;
using System.Text;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery
{
    public class Sort : ISort
    {
        public string Path { get; set; }
        public bool? Ascending { get; set; }

        public Sort()
        {

        }

        public Sort(string path)
        {
            Path = path;
        }

        public Sort(string path, bool? ascending)
        {
            Path = path;
            Ascending = ascending;
        }
    }
}
