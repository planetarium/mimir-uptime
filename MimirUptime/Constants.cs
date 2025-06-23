using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NodeUptime
{
    public static class Constants
    {
        public static readonly IReadOnlyDictionary<string, string> HEADLESS_URLS =
            new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>
                {
                    { "Odin", "https://odin-gql.nine-chronicles.com/graphql" },
                    { "Heimdall", "https://heimdall-gql.nine-chronicles.com/graphql" },
                }
            );


        public static readonly IReadOnlyDictionary<string, string> MIMIR_URLS =
            new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>
                {
                    { "Odin", "https://odin-mimir.9c.gg/graphql" },
                    { "Heimdall", "https://heimdall-mimir.9c.gg/graphql" },
                }
            );

        public static readonly IReadOnlyList<string> COLLECTION_NAMES = new List<string>
        {
            CollectionNames.GetCollectionName<AvatarDocument>(),
            CollectionNames.GetCollectionName<WorldInformationDocument>(),
            CollectionNames.GetCollectionName<AdventureCpDocument>(),
        };
    }
}
