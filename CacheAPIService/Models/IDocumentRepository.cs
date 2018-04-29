using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheAPIService.Models
{
    public interface IDocumentRepository
    {
        Document Get(int id);
        Document Add(Document item);
        Document Remove(Document item);
        void ClearCache();
    }
}
