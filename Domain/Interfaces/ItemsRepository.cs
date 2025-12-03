using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ItemsRepository
    {
        List<ItemValidating> Get();
        void Save(List<ItemValidating> items);
        void Approve(List<string> ids);

    }
}
