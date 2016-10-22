﻿using Longbow.Caching;
using Longbow.Caching.Configuration;
using Longbow.ExceptionManagement;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;

namespace Bootstrap.DataAccess
{
    public static class UserHelper
    {
        private const string UserDataKey = "UserData-CodeUserHelper";

        /// <summary>
        /// 查询所有用户
        /// </summary>
        /// <param name="pIds"></param>
        /// <returns></returns>
        public static IEnumerable<User> RetrieveUsers(string tId = null)
        {
            string sql = "select * from Users";
            var ret = CacheManager.GetOrAdd(UserDataKey, CacheSection.RetrieveIntervalByKey(UserDataKey), key =>
            {
                List<User> Users = new List<User>();
                DbCommand cmd = DBAccessManager.SqlDBAccess.CreateCommand(CommandType.Text, sql);
                try
                {
                    using (DbDataReader reader = DBAccessManager.SqlDBAccess.ExecuteReader(cmd))
                    {
                        while (reader.Read())
                        {
                            Users.Add(new User()
                            {
                                ID = (int)reader[0],
                                UserName = (string)reader[1],
                                Password = (string)reader[2],
                                PassSalt = (string)reader[3],
                            });
                        }
                    }
                }
                catch (Exception ex) { ExceptionManager.Publish(ex); }
                return Users;
            }, CacheSection.RetrieveDescByKey(UserDataKey));
            return string.IsNullOrEmpty(tId) ? ret : ret.Where(t => tId.Equals(t.ID.ToString(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="ids"></param>
        public static void DeleteUser(string ids)
        {
            if (string.IsNullOrEmpty(ids) || ids.Contains("'")) return;
            string sql = string.Format(CultureInfo.InvariantCulture, "Delete from Users where ID in ({0})", ids);
            using (DbCommand cmd = DBAccessManager.SqlDBAccess.CreateCommand(CommandType.Text, sql))
            {
                DBAccessManager.SqlDBAccess.ExecuteNonQuery(cmd);
                ClearCache();
            }
        }

        /// <summary>
        /// 保存新建/更新的用户信息
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool SaveUser(User p)
        {
            if (p == null) throw new ArgumentNullException("p");
            bool ret = false;
            if (p.UserName.Length > 50) p.UserName.Substring(0, 50);
            if (p.Password.Length > 50) p.Password.Substring(0, 50);
            if (p.PassSalt.Length > 50) p.PassSalt.Substring(0, 50);
            string sql = p.ID == 0 ?
                "Insert Into Users (UserName, Password, PassSalt) Values (@UserName, @Password, @PassSalt)" :
                "Update Users set UserName = @UserName, Password = @Password, PassSalt = @PassSalt where ID = @ID";
            try
            {
                using (DbCommand cmd = DBAccessManager.SqlDBAccess.CreateCommand(CommandType.Text, sql))
                {
                    cmd.Parameters.Add(DBAccessManager.SqlDBAccess.CreateParameter("@ID", p.ID, ParameterDirection.Input));
                    cmd.Parameters.Add(DBAccessManager.SqlDBAccess.CreateParameter("@UserName", p.UserName, ParameterDirection.Input));
                    cmd.Parameters.Add(DBAccessManager.SqlDBAccess.CreateParameter("@Password", p.Password, ParameterDirection.Input));
                    cmd.Parameters.Add(DBAccessManager.SqlDBAccess.CreateParameter("@PassSalt", p.PassSalt, ParameterDirection.Input));
                    DBAccessManager.SqlDBAccess.ExecuteNonQuery(cmd);
                }
                ret = true;
                ClearCache();
            }
            catch (DbException ex)
            {
                ExceptionManager.Publish(ex);
            }
            return ret;
        }


        // 更新缓存
        private static void ClearCache()
        {
            CacheManager.Clear(key => key.Contains("TerminalData-"));
        }
    }
}
