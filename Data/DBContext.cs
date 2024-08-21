using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using BeanScene.Models;
using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace BeanScene.Data
{
    public class BSDBContext : IdentityDbContext<ApplicationUser>
    {
        public BSDBContext(DbContextOptions<BSDBContext> options)
            : base(options)
        {
        }
        //link models here
        public DbSet<Reservation> Reservation { get; set; } = default!;
        public DbSet<User> User { get; set; } = default!;
        public DbSet<Sitting> Sitting { get; set; } = default!;
        public DbSet<Table> Table { get; set; } = default!;

        //always make sure phone and email are unique
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Phone)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
            //TableName is 'TableNo' and 'Area' merged -- and it has to be done at the dbcontext level, otherwise code is riddled with errors
            modelBuilder.Entity<Table>()
        .Property(t => t.TableName)
        .HasComputedColumnSql(@"
            CASE Area
                WHEN 0 THEN 'M'
                WHEN 1 THEN 'O'
                WHEN 2 THEN 'B'
            END + CAST(TableNo AS VARCHAR)");
            //default sittings, just for ease of use.
            modelBuilder.Entity<Sitting>().HasData(
                new Sitting
                {
                    SittingId = 1,
                    SittingType = sittingtype.Breakfast,
                    StartTime = new DateTime(1, 1, 1, 8, 0, 0),  // 8:00 AM
                    EndTime = new DateTime(1, 1, 1, 12, 0, 0),   // 12:00 PM
                    SittingStatus = sittingstatus.Open
                },
                new Sitting
                {
                    SittingId = 2,
                    SittingType = sittingtype.Lunch,
                    StartTime = new DateTime(1, 1, 1, 12, 0, 0), // 12:00 PM
                    EndTime = new DateTime(1, 1, 1, 16, 0, 0),   // 4:00 PM
                    SittingStatus = sittingstatus.Open
                },
                new Sitting
                {
                    SittingId = 3,
                    SittingType = sittingtype.Dinner,
                    StartTime = new DateTime(1, 1, 1, 16, 0, 0), // 4:00 PM
                    EndTime = new DateTime(1, 1, 1, 18, 0, 0),   // 6:00 PM
                    SittingStatus = sittingstatus.Open
                }
            );
        }
        //user roleview model is just where you 'view' roles
        public DbSet<BeanScene.Models.UserRoleViewModel> UserRoleViewModel { get; set; } = default!;
    }
}


