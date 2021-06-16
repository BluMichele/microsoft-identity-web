﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TodoListService
{
    /// <summary>
    /// This is a naive implementation of a cache to save user assertions used
    /// to get an OBO token, in the case of long running processes which want to 
    /// call OBO and get the token refreshed.
    /// 
    /// TODO: document this better.
    /// Could use a IDistributed cache
    /// Needs to add 
    ///  services.AddSingleton<ILongRunningProcessAssertionCache, LongRunningProcessAssertionCache>();
    /// in the Startup.cs
    /// </summary>
    public class LongRunningProcessContextFactory : ILongRunningProcessContextFactory
    {
        /// <summary>
        /// Get a key associated with the current incoming token
        /// </summary>
        /// <param name="httpContext">Http context in which the controller action is running</param>
        /// <returns>A unique string repesenting the incoming token to the web API. This
        /// key will be used in the future to retrieve the incoming token even if it has expired therefore
        /// enabling getting an OBO token.</returns>
        public string CreateKey(HttpContext httpContext)
        {
            JwtSecurityToken token = httpContext.Items["JwtSecurityTokenUsedToCallWebAPI"] as JwtSecurityToken;
            string key = Guid.NewGuid().ToString();
            privateAssertionOfKey.Add(key, token);
            return key;
        }

        /// <summary>
        /// Get a long running process context from a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public LongRunningProcessContext UseKey(HttpContext httpContext, string key)
        {
            JwtSecurityToken token = privateAssertionOfKey[key];
            return new LongRunningProcessContext(httpContext, token);
        }

        private IDictionary<string, JwtSecurityToken> privateAssertionOfKey = new Dictionary<string, JwtSecurityToken>();
    }
}
