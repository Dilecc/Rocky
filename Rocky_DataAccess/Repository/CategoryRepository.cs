using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocky.Data;
using Rocky.Models;
using Rocky_DataAccess.Repository.IRepository;

namespace Rocky_DataAccess.Repository
{
    public class CategoryRepository: Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Category obj)
        {
            var objFromDB = base.FirstOfDefault(u => u.Id == obj.Id);
            if (objFromDB != null)
            {
                objFromDB.Name = obj.Name;
                objFromDB.DispalyOrder = obj.DispalyOrder;
            }

        }
    }
}
