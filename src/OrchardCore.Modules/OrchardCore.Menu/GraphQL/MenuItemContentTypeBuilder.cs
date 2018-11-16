using GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.GraphQL
{
    public class MenuItemContentTypeBuilder : IContentTypeBuilder
    {
        public void Build(FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ContentItemType contentItemType)
        {
            var settings = contentTypeDefinition.Settings?.ToObject<ContentTypeSettings>();

            if (settings != null && settings.Stereotype != "MenuItem") return;

            contentItemType.Field<MenuItemsListQueryObjectType>(
                "menuItemsListPart",
                resolve: context => context.Source.As<MenuItemsListPart>()
            );

            contentItemType.Interface<MenuItemInterface>();
        }
    }
}