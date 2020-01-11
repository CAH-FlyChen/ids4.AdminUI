﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuickstartIdentityServer.CommonDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QuickstartIdentityServer.Filters
{
    public class WebApiDoResultAttribute : IResultFilter
    {
        public WebApiDoResultAttribute()
        {
        }
        public void OnResultExecuted(ResultExecutedContext context)
        {
            //throw new NotImplementedException();
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is EmptyResult)
            {
                context.Result = new JsonResult(new BaseResult());
            }
            else if (context.Result is ObjectResult)
            {
                var objresult = context.Result as ObjectResult;
                if ((objresult.Value as BaseResult) != null)
                    return;
                if ((objresult.Value as HttpResponseMessage) != null)
                    return;
                DataResult<object> result = new DataResult<object>(objresult.Value);
                if (objresult.StatusCode.HasValue && objresult.StatusCode != 200) result.Result = false;
                context.Result = new JsonResult(result);
            }
            //throw new NotImplementedException();
        }
    }
}
