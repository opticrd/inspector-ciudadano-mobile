using System;
using System.Collections.Generic;
using System.Text;

namespace Inspector.Framework.Dtos
{
    public class Response<T>
    {
        public bool Valid { get; set; }
        public T Payload { get; set; }
    }
}
