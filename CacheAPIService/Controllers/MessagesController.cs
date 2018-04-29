﻿using System;
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
        public HttpResponseMessage GetDocument(int id)
        {
            int timeToLive = 30;
            System.Net.Http.Headers.HttpRequestHeaders headers = Request.Headers;
            //Search for custom header in HttpRequestHeaders to specify TTL (assumes header is of format: "Custom-Ttl:[digits]")
            if (headers.Contains("Custom-Ttl"))
            {
                String customTtl = headers.GetValues("Custom-Ttl").First();
                timeToLive = Convert.ToInt32(customTtl);
            }

            Document item = repository.Get(id, timeToLive);
            HttpResponseMessage response;
            if (item == null)
            {
                response = Request.CreateResponse<String>(HttpStatusCode.NotFound, "Resource not found");
            } else
            {
                response = Request.CreateResponse<Document>(HttpStatusCode.OK, item);
            }
            return response;
        }

        /*WebApi (ApiController) that this is derived from uses media formatter to serialize model in response body when using HttpResponseMessage and a model is added to it*/
        public HttpResponseMessage PostDocument(Document item)
        {
            HttpResponseMessage response;
            int timeToLive = 30;
            System.Net.Http.Headers.HttpRequestHeaders headers = Request.Headers;

            //Search for custom header in HttpRequestHeaders to specify TTL (assumes header is of format: "Custom-Ttl:[digits]")
            if (headers.Contains("Custom-Ttl"))
            {
                String customTtl = headers.GetValues("Custom-Ttl").First();
                timeToLive = Convert.ToInt32(customTtl);
            }

            item = repository.Add(item, timeToLive);
            if (item != null)
            {
                response = Request.CreateResponse<Document>(HttpStatusCode.Created, item);
                string uri = Url.Link("DefaultApi", new { id = item.ID }); //When creating a new resource, should include resource location in location of header
                response.Headers.Location = new Uri(uri);
            } else
            {
                response = Request.CreateResponse<String>(HttpStatusCode.BadRequest, "Bad Request: A document with the same identifier already exists in cache.");
            }
            return response;
        }

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
    }
}
