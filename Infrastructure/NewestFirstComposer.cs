using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace HcmPortal.Infrastructure;

public class NewestFirstComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder.AddNotificationHandler<ContentSavedNotification, NewestFirstHandler>();
}

/// <summary>
/// Bài mới tạo trong backoffice dưới một chuyên mục được đưa lên đầu cây (mặc định Umbraco thêm xuống cuối).
/// Chỉ áp dụng khi có người dùng backoffice thao tác, để không xáo thứ tự khi uSync import lúc khởi động.
/// </summary>
public class NewestFirstHandler : INotificationHandler<ContentSavedNotification>
{
    private readonly IContentService _content;
    private readonly IBackOfficeSecurityAccessor _security;

    public NewestFirstHandler(IContentService content, IBackOfficeSecurityAccessor security)
    {
        _content = content;
        _security = security;
    }

    public void Handle(ContentSavedNotification notification)
    {
        if (_security.BackOfficeSecurity?.CurrentUser == null) return;

        foreach (var item in notification.SavedEntities)
        {
            if (!item.WasPropertyDirty("Id") || item.ParentId <= 0) continue;
            var parent = _content.GetById(item.ParentId);
            if (parent?.ContentType.Alias != "category") continue;

            var siblings = _content.GetPagedChildren(item.ParentId, 0, int.MaxValue, out _)
                .Where(x => x.Id != item.Id)
                .OrderBy(x => x.SortOrder);
            _content.Sort(new[] { item }.Concat(siblings));
        }
    }
}
