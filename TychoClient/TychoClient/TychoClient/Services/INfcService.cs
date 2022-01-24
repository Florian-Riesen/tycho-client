using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TychoClient.Models;

namespace TychoClient.Services
{
    public interface INfcService
    {
        Task<bool> WriteToChip(FreeloaderCustomerData data);
        Task<FreeloaderCustomerData> ReadFromChip();
    }
}
