﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickstartIdentityServer.DBManager.BaseData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickstartIdentityServer.DBManager
{
    /// <summary>
    /// 用户
    /// </summary>
    public class UserEntity: BaseEnity<int>
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Account { set; get; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Name { set; get; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Pwd { set; get; }
        /// <summary>
        /// Gets or sets the provider name.
        /// </summary>
        public string ProviderName { get; set; }
        /// <summary>
        /// 是否是系统所有者
        /// </summary>
        public bool IsSystemAdmin { set; get; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.Property(u=>u.Account).HasMaxLength(50).IsRequired();
            builder.Property(u=>u.Name).HasMaxLength(20);
            builder.Property(u=>u.Pwd).HasMaxLength(32);
            builder.Property(u=>u.ProviderName).HasMaxLength(20);
            builder.HasIndex(u => u.Account);
        }
    }
}
