﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheAPIService.Models
{
    /**
     * Interface for DocumentRepository class to implement. 
     */
    public interface IDocumentRepository
    {
        Document RetrieveDocument(int id);
        Document AddDocument(Document item);
        int ScheduleDeletionTime(int Id, int TimeToLive);
        void ClearCache();
    }
}
