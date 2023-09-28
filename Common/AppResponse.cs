using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSPL.Common
{

    public class AppResponse
    {
        public AppResponse()
        {
            this.Succeeded = false;
            this.Message = "";
        }

        public bool Succeeded { get; set; }

        public string Message { get; set; }

        public string Result { get; set; }
    }

    public class AppResponse<T> : AppResponse //where T : class
    {
        public AppResponse() : base()
        {
        }

        public T Response { get; set; }
    }
}
