using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClamShell.MessageBus.Models.Enums;

namespace ClamShell.MessageBus.Models
{
    public class TransferModel<T>
    {
        public T Data { get; set;}
    }
}
