using System;
using System.Collections.Generic;

namespace CourseLibrary.API.Services
{
    public class PropertyMapping<TSource, TDestination> : IPropertyMapping
    {
        public Dictionary<string, PropertyMappingValue> _mappingDictionary { get; private set; }

        public PropertyMapping(Dictionary<string, PropertyMappingValue> mappingDictonary)
        {
            _mappingDictionary = mappingDictonary ?? throw new ArgumentNullException(nameof(mappingDictonary));
        }
    }
}
