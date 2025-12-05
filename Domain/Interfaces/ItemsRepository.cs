using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ItemsRepository
    {
        List<IItemValidating> Get();
        void Save(List<IItemValidating> items);
        void Add(List<IItemValidating> items);

        void Approve(List<string> ids);

    }
}
