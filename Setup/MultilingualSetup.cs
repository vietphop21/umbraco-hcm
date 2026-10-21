using HcmPortal.Infrastructure;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace HcmPortal.Setup;

/// <summary>
/// Website song ngữ: tiếng Việt là ngôn ngữ mặc định (URL gốc), tiếng Anh nằm dưới /en.
/// Bài viết chưa có bản tiếng Anh không hiển thị ở bản /en cho tới khi biên tập viên dịch và publish.
/// </summary>
public class MultilingualSetupHandler : INotificationHandler<UmbracoApplicationStartedNotification>,
    INotificationHandler<UnattendedInstallNotification>
{
    private static readonly string[] VariantTypes =
    {
        DocTypes.Home, DocTypes.Category, DocTypes.Article, DocTypes.Video, DocTypes.Album, DocTypes.Audio,
        DocTypes.TextPage, DocTypes.SearchPage, DocTypes.SitemapPage,
    };

    private static readonly string[] VariantProps =
    {
        Props.SiteName, Props.Slogan, Props.FooterOrg, Props.FooterCopyright, Props.FooterAddress, Props.WebsiteLinks,
        Props.Description, Props.Summary, Props.BodyText, Props.Source,
    };

    /// <summary>Khung điều hướng cần có bản tiếng Anh để /en dùng được ngay.</summary>
    private static readonly string[] EnglishSkeletonTypes =
        { DocTypes.Category, DocTypes.TextPage, DocTypes.SearchPage, DocTypes.SitemapPage };

    private static readonly string[] HomeFooterProps =
        { Props.FooterOrg, Props.FooterCopyright, Props.FooterAddress, Props.WebsiteLinks };

    private static int _done;

    private readonly ILocalizationService _languages;
    private readonly IContentTypeService _contentTypes;
    private readonly IContentService _content;
    private readonly IDomainService _domains;
    private readonly IRuntimeState _runtime;
    private readonly ILogger<MultilingualSetupHandler> _logger;

    public MultilingualSetupHandler(ILocalizationService languages, IContentTypeService contentTypes, IContentService content,
        IDomainService domains, IRuntimeState runtime, ILogger<MultilingualSetupHandler> logger)
    {
        _languages = languages;
        _contentTypes = contentTypes;
        _content = content;
        _domains = domains;
        _runtime = runtime;
        _logger = logger;
    }

    public void Handle(UnattendedInstallNotification notification) => Run();

    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        if (_runtime.Level == RuntimeLevel.Run) Run();
    }

    private void Run()
    {
        if (Interlocked.Exchange(ref _done, 1) == 1) return;

        try
        {
            EnsureLanguages();
            EnsureVariants();
            var home = _content.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == DocTypes.Home);
            if (home == null) return;
            EnsureDomain(home);
            EnsureEnglishSkeleton(home);
        }
        catch (Exception ex)
        {
            _done = 0;
            _logger.LogError(ex, "Không thể thiết lập đa ngôn ngữ");
        }
    }

    // Tiếng Việt phải là mặc định trước khi bật đa ngôn ngữ cho DocType,
    // để nội dung hiện có được giữ làm bản tiếng Việt.
    private void EnsureLanguages()
    {
        var vi = _languages.GetLanguageByIsoCode(Cultures.Vi);
        if (vi == null)
            _languages.Save(new Language(Cultures.Vi, Cultures.ViName) { IsDefault = true, IsMandatory = true });
        else if (!vi.IsDefault)
        {
            vi.IsDefault = true;
            vi.IsMandatory = true;
            _languages.Save(vi);
        }

        if (_languages.GetLanguageByIsoCode(Cultures.En) == null)
            _languages.Save(new Language(Cultures.En, Cultures.EnName));
    }

    private void EnsureVariants()
    {
        foreach (var alias in VariantTypes)
        {
            var ct = _contentTypes.Get(alias);
            if (ct == null) continue;

            var changed = false;
            if (!ct.VariesByCulture())
            {
                ct.Variations = ContentVariation.Culture;
                changed = true;
            }
            foreach (var p in ct.PropertyTypes.Where(p => VariantProps.Contains(p.Alias) && !p.VariesByCulture()))
            {
                p.Variations = ContentVariation.Culture;
                changed = true;
            }
            if (changed) _contentTypes.Save(ct);
        }
    }

    private void EnsureDomain(IContent home)
    {
        var enId = _languages.GetLanguageByIsoCode(Cultures.En)!.Id;
        var domain = _domains.GetAssignedDomains(home.Id, true).FirstOrDefault(d => d.DomainName == Cultures.EnDomain)
                     ?? new UmbracoDomain(Cultures.EnDomain) { RootContentId = home.Id };
        if (domain.LanguageId == enId && domain.HasIdentity) return;
        domain.LanguageId = enId;
        _domains.Save(domain);
    }

    private void EnsureEnglishSkeleton(IContent home)
    {
        var nodes = new List<IContent> { home };
        nodes.AddRange(_content.GetPagedDescendants(home.Id, 0, int.MaxValue, out _)
            .Where(x => !x.Trashed && EnglishSkeletonTypes.Contains(x.ContentType.Alias)));

        foreach (var node in nodes.OrderBy(x => x.Level).ThenBy(x => x.SortOrder))
        {
            if (node.IsCulturePublished(Cultures.En)) continue;

            var viName = node.GetCultureName(Cultures.Vi) ?? node.Name ?? string.Empty;
            node.SetCultureName(UiText.EnNodeNames.GetValueOrDefault(viName, viName), Cultures.En);
            if (node.ContentType.Alias == DocTypes.Home)
            {
                node.SetValue(Props.SiteName, node.GetValue<string>(Props.SiteName, Cultures.Vi), Cultures.En);
                node.SetValue(Props.Slogan, UiText.EnSlogan, Cultures.En);
                foreach (var alias in HomeFooterProps)
                    node.SetValue(alias, node.GetValue<string>(alias, Cultures.Vi), Cultures.En);
            }

            var result = _content.SaveAndPublish(node, Cultures.En);
            if (!result.Success)
                _logger.LogWarning("Không publish được bản tiếng Anh của {Name}: {Result}", viName, result.Result);
        }
    }
}
