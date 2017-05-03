﻿using Sdl.Web.Common.Configuration;
using Sdl.Web.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Sdl.Web.Modules.Core.Models
{
    [SemanticEntity(Vocab = SchemaOrgVocabulary, EntityName = "ItemList", Prefix = "s", Public = true)]
    [Serializable]
    public class ContentList<T> : DynamicList where T : EntityModel
    {
        //TODO add concept of filtering/query (filter options and active filters/query)
        [SemanticProperty("s:headline")]
        public string Headline { get; set; }
        public Link Link { get; set; }
        public Tag ContentType { get; set; }
        public Tag Sort { get; set; }
        public int PageSize { get; set; }

        [SemanticProperty(IgnoreMapping = true)]
        public int CurrentPage 
        { 
            get 
            {
                return PageSize == 0 ? 1 : (Start / PageSize) + 1;
            }
        }

        public override Query GetQuery(Localization localization)
        {
            return new SimpleBrokerQuery
            {
                Start = Start,
                PageSize = PageSize,
                PublicationId = Int32.Parse(localization.LocalizationId),
                SchemaId = MapSchema(ContentType.Key, localization),
                Sort = Sort.Key,
                Localization = localization
            };
        }

        protected int MapSchema(string schemaKey, Localization localization)
        {
            string[] schemaKeyParts = schemaKey.Split('.');
            string moduleName = schemaKeyParts.Length > 1 ? schemaKeyParts[0] : SiteConfiguration.CoreModuleName;
            schemaKey = schemaKeyParts.Length > 1 ? schemaKeyParts[1] : schemaKeyParts[0];
            string schemaId = localization.GetConfigValue(string.Format("{0}.schemas.{1}", moduleName, schemaKey));
            int result;
            Int32.TryParse(schemaId, out result);
            return result;
        }

        [JsonIgnore]
        public override Type ResultType
        {
            get
            {
                return typeof(T);
            }
        }

        /// <summary>
        /// Gets or sets the items in the list.
        /// </summary>
        /// <remarks>
        /// The items can be retrieved dynamically, but also mapped from CM (e.g. ItemList Schema).
        /// </remarks>
        public List<T> ItemListElements
        {
            get
            {
                return QueryResults.Cast<T>().ToList();
            }
            set
            {
                if (value != null)
                {
                    QueryResults = value.Cast<EntityModel>().ToList();
                }
                else
                {
                    QueryResults = null;
                }
            }
        }
    }
}