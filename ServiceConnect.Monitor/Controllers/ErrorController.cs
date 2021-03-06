﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using ServiceConnect.Monitor.Interfaces;
using ServiceConnect.Monitor.Models;

namespace ServiceConnect.Monitor.Controllers
{
    public class ErrorController : ApiController
    {
        private readonly IErrorRepository _errorRepository;
        private readonly IServiceRepository _serviceRepository;

        public ErrorController(IErrorRepository errorRepository, IServiceRepository serviceRepository)
        {
            _errorRepository = errorRepository;
            _serviceRepository = serviceRepository;
        }

        [AcceptVerbs("GET")]
        [Route("errors")]
        public async Task<IList<Error>> FindErrors(Guid correlationId)
        {
            return await _errorRepository.Find(correlationId);
        }

        [AcceptVerbs("GET")]
        [Route("errors")]
        public async Task<IList<Error>> FindErrors(DateTime from, DateTime to, string tags = null)
        {
            List<string> tagList = null;
            if (!string.IsNullOrEmpty(tags))
                tagList = tags.Split(',').ToList();

            var errors = await _errorRepository.Find(from, to);

            if (tagList != null && tagList.Count > 0)
            {
                var results = new List<Error>();

                var services = await _serviceRepository.Find();

                foreach (Error error in errors)
                {
                    bool match = services.Any(service => (error.SourceAddress == service.Name || error.DestinationAddress == service.Name) && service.Tags != null && service.Tags.Any(tagList.Contains));
                    if (match)
                        results.Add(error);
                }

                return results;
            }
            
            return errors;
        }

        [AcceptVerbs("GET")]
        [Route("error/{id}")]
        public async Task<Error> Get(string id)
        {
            return await _errorRepository.Get(new Guid(id));
        }

        [AcceptVerbs("POST")]
        [Route("errors/retry")]
        public async Task<bool> RetryAll(List<Error> errors) 
        {
            foreach (var error in errors)
            {
                var env = Globals.Environments.FirstOrDefault(x => x.Server == error.Server);

                error.Headers.Remove("TimeReceived");
                error.Headers.Remove("DestinationMachine");
                error.Headers.Remove("DestinationAddress");
                error.Headers.Remove("RetryCount");
                error.Headers.Remove("Exception");

                if (env != null)
                {
                    env.Producer.Send(error.DestinationAddress, error.Body, error.Headers);

                    await _errorRepository.Remove(error.Id);
                }
            }

        	return true;
        }
    }
}
