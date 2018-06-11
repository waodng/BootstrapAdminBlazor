﻿using Longbow;
using Longbow.Cache;
using Longbow.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;

namespace Bootstrap.DataAccess
{
    /// <summary>
    /// 
    /// </summary>
    public static class NotificationHelper
    {
        /// <summary>
        /// 
        /// </summary>
        internal const string RetrieveNotificationsDataKey = "NotificationHelper-RetrieveNotifications";
        /// <summary>
        /// 
        /// </summary>
        public static ConcurrentBag<MessageBody> MessagePool { get; } = new ConcurrentBag<MessageBody>();
        /// <summary>
        /// 新用户注册的通知的面板显示
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Notification> RetrieveNotifications()
        {
            var notifies = CacheManager.GetOrAdd(RetrieveNotificationsDataKey, key =>
            {
                string sql = "select * from Notifications";
                List<Notification> notifications = new List<Notification>();
                DbCommand cmd = DBAccessManager.SqlDBAccess.CreateCommand(CommandType.Text, sql);
                try
                {
                    using (DbDataReader reader = DBAccessManager.SqlDBAccess.ExecuteReader(cmd))
                    {
                        while (reader.Read())
                        {
                            notifications.Add(new Notification()
                            {
                                Id = (int)reader[0],
                                Category = (string)reader[1],
                                Title = (string)reader[2],
                                Content = (string)reader[3],
                                RegisterTime = (DateTime)reader[4],
                                ProcessTime = LgbConvert.ReadValue(reader[5], DateTime.MinValue),
                                ProcessBy = LgbConvert.ReadValue(reader[6], string.Empty),
                                ProcessResult = LgbConvert.ReadValue(reader[7], string.Empty),
                                Status = (string)reader[8]
                            });
                        }
                    }
                }
                catch (Exception ex) { ExceptionManager.Publish(ex); }
                return notifications;

            });
            notifies.AsParallel().ForAll(n =>
            {
                var ts = DateTime.Now - n.RegisterTime;
                if (ts.TotalMinutes < 5) n.Period = "刚刚";
                else if (ts.Days > 0) n.Period = string.Format("{0}天", ts.Days);
                else if (ts.Hours > 0) n.Period = string.Format("{0}小时", ts.Hours);
                else if (ts.Minutes > 0) n.Period = string.Format("{0}分钟", ts.Minutes);
            });
            return notifies.OrderByDescending(n => n.RegisterTime);
        }
        /// <summary>
        /// 点击某一行用户注册通知的处理成功操作
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool ProcessRegisterUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            bool ret = false;
            try
            {
                using (DbCommand cmd = DBAccessManager.SqlDBAccess.CreateCommand(CommandType.StoredProcedure, "Proc_ProcessRegisterUser"))
                {
                    cmd.Parameters.Add(DBAccessManager.SqlDBAccess.CreateParameter("@id", id));
                    DBAccessManager.SqlDBAccess.ExecuteNonQuery(cmd);
                }
                CacheCleanUtility.ClearCache(notifyIds: id);
                ret = true;
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);
            }
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="noti"></param>
        /// <returns></returns>
        public static bool SaveNotification(Notification noti)
        {
            if (string.IsNullOrEmpty(noti.Title) || string.IsNullOrEmpty(noti.Content)) return false;
            bool ret = false;
            try
            {
                using (DbCommand cmd = DBAccessManager.SqlDBAccess.CreateCommand(CommandType.Text, "Insert into Notifications (Category, Title, Content, RegisterTime) values (N'2', @Title, @Content, GetDate())"))
                {
                    cmd.Parameters.Add(DBAccessManager.SqlDBAccess.CreateParameter("@Title", noti.Title));
                    cmd.Parameters.Add(DBAccessManager.SqlDBAccess.CreateParameter("@Content", noti.Content));
                    DBAccessManager.SqlDBAccess.ExecuteNonQuery(cmd);
                }
                CacheCleanUtility.ClearCache(notifyIds: string.Empty);
                ret = true;
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);
            }
            return ret;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class MessageBody : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public MessageBody()
        {
            timer = new Timer(state =>
            {
                var msg = this;
                NotificationHelper.MessagePool.TryTake(out msg);
            }, null, 5000, Timeout.Infinite);
        }
        private Timer timer = null;
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}-{1}", Category, Message);
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
