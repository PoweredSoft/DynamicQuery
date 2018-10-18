using System;
using System.Collections.Generic;
using System.Text;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery
{
    public class Group : IGroup
    {
        public string Path { get; set; }
        public bool? Ascending { get; set; }
    }
}
