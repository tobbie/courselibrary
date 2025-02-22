﻿using System;
using System.Collections.Generic;
using System.Linq;
using CourseLibrary.API.Services;
using System.Linq.Dynamic.Core;

namespace CourseLibrary.API.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy,
            Dictionary<string, PropertyMappingValue> mappingDictionary)

        {
            string orderByString = null;
            if (source == null) {
                throw new ArgumentNullException(nameof(source));

            }

            if (mappingDictionary == null) {
                throw new ArgumentNullException(nameof(mappingDictionary));
            }

            if (string.IsNullOrWhiteSpace(orderBy)) {
                return source;
            }


            var orderByAfterSplit = orderBy.Split(',');

            foreach (var orderByClause in orderByAfterSplit.Reverse())
            {
                var trimmedOrderByClause = orderByClause.Trim();

                var orderDescending = trimmedOrderByClause.EndsWith(" desc");

                //remove asc or desc from orederBt clause so we can get property name from dictionary
                var indexOfFirstSpace = trimmedOrderByClause.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? trimmedOrderByClause : trimmedOrderByClause.Remove(indexOfFirstSpace);

                if (!mappingDictionary.ContainsKey(propertyName)) {
                    throw new ArgumentException($"Key mapping for {propertyName} is missing");
                }

                //get PropertyMappingValue
                var propertyMappingValue = mappingDictionary[propertyName];
                if (propertyMappingValue == null) {
                    throw new ArgumentNullException("propertyMappingValue");
                }


                foreach (var destinationProperty in propertyMappingValue.DestinationProperties) {

                    if (propertyMappingValue.Revert) {
                        orderDescending = !orderDescending;
                    }

                   orderByString = orderByString + (string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ")
                                             + destinationProperty + (orderDescending ? " descending" : " ascending");
                }
 
            }

            return source.OrderBy(orderByString);

        }
    }
}
