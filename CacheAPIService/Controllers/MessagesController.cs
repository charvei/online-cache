﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CacheAPIService.Models;   //say that we're using Models


namespace CacheAPIService.Controllers
{
    //DocumentController is derived from ApiController
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

        /*api/document/id*/
        public Document GetDocument(int id)
        {
            //Probably good to use HttpResponseMessage as the return type and follow the below example
            Document item = repository.Get(id);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return item;
        }

        /*WebApi (ApiController) that this is derived from uses media formatter to serialize model in response body when using HttpResponseMessage and a model is added to it*/
        public HttpResponseMessage PostDocument(Document item)
        {
            item = repository.Add(item);
            var response = Request.CreateResponse<Document>(HttpStatusCode.Created, item);
            string uri = Url.Link("DefaultApi", new { id = item.ID }); //When creating a new resource, should include resource location in location of header
            response.Headers.Location = new Uri(uri);
            return response;
        }

        public HttpResponseMessage DeleteCache()
        {
            try
            {
                repository.ClearCache();
                var response = Request.CreateResponse<String>(HttpStatusCode.OK, "Cache cleared.");
                return response;
            } catch
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError); //generic and may not be most appropriate error
            }
        }
    }
}