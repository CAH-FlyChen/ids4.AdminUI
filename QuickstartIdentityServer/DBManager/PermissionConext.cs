﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using QuickstartIdentityServer.DBManager.BaseData;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuickstartIdentityServer.DBManager
{
    public class PermissionConext : DbContext
    {
        IServiceProvider provider { get; set; }
        /// <summary>
        /// 登录人userid
        /// </summary>
        public int UserId { get; set; }


        public DbSet<AppEntity> App { get; set; }
        public DbSet<ModuleEntity> Module { get; set; }
        public DbSet<UserEntity> User { get; set; }
        public DbSet<RoleEntity> Role { get; set; }
        public DbSet<PermissionEntity> Permission { get; set; }
        public DbSet<UserRoleMap> UserRoleMap { get; set; }
        public DbSet<RolePermissionMap> RolePermissionMap { get; set; }
        public DbSet<RoleAppAdmin> RoleAppAdmin { get; set; }
        public DbSet<RoleModuleMap> RoleModuleMap { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="options"></param>
        public PermissionConext(DbContextOptions<PermissionConext> options) : base(options)
        {
            var optionsExtensions = options.Extensions;
            foreach (IDbContextOptionsExtension dbContextOptionsExtension in optionsExtensions)
            {
                if (dbContextOptionsExtension is CoreOptionsExtension coreOptionsExtension)
                {
                    provider = coreOptionsExtension.ApplicationServiceProvider;
                }
            }
        }
        /// <summary>
        /// CONCAT
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string Concat(string a, string b)
        {
            throw new Exception();
        }
        public static string Concat(string a, string b, string c)
        {
            throw new Exception();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AppConfiguration());
            modelBuilder.ApplyConfiguration(new ModuleConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleAppConfiguration());
            modelBuilder.ApplyConfiguration(new RoleModuleMapConfiguration());
            modelBuilder.ApplyConfiguration(new RolePermissionMapConfiguration());
            modelBuilder.HasDbFunction(() => Concat(default(string), default(string))).HasName("CONCAT");
            modelBuilder.HasDbFunction(() => Concat(default(string), default(string), default(string))).HasName("CONCAT");

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var et = entityType.ClrType;
                if (typeof(IDeleteOperation).IsAssignableFrom(et))
                {
                   var parameter = Expression.Parameter(et, "e");
                   var body = Expression.Equal(Expression.Call(typeof(EF), nameof(EF.Property), new[] { typeof(bool) }, parameter, Expression.Constant(EntityConst.IsDeleted)), Expression.Constant(false));
                   modelBuilder.Entity(et).HasQueryFilter(Expression.Lambda(body, parameter));
                }
            }
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            PrivateCheckDoWork();
            //this.Database.CurrentTransaction
            //this.Database.GetDbConnection().ConnectionString = config.sql4;
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            //this.Database.GetDbConnection().ConnectionString = config.sql8;
            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            PrivateCheckDoWork();
            //this.Database.GetDbConnection().ConnectionString = config.sql4;
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            //this.Database.GetDbConnection().ConnectionString = config.sql8;
            return result;
        }

        private void PrivateCheckDoWork()
        {
            var entries = ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged).ToArray();
            if (entries.Length == 0) return;
            SetUserInfo();//设置当前用户
            var tsnow = GetUnixTimestamp();
            foreach (var entry in entries)
            {
                if (entry.Entity is IUpdateOperation bd)
                {
                    bd.UpdateId = UserId;
                    bd.UpdateTime = DateTime.Now;
                }
                if (entry.Entity is ITimeStamp ts)
                {
                    ts.Ts = tsnow;
                }
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (entry.Entity is ICreateOperation bdadd)
                        {
                            bdadd.CreateId = UserId;
                            bdadd.CreateTime = DateTime.Now;
                        }
                        if (entry.Entity is BaseKey<string> add)
                        {
                            if(string.IsNullOrEmpty(add.Id)) add.Id = Guid.NewGuid().ToString();
                        }
                        break;
                    case EntityState.Deleted:
                        if (entry.Entity is IDeleteOperation del)
                        {
                            entry.State = EntityState.Unchanged;
                            del.IsDeleted = true;
                        }
                        break;
                    case EntityState.Modified:
                        break;
                }
            }
        }

        /// <summary>
        /// 设置当前用户
        /// </summary>
        /// <param name="serviceProvider"></param>
        private void SetUserInfo()
        {
            var httpContextAccessor = provider.GetService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor?.HttpContext;
            if (httpContext == null) return;
            var subid = httpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (subid != null)
            {
                int.TryParse(subid, out int userid);
                UserId = userid;
            }
        }
        private long GetUnixTimestamp()
        {
            var utcTime = DateTime.UtcNow;
            var dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (utcTime.Ticks - dt1970.Ticks) / 10000000;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="work"></param>
        /// <returns></returns>
        public async Task New<T>(Func<PermissionConext, Task> work)
        {
            using (var scope = provider.CreateScope())
            {
                var p = scope.ServiceProvider.GetService<PermissionConext>();
                await work(p);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="work"></param>
        /// <returns></returns>
        public async Task<TResult> New<TResult>(Func<PermissionConext,Task<TResult>> work)
        {
            using (var scope = provider.CreateScope())
            {
                var p = scope.ServiceProvider.GetService<PermissionConext>();
                var result = await work(p);
                return result;
            }
        }
    }
}
