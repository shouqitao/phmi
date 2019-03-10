﻿using System;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using Npgsql;
using PHmiClient.Utils;
using PHmiModel.Entities;
using PHmiModel.Interfaces;

namespace PHmiModel {
    public class PHmiModelContext : DbContext, IModelContext {
        public PHmiModelContext(string connectionString)
            : base(new NpgsqlConnection(connectionString), true) {
            Database.SetInitializer<PHmiModelContext>(null);
        }

        public DbSet<AlarmCategory> AlarmCategories { get; set; }

        public DbSet<AlarmTag> AlarmTags { get; set; }

        public DbSet<DigTag> DigTags { get; set; }

        public DbSet<IoDevice> IoDevices { get; set; }

        public DbSet<Log> Logs { get; set; }

        public DbSet<NumTagType> NumTagTypes { get; set; }

        public DbSet<NumTag> NumTags { get; set; }

        public DbSet<Settings> Settings { get; set; }

        public DbSet<TrendCategory> TrendCategories { get; set; }

        public DbSet<TrendTag> TrendTags { get; set; }

        public DbSet<User> Users { get; set; }

        public void Save() {
            SaveChanges();
            HasChanges = false;
        }

        public void AddTo<T>(T entity) {
            if (typeof(T) == typeof(AlarmCategory))
                AlarmCategories.Add(entity as AlarmCategory);
            else if (typeof(T) == typeof(AlarmTag))
                AlarmTags.Add(entity as AlarmTag);
            else if (typeof(T) == typeof(DigTag))
                DigTags.Add(entity as DigTag);
            else if (typeof(T) == typeof(IoDevice))
                IoDevices.Add(entity as IoDevice);
            else if (typeof(T) == typeof(Log))
                Logs.Add(entity as Log);
            else if (typeof(T) == typeof(NumTag))
                NumTags.Add(entity as NumTag);
            else if (typeof(T) == typeof(TrendCategory))
                TrendCategories.Add(entity as TrendCategory);
            else if (typeof(T) == typeof(TrendTag))
                TrendTags.Add(entity as TrendTag);
            else if (typeof(T) == typeof(User))
                Users.Add(entity as User);
            else
                throw new NotImplementedException();
        }

        public void DeleteObject(object entity) {
            DbEntityEntry entry = Entry(entity);
            entry.State = entry.State == EntityState.Added ? EntityState.Detached : EntityState.Deleted;
        }

        public IQueryable<T> Get<T>() {
            if (typeof(T) == typeof(AlarmCategory)) return (IQueryable<T>) AlarmCategories;
            if (typeof(T) == typeof(IoDevice)) return (IQueryable<T>) IoDevices;
            if (typeof(T) == typeof(DigTag)) return (IQueryable<T>) DigTags;
            if (typeof(T) == typeof(Log)) return (IQueryable<T>) Logs;
            if (typeof(T) == typeof(NumTag)) return (IQueryable<T>) NumTags;
            if (typeof(T) == typeof(NumTagType)) return (IQueryable<T>) NumTagTypes;
            if (typeof(T) == typeof(Settings)) return (IQueryable<T>) Settings;
            if (typeof(T) == typeof(TrendCategory)) return (IQueryable<T>) TrendCategories;
            if (typeof(T) == typeof(User)) return (IQueryable<T>) Users;
            throw new NotSupportedException();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder
                .Entity<AlarmCategory>()
                .HasMany(e => e.AlarmTags)
                .WithRequired(e => e.AlarmCategory)
                .HasForeignKey(e => e.RefCategory);

            modelBuilder
                .Entity<DigTag>()
                .HasMany(e => e.AlarmTags)
                .WithRequired(e => e.DigTag)
                .HasForeignKey(e => e.RefDigTag);

            modelBuilder
                .Entity<DigTag>()
                .HasMany(e => e.TrendTags)
                .WithOptional(e => e.Trigger)
                .HasForeignKey(e => e.RefTrigger);

            modelBuilder
                .Entity<IoDevice>()
                .HasMany(e => e.DigTags)
                .WithRequired(e => e.IoDevice)
                .HasForeignKey(e => e.RefIoDevice);

            modelBuilder
                .Entity<IoDevice>()
                .HasMany(e => e.NumTags)
                .WithRequired(e => e.IoDevice)
                .HasForeignKey(e => e.RefIoDevice);

            modelBuilder
                .Entity<NumTagType>()
                .HasMany(e => e.NumTags)
                .WithRequired(e => e.NumTagType)
                .HasForeignKey(e => e.RefTagType);

            modelBuilder
                .Entity<NumTag>()
                .HasMany(e => e.TrendTags)
                .WithRequired(e => e.NumTag)
                .HasForeignKey(e => e.RefNumTag);

            modelBuilder
                .Entity<TrendCategory>()
                .HasMany(e => e.TrendTags)
                .WithRequired(e => e.TrendCategory)
                .HasForeignKey(e => e.RefCategory);

            base.OnModelCreating(modelBuilder);
        }

        private static Stream GetEmbeddedResource(string resource) {
            Assembly assembly = Assembly.GetAssembly(typeof(PHmiModelContext));
            return assembly.GetManifestResourceStream(resource);
        }

        public static Stream GetPHmiScriptStream() {
            return GetEmbeddedResource("PHmiModel.Schemes.PHmi.sql");
        }

        public static Stream GetPHmiScriptRowsStream() {
            return GetEmbeddedResource("PHmiModel.Schemes.PHmiRows.sql");
        }

        public void StartTrackingChanges() {
            var timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs eventArgs) {
            var timer = (DispatcherTimer) sender;
            timer.Stop();
            try {
                HasChanges = ChangeTracker.Entries().Any(e => e.State == EntityState.Added
                                                              || e.State == EntityState.Modified
                                                              || e.State == EntityState.Deleted);
                timer.Start();
            } catch (InvalidOperationException) { }
        }

        #region HasChanges

        private bool _hasChanges;

        public bool HasChanges {
            get { return _hasChanges; }
            private set {
                if (_hasChanges == value)
                    return;
                _hasChanges = value;
                OnPropertyChanged(PropertyHelper.GetPropertyName(this, e => e.HasChanges));
            }
        }

        #endregion HasChanges

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion PropertyChanged
    }
}