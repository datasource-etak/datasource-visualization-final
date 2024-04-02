using BlazorDatasource.Shared.Domain;
using BlazorDatasource.Shared.Mapping;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using BlazorDatasource.Shared.Domain.Custom;

namespace BlazorDatasource.Shared
{
    /// <summary>
    /// Represents base object context
    /// </summary>
    public partial class EntityObjectContext : DbContext, IDbContext
    {
        #region Ctor

        public EntityObjectContext(DbContextOptions<EntityObjectContext> options) : base(options)
        {

        }

        #endregion
        public DbSet<SearchQuery>? SearchQueries { get; set; }
        public DbSet<Source>? Sources { get; set; }
        public DbSet<FavoriteDataset>? FavoriteDatasets { get; set; }
        public DbSet<SharedDataset>? SharedDatasets { get; set; }

        #region Utilities

        /// <summary>
        /// Further configuration the model
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //dynamically load all entity type configurations
            var typeConfigurations = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
                (type.BaseType?.IsGenericType ?? false)
                    && type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));

            foreach (var typeConfiguration in typeConfigurations)
            {
                var configuration = Activator.CreateInstance(typeConfiguration) as IMappingConfiguration;
                configuration?.ApplyConfiguration(modelBuilder);
            }

            base.OnModelCreating(modelBuilder);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a DbSet that can be used to query and save instances of entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>A set for the given entity type</returns>
        public virtual new DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        /// <summary>
        /// Detach an entity from the context
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        public virtual void Detach<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entityEntry = Entry(entity);
            if (entityEntry == null)
                return;

            //set the entity is not being tracked by the context
            entityEntry.State = EntityState.Detached;
        }

        #endregion
    }
}
