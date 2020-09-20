using System;
using System.Collections.Generic;
using System.Linq;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        public PropertyMappingService()
        {
            _propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(authorPropertyMapping));
        }

        private Dictionary<string, PropertyMappingValue> authorPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase) {
                { "Id", new PropertyMappingValue(new List<string>(){"Id"})},
                { "MainCategory", new PropertyMappingValue(new List<string>(){"MainCategory"})},
                { "Age", new PropertyMappingValue(new List<string>(){"DateOfBirth"}, true)},
                { "Name", new PropertyMappingValue(new List<string>(){"FirstName", "LastName"})},
            };

        private IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            //get matching mapping
            var matchingMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First()._mappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)}, {typeof(TDestination)}");
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields) {

            var propertyMapping = GetPropertyMapping<TSource, TDestination>();
            if (string.IsNullOrWhiteSpace(fields)) {
                return true;
            }

            var fieldsAfterSplit = fields.Split();

            foreach (var field in fieldsAfterSplit)
            {
                var trimmedField = field.Trim();

                var indexOfFristSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFristSpace == -1 ? trimmedField : trimmedField.Remove(indexOfFristSpace);

                if (!propertyMapping.ContainsKey(propertyName)) { return false; }

            }

            return true;
         }
    }

}
