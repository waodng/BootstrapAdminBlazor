﻿using Bootstrap.DataAccess;
using Longbow.Web.Mvc;
using System;
using System.Linq;

namespace Bootstrap.Admin.Query
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryExceptionOption : PaginationOption
    {
        /// <summary>
        /// 
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public QueryData<object> RetrieveData()
        {
            var data = ExceptionsHelper.RetrieveExceptions();
            if (StartTime > DateTime.MinValue)
            {
                data = data.Where(t => t.LogTime > StartTime.Value);
            }
            if (EndTime > DateTime.MinValue)
            {
                data = data.Where(t => t.LogTime < EndTime.Value.AddDays(1));
            }
            var ret = new QueryData<object>();
            ret.total = data.Count();
            switch (Sort)
            {
                case "ErrorPage":
                    data = Order == "asc" ? data.OrderBy(t => t.ErrorPage) : data.OrderByDescending(t => t.ErrorPage);
                    break;
                case "UserID":
                    data = Order == "asc" ? data.OrderBy(t => t.UserId) : data.OrderByDescending(t => t.UserId);
                    break;
                case "UserIp":
                    data = Order == "asc" ? data.OrderBy(t => t.UserIp) : data.OrderByDescending(t => t.UserIp);
                    break;
                case "LogTime":
                    data = Order == "asc" ? data.OrderBy(t => t.LogTime) : data.OrderByDescending(t => t.LogTime);
                    break;
                default:
                    break;
            }
            ret.rows = data.Skip(Offset).Take(Limit).Select(ex => new { ex.UserId, ex.UserIp, ex.LogTime, ex.Message, ex.ErrorPage, ex.ExceptionType });
            return ret;
        }
    }
}
