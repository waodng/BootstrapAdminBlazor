﻿using Longbow.Data;
using Longbow.Web.Mvc;
using PetaPoco;
using System;

namespace Bootstrap.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// 查询所有日志信息
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static Page<Log> Retrieves(PaginationOption op, DateTime? startTime, DateTime? endTime, string opType) => DbContextManager.Create<Log>().Retrieves(op, startTime, endTime, opType);

        /// <summary>
        /// 保存新增的日志信息
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static bool Save(Log log)
        {
            if (log.Id == string.Empty) log.Id = null;
            log.LogTime = DateTime.Now;
            return DbContextManager.Create<Log>().Save(log);
        }
    }
}
