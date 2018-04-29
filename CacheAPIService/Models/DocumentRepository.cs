using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

namespace CacheAPIService.Models
{
    public class DocumentRepository: IDocumentRepository
    {
        //Dictionary good because only retrieving one at a time and a unique identifier is used
        private static Dictionary<int, String> Documents = new Dictionary<int, String>();
        private static readonly Timer Timer = new Timer(OnTimerElapsed);
        private static Dictionary<int, System.DateTime> DeleteSchedule = new Dictionary<int, System.DateTime>();
        private int TTL = 30;

        /*Constructor*/
        public DocumentRepository()
        {
            Start();
        }

        public static void Start()
        {
            //look into what each of the arguments mean
            Timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        /*I believe the state object is the object keeping track of the state of the program for the timer. without specifying an
         additional state object, the object state is the timer itself.*/
        private static void OnTimerElapsed(object state)
        {
            System.Diagnostics.Debug.WriteLine(DateTime.Now);
            /*Can't modify a collection while its being enumerated in a foreach loop
             i.e. you can't change the dictionary you're looping over while also looping through it, so make a list of dictionary,
             loop through the list which can change the dictionary itself*/
            foreach (KeyValuePair<int, DateTime> ScheduledDeletion in DeleteSchedule.ToArray())
            {
                System.Diagnostics.Debug.WriteLine(ScheduledDeletion.Key + ", @ " + DeleteSchedule[ScheduledDeletion.Key]);
                if (DeleteSchedule[ScheduledDeletion.Key] <= DateTime.Now)
                {
                    System.Diagnostics.Debug.WriteLine("DELETED");
                    Documents.Remove(ScheduledDeletion.Key);
                    DeleteSchedule.Remove(ScheduledDeletion.Key);
                }
            }
        }

        /*Add the document Id to DeleteSchedule with the DateTime of when to delete*/
        private int ScheduleDeletionTime(int Id, int TimeToLive)
        {
            DateTime DeleteTime = (DateTime.Now).AddSeconds(TimeToLive);
            //If it exists update, if it doesn't Add.
            if (DeleteSchedule.ContainsKey(Id))
            {
                //Document is currently stored in the cache -- renew it's TTL
                DeleteSchedule[Id] = DeleteTime;
            } else
            {
                //Document is not currently in the cache -- add to cache
                DeleteSchedule.Add(Id, DeleteTime);
            }
            
            return Id;
        }

        public Document Get(int Id)
        {
            String Message = "";
            Document Item = new Document();
            if (Documents.TryGetValue(Id, out Message))
            {
                //Document Item = new Document();
                Item.ID = Id;
                Item.Message = Message;
                System.Diagnostics.Debug.WriteLine("Item id: " + Item.ID + " retrieved.");
                ScheduleDeletionTime(Item.ID, TTL);
                return Item;
            } else
            {
                //throw error -- probably not great
                //throw new ArgumentException("Id of document to be retrieved was not found", "Id");
                Item = null;
            }
            return Item;
        }

        /*Add a document to documents dictionary*/
        public Document Add(Document Item)
        {
            //Add with throw exception if id already exists, maybe worth try catching here?
            System.Diagnostics.Debug.WriteLine("Item id: " + Item.ID + " added.");
            try
            {
                Documents.Add(Item.ID, Item.Message);
                ScheduleDeletionTime(Item.ID, TTL);
            } catch (ArgumentException)
            {
                Item = null;
            }
            return Item;
        }

        /*Some kind of timer that gets the current time item is added, sets timer to 30 seconds, and waits until it expires
         and deletes it, OR, waits til it is geted, and then renew to 30 seconds*/


        /*for adjustable TTL: have it read the header for 'expires' or something*/

        /*Maybe change this to just the ID?*/
        public Document Remove(Document item)
        {
            if (!(Documents.Remove(item.ID)))
            {
                //item failed to be removed
                throw new ArgumentException("Id of document to be removed was not found", "item.ID");
            }
            return item;
        }

        public int RemoveById(int Id)
        {
            Documents.Remove(Id);
            return Id;
        }

        /*Is creating a new instance better than wiping it all?*/
        public void ClearCache()
        {
            Documents = new Dictionary<int, string>();
        }
    }
}