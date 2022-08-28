using eComStore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eComStore.DataAccess.Repository.IRepository
{
    public interface ICoverTypeRepository:IRepository<CoverType>
    {
        void Update(CoverType obj);
    }
}
