using HcmPortal.Infrastructure;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace HcmPortal.Setup;

/// <summary>
/// Đưa cây nội dung về đúng Đề xuất site map (21/10/2026): 11 chuyên mục với URL cố định,
/// cây giai đoạn 5 nhánh tại /giai-doan, trang tra cứu tại /tra-cuu.
/// Chỉ sửa phần còn lệch; tư liệu được di chuyển chứ không xoá.
/// </summary>
public class SiteMapSetupHandler : INotificationHandler<UmbracoApplicationStartedNotification>,
    INotificationHandler<UnattendedInstallNotification>
{
    private const string SiteName = "Không gian Văn hóa Hồ Chí Minh";
    private const string OldSiteName = "Hồ Chí Minh";
    private const string Slogan = "Cơ sở dữ liệu chuyên đề trên nền tảng số";
    private const string SearchName = "Tra cứu";
    private const string IntroPageName = "Giới thiệu";
    private const string GroupContent = "content";
    private const string GroupContentName = "Nội dung";

    /// <summary>Chuyên mục đã có trên cây (tên cũ) được dùng lại cho chuyên mục mới.</summary>
    private static readonly Dictionary<string, string> ExistingByUrl = new()
    {
        [SiteMap.Life] = "Cuộc đời, sự nghiệp",
        [SiteMap.Thought] = "Tư tưởng, đạo đức, phong cách Hồ Chí Minh",
        [SiteMap.Follow] = "Học và làm theo Bác",
    };

    /// <summary>Kho media cũ (ẩn) gom vào Thư viện tư liệu số: tên cũ → tên mới.</summary>
    private static readonly (string Old, string New)[] MediaFolders =
    {
        ("Tư liệu Video", "Phim tư liệu"),
        ("Tư liệu Ảnh", "Hình ảnh tư liệu"),
        ("Tư liệu Audio", "Sách nói"),
    };

    private static readonly Dictionary<string, (string Title, string Summary)[]> DemoArticles = new()
    {
        [SiteMap.Saigon] = new[]
        {
            ("Bến Nhà Rồng – điểm khởi đầu hành trình cứu nước", "Di tích lịch sử gắn với ngày 5/6/1911, nay là Bảo tàng Hồ Chí Minh – chi nhánh TP. Hồ Chí Minh."),
            ("Thành phố mang tên Bác", "Hành trình xây dựng và phát triển của Thành phố vinh dự mang tên Chủ tịch Hồ Chí Minh (bản demo)."),
        },
        [SiteMap.Works] = new[]
        {
            ("Giới thiệu các tác phẩm tiêu biểu", "Tổng quan các tác phẩm, bài viết, bài nói tiêu biểu của Chủ tịch Hồ Chí Minh (bản demo)."),
            ("Hướng dẫn tra cứu tác phẩm trong Tủ sách điện tử", "Cách tìm và đọc trực tuyến các tác phẩm đã số hóa (bản demo)."),
        },
        [SiteMap.Kids] = new[]
        {
            ("Chuyện kể: Bác Hồ với các cháu thiếu nhi", "Những câu chuyện giản dị, gần gũi dành cho các em (bản demo)."),
            ("Đố vui: Em yêu Bác Hồ", "Khu vực nhúng trò chơi đố vui, ghép hình do cán bộ thư viện tạo sẵn (bản demo)."),
        },
        [SiteMap.Sectors] = new[]
        {
            ("Bác Hồ với ngành giáo dục", "Những lời dạy của Bác đối với thầy giáo, học sinh (bản demo)."),
            ("Bác Hồ với ngành y tế", "Tình cảm và lời căn dặn của Bác với đội ngũ thầy thuốc (bản demo)."),
        },
        [SiteMap.Documents] = new[]
        {
            ("Hướng dẫn học tập chuyên đề năm 2026", "Văn bản hướng dẫn tổ chức học tập chuyên đề (bản demo, tệp PDF đính kèm sau)."),
            ("Kế hoạch tuyên truyền các ngày lễ lớn", "Kế hoạch tuyên truyền kỷ niệm các ngày lễ lớn trong năm (bản demo)."),
        },
        [SiteMap.Map] = new[]
        {
            ("Khu di tích Kim Liên (Nghệ An)", "Quê nội, quê ngoại của Chủ tịch Hồ Chí Minh (bản demo, tọa độ bổ sung sau)."),
            ("Khu di tích Pác Bó (Cao Bằng)", "Nơi Bác sống và làm việc khi trở về nước năm 1941 (bản demo, tọa độ bổ sung sau)."),
        },
    };

    private static int _done;

    private readonly IContentTypeService _contentTypes;
    private readonly IDataTypeService _dataTypes;
    private readonly IContentService _content;
    private readonly IShortStringHelper _sh;
    private readonly IRuntimeState _runtime;
    private readonly ILogger<SiteMapSetupHandler> _logger;

    private readonly IMediaService _media;
    private readonly Umbraco.Cms.Core.IO.MediaFileManager _mediaFiles;
    private readonly Umbraco.Cms.Core.PropertyEditors.MediaUrlGeneratorCollection _urlGenerators;
    private readonly IContentTypeBaseServiceProvider _typeProvider;
    private readonly IWebHostEnvironment _env;

    public SiteMapSetupHandler(IContentTypeService contentTypes, IDataTypeService dataTypes, IContentService content,
        IMediaService media, Umbraco.Cms.Core.IO.MediaFileManager mediaFiles,
        Umbraco.Cms.Core.PropertyEditors.MediaUrlGeneratorCollection urlGenerators, IContentTypeBaseServiceProvider typeProvider,
        IWebHostEnvironment env, IShortStringHelper sh, IRuntimeState runtime, ILogger<SiteMapSetupHandler> logger)
    {
        _media = media;
        _mediaFiles = mediaFiles;
        _urlGenerators = urlGenerators;
        _typeProvider = typeProvider;
        _env = env;
        _contentTypes = contentTypes;
        _dataTypes = dataTypes;
        _content = content;
        _sh = sh;
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
            EnsureUrlNameProperty();
            var home = _content.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == DocTypes.Home && !x.Trashed);
            if (home == null) return;
            EnsureSiteName(home);
            EnsureSections(home);
            EnsureSearchPage(home);
            EnsurePeriods(home);
            EnsureBanner(home);
            EnsureEDocuments(home);
            EnsureMapPlaces(home);
            EnsureThumbnails(home);
        }
        catch (Exception ex)
        {
            _done = 0;
            _logger.LogError(ex, "Không thể sắp xếp cây nội dung theo site map");
        }
    }

    private void EnsureUrlNameProperty()
    {
        var textstring = _dataTypes.GetAll().First(d => d.Name == "Textstring");
        foreach (var alias in new[] { DocTypes.Category, DocTypes.TextPage, DocTypes.SearchPage, DocTypes.LibraryRoot })
        {
            var ct = _contentTypes.Get(alias);
            if (ct == null) continue;
            if (!ct.PropertyTypeExists(SiteMap.UrlNameProp))
                ct.AddPropertyType(new PropertyType(_sh, textstring, SiteMap.UrlNameProp) { Name = "Đường dẫn (URL)" }, GroupContent, GroupContentName);

            // Umbraco đọc umbracoUrlName theo ngôn ngữ của trang: DocType đa ngôn ngữ thì trường này cũng phải theo ngôn ngữ.
            var pt = ct.PropertyTypes.First(p => p.Alias == SiteMap.UrlNameProp);
            if (ct.VariesByCulture() && !pt.VariesByCulture()) pt.Variations = ContentVariation.Culture;
            if (ct.IsDirty() || pt.IsDirty()) _contentTypes.Save(ct);
        }
    }

    private void EnsureSiteName(IContent home)
    {
        if (home.GetValue<string>(Props.SiteName, Cultures.Vi) != OldSiteName) return;
        home.SetValue(Props.SiteName, SiteName, Cultures.Vi);
        home.SetValue(Props.Slogan, Slogan, Cultures.Vi);
        Publish(home);
    }

    private List<IContent> Children(IContent parent)
        => _content.GetPagedChildren(parent.Id, 0, 1000, out _).Where(c => !c.Trashed).ToList();

    private void EnsureSections(IContent home)
    {
        var children = Children(home);
        foreach (var s in SiteMap.Sections)
        {
            var node = children.FirstOrDefault(c => UrlName(c) == s.UrlName)
                       ?? children.FirstOrDefault(c => c.Name == s.Name)
                       ?? (s.UrlName == SiteMap.Intro ? children.FirstOrDefault(c => c.ContentType.Alias == DocTypes.TextPage && c.Name == IntroPageName) : null)
                       ?? (ExistingByUrl.TryGetValue(s.UrlName, out var old) ? children.FirstOrDefault(c => c.Name == old) : null);

            var isNew = node == null;
            node ??= _content.Create(s.Name, home.Id, DocTypes.Category);
            if (UrlName(node) == s.UrlName && !isNew) continue;

            SetName(node, s.Name);
            Set(node, SiteMap.UrlNameProp, s.UrlName);
            Set(node, Props.Description, s.Description);
            if (node.HasProperty(LibraryProps.Hide)) node.SetValue(LibraryProps.Hide, false);
            Publish(node);

            if (isNew && DemoArticles.TryGetValue(s.UrlName, out var demos))
                foreach (var (title, summary) in demos) CreateArticle(node, title, summary);
            if (s.UrlName == SiteMap.Media) GatherMediaFolders(home, node);
        }
    }

    private void GatherMediaFolders(IContent home, IContent media)
    {
        foreach (var (oldName, newName) in MediaFolders)
        {
            var folder = Children(home).FirstOrDefault(c => c.Name == oldName);
            if (folder == null) continue;
            _content.Move(folder, media.Id);
            SetName(folder, newName);
            folder.SetValue(LibraryProps.Hide, false);
            Publish(folder);
        }
    }

    private void EnsureSearchPage(IContent home)
    {
        var search = Children(home).FirstOrDefault(c => c.ContentType.Alias == DocTypes.SearchPage);
        if (search == null || UrlName(search) == SiteMap.Search) return;
        SetName(search, SearchName);
        Set(search, SiteMap.UrlNameProp, SiteMap.Search);
        Publish(search);
    }

    /// <summary>Cây giai đoạn cũ (5 nhánh 1890–1969) chuyển sang 5 nhánh mới; tư liệu được dồn sang nhánh tương ứng.</summary>
    private void EnsurePeriods(IContent home)
    {
        var root = Children(home).FirstOrDefault(c => c.ContentType.Alias == DocTypes.LibraryRoot);
        if (root == null) return;
        if (UrlName(root) != SiteMap.Periods)
        {
            root.Name = LibrarySeed.LibraryNewName;
            Set(root, SiteMap.UrlNameProp, SiteMap.Periods);
            root.SetValue(LibraryProps.Hide, true);
            Publish(root);
        }

        var periods = Children(root).Where(c => c.ContentType.Alias == DocTypes.Period).OrderBy(c => c.SortOrder).ToList();
        var target = LibrarySeed.Periods;
        if (periods.Count != target.Length || periods.Select(p => p.Name).SequenceEqual(target.Select(t => t.Name))) return;

        // Nhánh cũ i+1 dồn tư liệu vào nhánh mới i: (1911–1920 + 1920–1930) → 1911–1941, 1930–1945 → 1941–1954, 1945–1969 → 1954–1969.
        for (var i = 1; i < periods.Count - 1; i++)
            MoveItems(periods[i + 1], periods[i]);

        for (var i = 0; i < periods.Count; i++)
        {
            var p = periods[i];
            p.Name = target[i].Name;
            p.SetValue(Props.Description, target[i].Description);
            p.SetValue(LibraryProps.FromYear, target[i].From);
            p.SetValue(LibraryProps.ToYear, target[i].To);
            Publish(p);
        }

        var last = periods[^1];
        foreach (var item in target[^1].Items)
        {
            var topic = Children(last).FirstOrDefault(t => t.Name == item.Topic);
            if (topic != null) CreateArticle(topic, item.Title, item.Summary, item.Year);
        }
        _logger.LogInformation("Đã chuyển cây giai đoạn sang 5 nhánh mới");
    }

    private void MoveItems(IContent from, IContent to)
    {
        var targetTopics = Children(to);
        foreach (var topic in Children(from))
        {
            var dest = targetTopics.FirstOrDefault(t => t.Name == topic.Name);
            if (dest == null) continue;
            foreach (var item in Children(topic)) _content.Move(item, dest.Id);
        }
    }

    private void CreateArticle(IContent parent, string title, string summary, int year = 0)
    {
        var c = _content.Create(title, parent.Id, DocTypes.Article);
        SetName(c, title);
        Set(c, Props.Summary, summary);
        Set(c, Props.BodyText, $"<p>{summary}</p><p>Nội dung chi tiết sẽ được Ban biên tập cập nhật.</p>");
        Set(c, "publishDate", DateTime.Now);
        if (year > 0) Set(c, LibraryProps.EventYear, year);
        Publish(c);
    }

    /// <summary>Địa danh demo cho Bản đồ số; tọa độ gần đúng, Ban biên tập hiệu chỉnh khi nhập chính thức.</summary>
    private static readonly (string Title, string Summary, double Lat, double Lng)[] MapPlaces =
    {
        ("Khu di tích Kim Liên (Nghệ An)", "Quê nội, quê ngoại của Chủ tịch Hồ Chí Minh tại huyện Nam Đàn.", 18.7056, 105.5428),
        ("Khu di tích Pác Bó (Cao Bằng)", "Nơi Bác sống và làm việc khi trở về nước năm 1941.", 22.9806, 106.0444),
        ("Khu di tích Tân Trào (Tuyên Quang)", "Thủ đô khu giải phóng, nơi diễn ra Quốc dân Đại hội năm 1945.", 21.7864, 105.4386),
        ("Khu di tích ATK Định Hóa (Thái Nguyên)", "An toàn khu, nơi Bác sống và làm việc trong kháng chiến chống Pháp.", 21.9200, 105.6400),
        ("Quảng trường Ba Đình – Lăng Chủ tịch Hồ Chí Minh (Hà Nội)", "Nơi Bác đọc Tuyên ngôn độc lập ngày 2/9/1945.", 21.0368, 105.8347),
        ("Khu di tích Phủ Chủ tịch – Nhà sàn Bác Hồ (Hà Nội)", "Nơi Bác sống và làm việc từ năm 1954 đến năm 1969.", 21.0389, 105.8338),
        ("Bến Nhà Rồng (TP. Hồ Chí Minh)", "Nơi người thanh niên Nguyễn Tất Thành ra đi tìm đường cứu nước ngày 5/6/1911.", 10.7681, 106.7068),
    };

    private void EnsureMapPlaces(IContent home)
    {
        var article = _contentTypes.Get(DocTypes.Article);
        if (article == null) return;
        if (!article.PropertyTypeExists(MapProps.Lat))
        {
            var textstring = _dataTypes.GetAll().First(d => d.Name == "Textstring");
            article.AddPropertyType(new PropertyType(_sh, textstring, MapProps.Lat) { Name = "Vĩ độ (Lat)" }, "map", "Bản đồ");
            article.AddPropertyType(new PropertyType(_sh, textstring, MapProps.Lng) { Name = "Kinh độ (Lng)" }, "map", "Bản đồ");
            _contentTypes.Save(article);
        }

        var map = Children(home).FirstOrDefault(c => UrlName(c) == SiteMap.Map);
        if (map == null) return;
        var existing = Children(map);
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        foreach (var (title, summary, lat, lng) in MapPlaces)
        {
            var node = existing.FirstOrDefault(c => c.Name == title);
            if (node != null && !string.IsNullOrEmpty(node.GetValue<string>(MapProps.Lat))) continue;
            if (node == null)
            {
                node = _content.Create(title, map.Id, DocTypes.Article);
                SetName(node, title);
                Set(node, Props.Summary, summary);
                Set(node, Props.BodyText, $"<p>{summary}</p><p>Nội dung chi tiết sẽ được Ban biên tập cập nhật.</p>");
                Set(node, "publishDate", DateTime.Now);
            }
            node.SetValue(MapProps.Lat, lat.ToString(inv));
            node.SetValue(MapProps.Lng, lng.ToString(inv));
            Publish(node);
        }
    }

    private const string EDocsName = "Tài liệu điện tử (PDF)";
    private const string PdfFolder = "Tài liệu PDF (demo)";

    /// <summary>Tài liệu điện tử demo trong Thư viện tư liệu số; dùng chung tệp PDF demo, Ban biên tập thay bằng tài liệu thật.</summary>
    private static readonly (string Title, string Summary, string Author, int Year)[] EDocs =
    {
        ("Tuyển tập tư liệu: Cuộc đời và sự nghiệp (bản demo)", "Tập tư liệu điện tử tóm lược các mốc lịch sử từ năm 1890 đến năm 1969.", "Ban Biên tập", 2026),
        ("Sổ tay học tập và làm theo tư tưởng, đạo đức, phong cách Hồ Chí Minh (bản demo)", "Tài liệu hướng dẫn sinh hoạt chuyên đề dành cho cán bộ, đoàn viên.", "Ban Biên tập", 2025),
        ("Di tích lưu niệm Chủ tịch Hồ Chí Minh trên cả nước (bản demo)", "Giới thiệu các khu di tích, nhà lưu niệm gắn với cuộc đời Chủ tịch Hồ Chí Minh.", "Thư viện KHTH TP.HCM", 2024),
        ("Chủ tịch Hồ Chí Minh với Sài Gòn – TP. Hồ Chí Minh (bản demo)", "Tập tư liệu về tình cảm của Bác với Thành phố mang tên Người.", "Thư viện KHTH TP.HCM", 2024),
    };

    private void EnsureEDocuments(IContent home)
    {
        var category = _contentTypes.Get(DocTypes.Category);
        var pdfType = _contentTypes.Get(DocTypes.PdfDocument);
        if (category == null || pdfType == null) return;
        var allowed = category.AllowedContentTypes?.ToList() ?? new List<ContentTypeSort>();
        if (allowed.All(a => a.Alias != DocTypes.PdfDocument))
        {
            allowed.Add(new ContentTypeSort(new Lazy<int>(() => pdfType.Id), allowed.Count, pdfType.Alias));
            category.AllowedContentTypes = allowed;
            _contentTypes.Save(category);
        }

        var media = Children(home).FirstOrDefault(c => UrlName(c) == SiteMap.Media);
        if (media == null) return;
        var folder = Children(media).FirstOrDefault(c => c.Name == EDocsName);
        if (folder == null)
        {
            folder = _content.Create(EDocsName, media.Id, DocTypes.Category);
            SetName(folder, EDocsName);
            Set(folder, Props.Description, "Sách, tài liệu số hóa định dạng PDF, đọc trực tuyến dạng lật trang.");
            Publish(folder);
        }
        if (Children(folder).Any()) return;

        var pdfFolder = _media.GetRootMedia().FirstOrDefault(m => m.Name == PdfFolder);
        var pdf = pdfFolder == null ? null : _media.GetPagedChildren(pdfFolder.Id, 0, 10, out _).FirstOrDefault();
        if (pdf == null) return;
        var pick = System.Text.Json.JsonSerializer.Serialize(new[] { new { key = Guid.NewGuid(), mediaKey = pdf.Key } });

        foreach (var (title, summary, author, year) in EDocs)
        {
            var doc = _content.Create(title, folder.Id, DocTypes.PdfDocument);
            SetName(doc, title);
            Set(doc, Props.Summary, summary);
            Set(doc, Props.BodyText, $"<p>{summary}</p>");
            Set(doc, LibraryProps.Author, author);
            Set(doc, LibraryProps.PdfFile, pick);
            Set(doc, LibraryProps.EventYear, year);
            Set(doc, "publishDate", DateTime.Now);
            Publish(doc);
        }
        _logger.LogInformation("Đã tạo chuyên mục Tài liệu điện tử (PDF)");
    }

    private const string BannerFile = "banner.png";
    private const string BannerFolder = "Banner";
    private static readonly string[] BannerProps = { "bannerImage", "bannerMobile" };

    /// <summary>Banner do khách hàng cung cấp (Setup/demo-media/banner.png); chỉ gán khi Trang chủ chưa có banner.</summary>
    private void EnsureBanner(IContent home)
    {
        if (BannerProps.All(p => !string.IsNullOrEmpty(home.GetValue<string>(p)))) return;
        var file = Path.Combine(_env.ContentRootPath, "Setup", "demo-media", BannerFile);
        if (!System.IO.File.Exists(file)) return;

        var folder = _media.GetRootMedia().FirstOrDefault(m => m.Name == BannerFolder);
        if (folder == null)
        {
            folder = _media.CreateMedia(BannerFolder, Constants.System.Root, Constants.Conventions.MediaTypes.Folder);
            _media.Save(folder);
        }
        var image = _media.CreateMedia(Path.GetFileNameWithoutExtension(BannerFile), folder.Id, Constants.Conventions.MediaTypes.Image);
        using (var fs = System.IO.File.OpenRead(file))
            image.SetValue(_mediaFiles, _urlGenerators, _sh, _typeProvider, Constants.Conventions.Media.File, BannerFile, fs);
        _media.Save(image);

        var pick = System.Text.Json.JsonSerializer.Serialize(new[] { new { key = Guid.NewGuid(), mediaKey = image.Key } });
        foreach (var p in BannerProps.Where(p => string.IsNullOrEmpty(home.GetValue<string>(p)))) home.SetValue(p, pick);
        Publish(home);
        _logger.LogInformation("Đã gán banner cho Trang chủ");
    }

    private const string ImageFolder = "Ảnh minh họa (demo)";

    /// <summary>Tư liệu chưa có ảnh đại diện được gắn ảnh di tích thật (Wikimedia Commons) trong thư mục Media demo.</summary>
    private void EnsureThumbnails(IContent home)
    {
        var folder = _media.GetRootMedia().FirstOrDefault(m => m.Name == ImageFolder);
        if (folder == null) return;
        var images = _media.GetPagedChildren(folder.Id, 0, 500, out _).ToList();
        if (images.Count == 0) return;

        var missing = _content.GetPagedDescendants(home.Id, 0, 10000, out _)
            .Where(c => !c.Trashed && SiteHelpers.ItemTypes.Contains(c.ContentType.Alias)
                        && c.HasProperty(LibraryProps.Thumbnail) && string.IsNullOrEmpty(c.GetValue<string>(LibraryProps.Thumbnail)))
            .ToList();
        var n = 0;
        foreach (var c in missing)
        {
            var img = images[(c.Id + n++) % images.Count];
            c.SetValue(LibraryProps.Thumbnail, System.Text.Json.JsonSerializer.Serialize(new[] { new { key = Guid.NewGuid(), mediaKey = img.Key } }));
            Publish(c);
        }
        if (missing.Count > 0) _logger.LogInformation("Đã gắn ảnh cho {Count} tư liệu chưa có ảnh", missing.Count);
    }

    private static string? UrlName(IContent c)
    {
        var pt = c.Properties.TryGetValue(SiteMap.UrlNameProp, out var p) ? p?.PropertyType : null;
        if (pt == null) return null;
        return pt.VariesByCulture() ? c.GetValue<string>(SiteMap.UrlNameProp, Cultures.Vi) : c.GetValue<string>(SiteMap.UrlNameProp);
    }

    private static void SetName(IContent c, string name)
    {
        if (c.ContentType.VariesByCulture()) c.SetCultureName(name, Cultures.Vi);
        else c.Name = name;
    }

    private static void Set(IContent c, string alias, object? value)
    {
        if (!c.Properties.TryGetValue(alias, out var prop) || prop == null) return;
        var pt = prop.PropertyType;
        if (pt == null) return;
        if (pt.VariesByCulture()) c.SetValue(alias, value, Cultures.Vi);
        else c.SetValue(alias, value);
    }

    private void Publish(IContent c)
    {
        var result = c.ContentType.VariesByCulture() ? _content.SaveAndPublish(c, Cultures.Vi) : _content.SaveAndPublish(c);
        if (!result.Success) _logger.LogWarning("Không publish được {Name}: {Result}", c.Name, result.Result);
    }
}
