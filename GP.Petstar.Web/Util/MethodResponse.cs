﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GP.Petstar.Web.Util
{
    public class MethodResponse<T>
    {

        public int code { get; set; }
        public string message { get; set; }
        public T Result { get; set; }
    }
}