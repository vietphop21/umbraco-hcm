using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace HcmPortal.Controllers;

public class ContactModel
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string Message { get; set; } = "";
}

/// <summary>Lưu thư liên hệ thành node "Thư liên hệ" (chưa xuất bản) dưới trang Liên hệ để admin đọc.</summary>
public class ContactController : SurfaceController
{
    private readonly IContentService _content;

    public ContactController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider, IContentService content)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        => _content = content;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Submit(ContactModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Message))
        {
            TempData["contactError"] = "Vui lòng nhập đầy đủ họ tên, email và nội dung.";
            return CurrentUmbracoPage();
        }

        var msg = _content.Create($"{model.Name.Trim()} - {DateTime.Now:dd/MM/yyyy HH:mm}", CurrentPage!.Id, "contactMessage");
        msg.SetValue("senderEmail", model.Email.Trim());
        msg.SetValue("senderPhone", model.Phone?.Trim());
        msg.SetValue("messageText", model.Message.Trim());
        _content.Save(msg);

        TempData["contactOk"] = true;
        return RedirectToCurrentUmbracoPage();
    }
}
