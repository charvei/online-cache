using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Cache;
using System.Text.RegularExpressions;
using CacheAPIService.Models;


namespace CacheAPIService.Controllers
{
    /**
     * Controller for handling Http requests and connecting with model / business logic
     */ 
    public class MessagesController : ApiController
    {
        //Why static?
        //Why readonly?
        static readonly IDocumentRepository repository = new DocumentRepository(); //hold an instance of movie repository
        /*
         * In this controller we will use this instance of the repository to implement the business logic
           rather than directly in the controller. Do it outside the controller (action methods) SLIM.
         - Not a good idea to implement business logic in the action method
         - Action method's responsibility is to say WHERE we implement the business logic rather than implement it itself
         - Not good practice to do it inside controller in "large company"
         */

        /**
         * Function to be called on Http GET request (api/messages/[id]). Attempts to retrieve a document from the repository
         * with id corresponding to id parameter. If document can be succesfully retrieved, the document is returned to client
         * with 'OK' response code. Otherwise a resource not found message (status code: 404) is sent.
         * 
         * Args: 
         *  int
         *      - The id of document requested in Http GET request
         * 
         * Returns: 
         *  HttpResponseMessage
         */
        public HttpResponseMessage GetDocument(int id)
        {
            Document item = repository.Get(id);
            HttpResponseMessage response;
            if (item == null)
            {
                response = Request.CreateResponse<String>(HttpStatusCode.NotFound, "Resource not found");
            } else
            {
                repository.ScheduleDeletionTime(item.ID, GetTimeToLive());
                response = Request.CreateResponse<Document>(HttpStatusCode.OK, item);
            }
            return response;
        }

        /*WebApi (ApiController) that this is derived from uses media formatter to serialize model in response body when using HttpResponseMessage and a model is added to it*/
        /**
         * Function to be called on Http POST request (api/messages). Attempts to add document to repository. If successfully created,
         * an Http Created message (status code: 201) with a copy of the created document and, in the response header, the location of
         * newly created file is returned to client. If 
         * 
         * Args: 
         *  Document item
         *      - The Document to be created
         * 
         * Returns:
         *  HttpResponseMessage
         */
        public HttpResponseMessage PostDocument(Document item)
        {
            HttpResponseMessage response;
            item = repository.Add(item);
            if (item != null)
            {
                response = Request.CreateResponse<Document>(HttpStatusCode.Created, item);
                string uri = Url.Link("DefaultApi", new { id = item.ID }); 
                response.Headers.Location = new Uri(uri);
                repository.ScheduleDeletionTime(item.ID, GetTimeToLive());
            } else
            {
                response = Request.CreateResponse<String>(HttpStatusCode.BadRequest, 
                        "Bad Request: specified document was not able to be created, a document with the same id may already exist");
                //not sure about this -- would like be more confident in the HttpStatusCode to return. Ie maybe its for another reason and not a bad request.
            }
            return response;
        }

        /**
         * Function to be called on Http DELETE request (api/messages). Calls on repository function to clear cache. Returns Http
         * OK response (status code: 200) if successful. In case of an error occuring during this process, returns Http Internal 
         * Server Error response (status code 500).
         * 
         * Returns:
         *  HttpResponseMessage
         */
        public HttpResponseMessage DeleteCache()
        {
            try
            {
                repository.ClearCache();
                var response = Request.CreateResponse<String>(HttpStatusCode.OK, "OK: Cache cleared.");
                return response;
            } catch
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError); //generic and may not be most appropriate error
            }
        }

        /**
         * Helper function. Returns Time To Live value for a document according to either the value in seconds corresponding to
         * custom header "Custom-Ttl" or, if no such header exists, a default value of 30 seconds.
         * 
         * returns: 
         *  int
         *      - Time To Live value to be passed to repository .Add and .Get function parameters.
         */
        private int GetTimeToLive()
        {
            int TimeToLive = 30;
            System.Net.Http.Headers.HttpRequestHeaders headers = Request.Headers;
            //Search for custom header in HttpRequestHeaders to specify TTL (assumes header is of format: "Custom-Ttl:[digits]")
            if (headers.Contains("Custom-Ttl"))
            {
                String customTtl = headers.GetValues("Custom-Ttl").First();
                TimeToLive = Convert.ToInt32(customTtl);
            }
            return TimeToLive;
        }
    }
}
