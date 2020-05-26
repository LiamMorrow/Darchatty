using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Darchatty.Data
{
    public static class ContextExtensions
    {
        public static PropertyBuilder UsePostgresNamingConventions(this PropertyBuilder propertyBuilder, string prefix)
        {
            propertyBuilder.HasColumnName(prefix + "_" + propertyBuilder.Metadata.Name.ToSnakeCase());
            return propertyBuilder;
        }

        public static PropertyBuilder UsePostgresNamingConventions(this PropertyBuilder propertyBuilder)
        {
            propertyBuilder.HasColumnName(propertyBuilder.Metadata.GetFieldName().ToSnakeCase());
            return propertyBuilder;
        }

        public static ModelBuilder UsePostgresNamingConventions(this ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (entity.IsOwned())
                {
                    continue;
                }

                // Replace table names
                entity.SetTableName(entity.GetTableName().ToSnakeCase());

                // Replace column names
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName().ToSnakeCase());
                }

                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName().ToSnakeCase());
                }

                foreach (var key in entity.GetForeignKeys())
                {
                    key.SetConstraintName(key.GetConstraintName().ToSnakeCase());
                }

                foreach (var index in entity.GetIndexes())
                {
                    index.SetName(index.GetName().ToSnakeCase());
                }
            }

            return modelBuilder;
        }

        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
    }
}
