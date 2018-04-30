using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

namespace CacheAPIService.Models
{
    /**
     * Repository for implementing business logic of cache such as storage and handling of Documents.
     */ 
    public class DocumentRepository: IDocumentRepository
    {
        private static Dictionary<int, String> documents = new Dictionary<int, String>();
        private static readonly Timer timer = new Timer(OnTimerElapsed);
        private static Dictionary<int, System.DateTime> deleteSchedule = new Dictionary<int, System.DateTime>();
        //Unifying documents & deleteSchedule into a single dictionary may be preferable

        /**
         * Constructor for DocumentRepository
         */
        public DocumentRepository()
        {
            StartTimer();
        }

        /**
         * Timer initialisation
         */
        private static void StartTimer()
        {
            timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        /**
         * Callback function for timer (this function is invoked at every time interval as 
         * specified in Timer.Change()). Determines if any Documents have existed past their 
         * Time To Live, and if so removes them from the cache.
         * 
         * Args:
         *  object state
         *      - object keeping track of the state of the program for the timer. 
         */
        private static void OnTimerElapsed(object state)
        {
            foreach (KeyValuePair<int, DateTime> scheduledDeletion in deleteSchedule.ToArray())
            {
                if (deleteSchedule[scheduledDeletion.Key] <= DateTime.Now)
                {
                    documents.Remove(scheduledDeletion.Key);
                    deleteSchedule.Remove(scheduledDeletion.Key);
                }
            }
        }

        /**
         * Adds Id, DateTime key-value pair to the delete schedule dictionary to specify the 
         * time in which a document's Time To Live has been exceeded.
         * 
         * Args:
         *  int Id
         *      - Id of Document
         *  int TimeToLive
         *      - Time to Live of Document
         *      
         */
        public int ScheduleDeletionTime(int id, int timeToLive)
        {
            DateTime deleteTime = (DateTime.Now).AddSeconds(timeToLive);
            if (deleteSchedule.ContainsKey(id))
            {
                deleteSchedule[id] = deleteTime;
            }
            else
            {
                deleteSchedule.Add(id, deleteTime);
            }
            return id;
        }
        
        /**
         * If Document is stored in cache, return a copy of the Document corresponding to the 
         * provided id. If no Document found in cache's store of documents, return null.
         * 
         * Args:
         *  int id
         *      - id of document to retrieve from document store
         * 
         * Returns:
         *  Document item
         *      - Copy of Document stored in cache
         */
        public Document RetrieveDocument(int id)
        {
            String message = "";
            Document item = new Document();
            if (documents.TryGetValue(id, out message))
            {
                item.ID = id;
                item.Message = message;
                return item;
            } else
            {
                item = null;
            }
            return item;
        }

        /**
         * Add a document to the store of documents. Returns document added if successful. Returns null
         * value if provided item was not able to be added.
         * 
         * Args:
         *  Document item
         * 
         * Returns:
         *  Document item
         *      - the Document added to cache's document store.
         */
        public Document AddDocument(Document item)
        {
            try
            {
                documents.Add(item.ID, item.Message);
            } catch (ArgumentException)
            {
                item = null;
            }
            return item;
        }

        /**
         * Set document store and delete schedule to new empty dictionaries respectively.
         */
        public void ClearCache()
        {
            documents = new Dictionary<int, String>();
            deleteSchedule = new Dictionary<int, System.DateTime>();
        }
    }
}