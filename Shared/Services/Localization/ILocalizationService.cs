﻿using BlazorDatasource.Shared.Domain;
using BlazorDatasource.Shared.Domain.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlazorDatasource.Shared.Services.Localization
{
    /// <summary>
    /// Localization manager interface
    /// </summary>
    public partial interface ILocalizationService
    {
        /// <summary>
        /// Deletes a locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteLocaleStringResourceAsync(LocaleStringResource localeStringResource);

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="localeStringResourceId">Locale string resource identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the locale string resource
        /// </returns>
        Task<LocaleStringResource> GetLocaleStringResourceByIdAsync(int localeStringResourceId);

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="resourceName">A string representing a resource name</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the locale string resource
        /// </returns>
        Task<LocaleStringResource> GetLocaleStringResourceByNameAsync(string resourceName,
                                                                      int languageId);

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="resourceName">A string representing a resource name</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>
        /// The locale string resource
        /// </returns>
        LocaleStringResource GetLocaleStringResourceByName(string resourceName,
                                                           int languageId);

        /// <summary>
        /// Inserts a locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertLocaleStringResourceAsync(LocaleStringResource localeStringResource);

        /// <summary>
        /// Updates the locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateLocaleStringResourceAsync(LocaleStringResource localeStringResource);

        /// <summary>
        /// Updates the locale string resource
        /// </summary>
        /// <param name="localeStringResource">Locale string resource</param>
        void UpdateLocaleStringResource(LocaleStringResource localeStringResource);

        /// <summary>
        /// Gets all locale string resources by language identifier
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the locale string resources
        /// </returns>
        Task<Dictionary<string, KeyValuePair<int, string>>> GetAllResourceValuesAsync(int languageId);

        /// <summary>
        /// Gets a resource string based on the specified ResourceKey property.
        /// </summary>
        /// <param name="resourceKey">A string representing a ResourceKey.</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a string representing the requested resource string.
        /// </returns>
        Task<string> GetResourceAsync(string resourceKey);

        /// <summary>
        /// Gets a resource string based on the specified ResourceKey property.
        /// </summary>
        /// <param name="resourceKey">A string representing a ResourceKey.</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="returnEmptyIfNotFound">A value indicating whether an empty string will be returned if a resource is not found and default value is set to empty string</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a string representing the requested resource string.
        /// </returns>
        Task<string> GetResourceAsync(string resourceKey, int languageId, string defaultValue = "", bool returnEmptyIfNotFound = false);

        /// <summary>
        /// Export language resources to XML
        /// </summary>
        /// <param name="language">Language</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result in XML format
        /// </returns>
        Task<string> ExportResourcesToXmlAsync(Language language);

        /// <summary>
        /// Import language resources from XML file
        /// </summary>
        /// <param name="language">Language</param>
        /// <param name="xmlStreamReader">Stream reader of XML file</param>
        /// <param name="updateExistingResources">A value indicating whether to update existing resources</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ImportResourcesFromXmlAsync(Language language, StreamReader xmlStreamReader, bool updateExistingResources = true);

        /// <summary>
        /// Get localized property of an entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="keySelector">Key selector</param>
        /// <param name="languageId">Language identifier; pass null to use the current working language; pass 0 to get standard language value</param>
        /// <param name="returnDefaultValue">A value indicating whether to return default value (if localized is not found)</param>
        /// <param name="ensureTwoPublishedLanguages">A value indicating whether to ensure that we have at least two published languages; otherwise, load only default value</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the localized property
        /// </returns>
        Task<TPropType> GetLocalizedAsync<TEntity, TPropType>(TEntity entity,
                                                              Expression<Func<TEntity, TPropType>> keySelector,
                                                              int? languageId = null,
                                                              bool returnDefaultValue = true,
                                                              bool ensureTwoPublishedLanguages = true)
            where TEntity : BaseEntity, ILocalizedEntity;

        /// <summary>
        /// Get localized value of enum
        /// </summary>
        /// <typeparam name="TEnum">Enum type</typeparam>
        /// <param name="enumValue">Enum value</param>
        /// <param name="languageId">Language identifier; pass null to use the current working language</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the localized value
        /// </returns>
        Task<string> GetLocalizedEnumAsync<TEnum>(TEnum enumValue, int? languageId = null) where TEnum : struct;

        /// <summary>
        /// Add a locale resource (if new) or update an existing one
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <param name="resourceValue">Resource value</param>
        /// <param name="languageCulture">Language culture code. If null or empty, then a resource will be added for all languages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddOrUpdateLocaleResourceAsync(string resourceName, string resourceValue, string languageCulture = null);

        /// <summary>
        /// Add locale resources
        /// </summary>
        /// <param name="resources">Resource name-value pairs</param>
        /// <param name="languageId">Language identifier; pass null to add the passed resources for all languages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddOrUpdateLocaleResourceAsync(IDictionary<string, string> resources, int? languageId = null);

        /// <summary>
        /// Add locale resources
        /// </summary>
        /// <param name="resources">Resource name-value pairs</param>
        /// <param name="languageId">Language identifier; pass null to add the passed resources for all languages</param>
        void AddOrUpdateLocaleResource(IDictionary<string, string> resources, int? languageId = null);

        /// <summary>
        /// Delete a locale resource
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteLocaleResourceAsync(string resourceName);

        /// <summary>
        /// Delete locale resources
        /// </summary>
        /// <param name="resourceNames">Resource names</param>
        /// <param name="languageId">Language identifier; pass null to delete the passed resources from all languages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteLocaleResourcesAsync(IList<string> resourceNames, int? languageId = null);

        /// <summary>
        /// Delete locale resources
        /// </summary>
        /// <param name="resourceNames">Resource names</param>
        /// <param name="languageId">Language identifier; pass null to delete the passed resources from all languages</param>
        void DeleteLocaleResources(IList<string> resourceNames, int? languageId = null);

        /// <summary>
        /// Delete locale resources by the passed name prefix
        /// </summary>
        /// <param name="resourceNamePrefix">Resource name prefix</param>
        /// <param name="languageId">Language identifier; pass null to delete resources by prefix from all languages</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteLocaleResourcesAsync(string resourceNamePrefix, int? languageId = null);
    }
}
