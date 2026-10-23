using System.Text.Json;
using HcmPortal.Infrastructure;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace HcmPortal.Setup;

/// <summary>
/// Thư viện số: cây thư mục 3 cấp (Giai đoạn → Chuyên mục → Nội dung) là chuẩn phân loại chính của kho tư liệu.
/// Chỉ bổ sung khi chưa có, không xoá hay ghi đè nội dung biên tập viên đã nhập.
/// </summary>
public class DigitalLibrarySetupHandler : INotificationHandler<UmbracoApplicationStartedNotification>,
    INotificationHandler<UnattendedInstallNotification>
{
    private const string LibraryTemplate = "Library";
    private const string DetailTemplate = "detail";
    private const string PeriodPickerName = "Chọn giai đoạn lịch sử";
    private const string GroupLibrary = "library";
    private const string GroupLibraryName = "Thư viện số";
    private const string GroupContent = "content";
    private const string GroupContentName = "Nội dung";
    private const string ImageFolder = "Ảnh minh họa (demo)";
    private const string AvFolder = "Video & Audio (demo)";
    private const string PdfFolder = "Tài liệu PDF (demo)";
    private const string DemoMediaDir = "demo-media";
    private const string VideoMediaType = "umbracoMediaVideo";
    private const string AudioMediaType = "umbracoMediaAudio";

    private static readonly string[] ItemTypesWithLibraryProps =
        { DocTypes.Article, DocTypes.Video, DocTypes.Audio, DocTypes.Album };

    private static int _done;

    private readonly IContentTypeService _contentTypes;
    private readonly IDataTypeService _dataTypes;
    private readonly IFileService _files;
    private readonly IContentService _content;
    private readonly IMediaService _media;
    private readonly MediaFileManager _mediaFiles;
    private readonly MediaUrlGeneratorCollection _urlGenerators;
    private readonly IContentTypeBaseServiceProvider _typeProvider;
    private readonly PropertyEditorCollection _editors;
    private readonly IConfigurationEditorJsonSerializer _serializer;
    private readonly IShortStringHelper _sh;
    private readonly IRuntimeState _runtime;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DigitalLibrarySetupHandler> _logger;

    public DigitalLibrarySetupHandler(IContentTypeService contentTypes, IDataTypeService dataTypes, IFileService files,
        IContentService content, IMediaService media, MediaFileManager mediaFiles, MediaUrlGeneratorCollection urlGenerators,
        IContentTypeBaseServiceProvider typeProvider, PropertyEditorCollection editors,
        IConfigurationEditorJsonSerializer serializer, IShortStringHelper sh, IRuntimeState runtime,
        IWebHostEnvironment env, ILogger<DigitalLibrarySetupHandler> logger)
    {
        _contentTypes = contentTypes;
        _dataTypes = dataTypes;
        _files = files;
        _content = content;
        _media = media;
        _mediaFiles = mediaFiles;
        _urlGenerators = urlGenerators;
        _typeProvider = typeProvider;
        _editors = editors;
        _serializer = serializer;
        _sh = sh;
        _runtime = runtime;
        _env = env;
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
            EnsureSchema();
            var home = _content.GetRootContent().FirstOrDefault(x => x.ContentType.Alias == DocTypes.Home);
            if (home == null) return;
            if (_content.GetPagedChildren(home.Id, 0, 500, out _).Any(x => x.ContentType.Alias == DocTypes.LibraryRoot))
                return;
            SeedLibrary(home);
        }
        catch (Exception ex)
        {
            _done = 0;
            _logger.LogError(ex, "Không thể khởi tạo Thư viện số");
        }
    }

    // ================= Schema =================

    private void EnsureSchema()
    {
        var tLibrary = EnsureTemplate(LibraryTemplate);
        var tDetail = _files.GetTemplate(DetailTemplate);
        var periodPicker = EnsurePeriodPicker();

        var pdf = EnsureType(DocTypes.PdfDocument, "Tài liệu PDF / Sách", "icon-book-alt", tDetail, ct =>
        {
            Prop(ct, GroupContent, GroupContentName, Props.Summary, "Tóm tắt", "Textarea");
            Prop(ct, GroupContent, GroupContentName, LibraryProps.Thumbnail, "Ảnh bìa", "Image Media Picker");
            Prop(ct, GroupContent, GroupContentName, LibraryProps.PdfFile, "File PDF", "Media Picker");
            Prop(ct, GroupContent, GroupContentName, LibraryProps.Author, "Tác giả / Nhà xuất bản", "Textstring");
            Prop(ct, GroupContent, GroupContentName, Props.BodyText, "Giới thiệu", "Richtext editor");
            Prop(ct, GroupContent, GroupContentName, "publishDate", "Ngày đăng", "Date Picker with time");
            Prop(ct, GroupContent, GroupContentName, Props.Source, "Nguồn", "Textstring");
        });
        var topic = EnsureType(DocTypes.PeriodTopic, "Chuyên mục giai đoạn", "icon-folder", tLibrary, ct =>
        {
            Prop(ct, GroupContent, GroupContentName, Props.Description, "Mô tả", "Textarea");
            Prop(ct, GroupContent, GroupContentName, LibraryProps.Thumbnail, "Ảnh đại diện", "Image Media Picker");
            Prop(ct, GroupContent, GroupContentName, LibraryProps.Hide, "Ẩn", "True/false");
        });
        var period = EnsureType(DocTypes.Period, "Giai đoạn lịch sử", "icon-calendar", tLibrary, ct =>
        {
            Prop(ct, GroupContent, GroupContentName, Props.Description, "Mô tả", "Textarea");
            Prop(ct, GroupContent, GroupContentName, LibraryProps.Thumbnail, "Ảnh đại diện", "Image Media Picker");
            Prop(ct, GroupContent, GroupContentName, LibraryProps.FromYear, "Từ năm", "Numeric");
            Prop(ct, GroupContent, GroupContentName, LibraryProps.ToYear, "Đến năm", "Numeric");
        });
        var root = EnsureType(DocTypes.LibraryRoot, "Thư viện số", "icon-books", tLibrary, ct =>
        {
            Prop(ct, GroupContent, GroupContentName, Props.Description, "Mô tả", "Textarea");
            Prop(ct, GroupContent, GroupContentName, LibraryProps.Hide, "Ẩn khỏi menu", "True/false");
        });

        var itemTypes = ItemTypesWithLibraryProps.Select(a => _contentTypes.Get(a)).OfType<IContentType>().Append(pdf).ToList();
        foreach (var ct in itemTypes)
        {
            var changed = Prop(ct, GroupLibrary, GroupLibraryName, LibraryProps.EventYear, "Năm của tư liệu", "Numeric");
            changed |= Prop(ct, GroupLibrary, GroupLibraryName, LibraryProps.Periods, "Gắn vào giai đoạn (liên kết chéo)", periodPicker);
            if (changed) _contentTypes.Save(ct);
        }

        Allow(root, period);
        Allow(period, topic);
        Allow(topic, itemTypes.ToArray());
        if (_contentTypes.Get(DocTypes.Home) is { } home) Allow(home, root);
    }

    private ITemplate EnsureTemplate(string name)
    {
        var alias = name.ToLowerInvariant();
        var existing = _files.GetTemplate(alias);
        if (existing != null) return existing;
        var path = Path.Combine(_env.ContentRootPath, "Views", name + ".cshtml");
        var body = System.IO.File.Exists(path) ? System.IO.File.ReadAllText(path) : null;
        return _files.CreateTemplateWithIdentity(name, alias, body);
    }

    private IDataType EnsurePeriodPicker()
    {
        var existing = _dataTypes.GetAll().FirstOrDefault(d => d.Name == PeriodPickerName);
        if (existing != null) return existing;
        var editor = _editors[Constants.PropertyEditors.Aliases.MultiNodeTreePicker]!;
        var dt = new DataType(editor, _serializer)
        {
            Name = PeriodPickerName,
            Configuration = new MultiNodePickerConfiguration
            {
                TreeSource = new MultiNodePickerConfigurationTreeSource { ObjectType = "content" },
                Filter = DocTypes.Period,
            },
        };
        _dataTypes.Save(dt);
        return dt;
    }

    private IContentType EnsureType(string alias, string name, string icon, ITemplate? template, Action<IContentType> configure)
    {
        var existing = _contentTypes.Get(alias);
        if (existing != null) return existing;
        var ct = new ContentType(_sh, -1) { Alias = alias, Name = name, Icon = icon };
        if (template != null)
        {
            ct.AllowedTemplates = new[] { template };
            ct.SetDefaultTemplate(template);
        }
        configure(ct);
        _contentTypes.Save(ct);
        return ct;
    }

    private bool Prop(IContentType ct, string group, string groupName, string alias, string name, string dataTypeName)
        => Prop(ct, group, groupName, alias, name, _dataTypes.GetAll().First(d => d.Name == dataTypeName));

    private bool Prop(IContentType ct, string group, string groupName, string alias, string name, IDataType dt)
    {
        if (ct.PropertyTypeExists(alias)) return false;
        ct.AddPropertyType(new PropertyType(_sh, dt, alias) { Name = name }, group, groupName);
        return true;
    }

    private void Allow(IContentType parent, params IContentType[] children)
    {
        var current = parent.AllowedContentTypes?.ToList() ?? new List<ContentTypeSort>();
        var missing = children.Where(c => current.All(s => s.Alias != c.Alias)).ToList();
        if (missing.Count == 0) return;
        current.AddRange(missing.Select((c, i) => new ContentTypeSort(new Lazy<int>(() => c.Id), current.Count + i, c.Alias)));
        parent.AllowedContentTypes = current;
        _contentTypes.Save(parent);
    }

    // ================= Nội dung demo =================

    private void SeedLibrary(IContent home)
    {
        var images = MediaIn(ImageFolder);
        var videos = MediaIn(AvFolder).Where(m => m.ContentType.Alias == VideoMediaType).ToList();
        var audios = MediaIn(AvFolder).Where(m => m.ContentType.Alias == AudioMediaType).ToList();
        if (videos.Count == 0 || audios.Count == 0)
        {
            var uploaded = UploadDemoAv();
            videos = uploaded.Where(m => m.ContentType.Alias == VideoMediaType).ToList();
            audios = uploaded.Where(m => m.ContentType.Alias == AudioMediaType).ToList();
        }
        var pdf = UploadDemoPdf();
        var n = 0;

        var root = Create(LibrarySeed.RootName, home, DocTypes.LibraryRoot, c => Set(c, Props.Description, LibrarySeed.RootDescription));
        var periodNodes = new List<IContent>();
        foreach (var p in LibrarySeed.Periods)
        {
            var period = Create(p.Name, root, DocTypes.Period, c =>
            {
                Set(c, Props.Description, p.Description);
                Set(c, LibraryProps.FromYear, p.From);
                Set(c, LibraryProps.ToYear, p.To);
                if (Next(images, ref n) is { } img) Set(c, LibraryProps.Thumbnail, Pick(img));
            });
            periodNodes.Add(period);

            foreach (var topicName in LibrarySeed.Topics)
            {
                var topic = Create(topicName, period, DocTypes.PeriodTopic, c => Set(c, Props.Description, $"{topicName} – {p.Name}"));
                foreach (var item in p.Items.Where(i => i.Topic == topicName))
                {
                    Create(item.Title, topic, item.Type, c =>
                    {
                        Set(c, Props.Summary, item.Summary);
                        Set(c, Props.BodyText, LibrarySeed.Body(p, item));
                        Set(c, Props.Source, LibrarySeed.AuthorDemo);
                        Set(c, "publishDate", DateTime.Now.AddDays(-n));
                        Set(c, LibraryProps.EventYear, item.Year);
                        if (Next(images, ref n) is { } img) Set(c, LibraryProps.Thumbnail, Pick(img));
                        if (item.Type == DocTypes.Video && Next(videos, ref n) is { } v) Set(c, LibraryProps.VideoFile, Pick(v));
                        if (item.Type == DocTypes.Audio && Next(audios, ref n) is { } a) Set(c, LibraryProps.AudioFile, Pick(a));
                        if (item.Type == DocTypes.PdfDocument && pdf != null)
                        {
                            Set(c, LibraryProps.PdfFile, Pick(pdf));
                            Set(c, LibraryProps.Author, LibrarySeed.AuthorDemo);
                        }
                    });
                }
            }
        }
        CrossLink(home, periodNodes);
        _logger.LogInformation("Đã tạo Thư viện số với {Count} giai đoạn", periodNodes.Count);
    }

    private void CrossLink(IContent home, List<IContent> periods)
    {
        var all = _content.GetPagedDescendants(home.Id, 0, 10000, out _)
            .Where(c => c.ContentType.Alias == DocTypes.Article && c.Published).ToList();
        foreach (var (part, indexes) in LibrarySeed.CrossLinks)
        {
            var udis = string.Join(",", indexes.Select(i => periods[i].GetUdi().ToString()));
            foreach (var c in all.Where(c => (c.Name ?? "").Contains(part)))
            {
                Set(c, LibraryProps.Periods, udis);
                Publish(c);
            }
        }
    }

    private IContent Create(string name, IContent parent, string type, Action<IContent> set)
    {
        var c = _content.Create(name, parent.Id, type);
        if (c.ContentType.VariesByCulture()) c.SetCultureName(name, Cultures.Vi);
        set(c);
        Publish(c);
        return c;
    }

    private void Publish(IContent c)
    {
        var result = c.ContentType.VariesByCulture() ? _content.SaveAndPublish(c, Cultures.Vi) : _content.SaveAndPublish(c);
        if (!result.Success) _logger.LogWarning("Không publish được {Name}: {Result}", c.Name, result.Result);
    }

    private static void Set(IContent c, string alias, object? value)
    {
        if (!c.Properties.TryGetValue(alias, out var prop) || prop == null) return;
        var pt = prop.PropertyType;
        if (pt == null) return;
        if (pt.VariesByCulture()) c.SetValue(alias, value, Cultures.Vi);
        else c.SetValue(alias, value);
    }

    private static T? Next<T>(List<T> list, ref int n) where T : class
        => list.Count == 0 ? null : list[n++ % list.Count];

    private static string Pick(IMedia m) => JsonSerializer.Serialize(new[] { new { key = Guid.NewGuid(), mediaKey = m.Key } });

    private List<IMedia> MediaIn(string folderName)
    {
        var folder = _media.GetRootMedia().FirstOrDefault(m => m.Name == folderName);
        return folder == null ? new() : _media.GetPagedChildren(folder.Id, 0, 500, out _).ToList();
    }

    private IMedia Folder(string name)
    {
        var folder = _media.GetRootMedia().FirstOrDefault(m => m.Name == name);
        if (folder != null) return folder;
        folder = _media.CreateMedia(name, Constants.System.Root, Constants.Conventions.MediaTypes.Folder);
        _media.Save(folder);
        return folder;
    }

    private IMedia Upload(IMedia folder, string file, string mediaType)
    {
        var m = _media.CreateMedia(Path.GetFileNameWithoutExtension(file), folder.Id, mediaType);
        using (var fs = System.IO.File.OpenRead(file))
            m.SetValue(_mediaFiles, _urlGenerators, _sh, _typeProvider, Constants.Conventions.Media.File, Path.GetFileName(file), fs);
        _media.Save(m);
        return m;
    }

    private string DemoDir => Path.Combine(_env.ContentRootPath, "Setup", DemoMediaDir);

    private List<IMedia> UploadDemoAv()
    {
        if (!Directory.Exists(DemoDir)) return new();
        var folder = Folder(AvFolder);
        var result = new List<IMedia>();
        foreach (var f in Directory.GetFiles(DemoDir, "*.mp4").OrderBy(f => f)) result.Add(Upload(folder, f, VideoMediaType));
        foreach (var f in Directory.GetFiles(DemoDir, "*.mp3").OrderBy(f => f)) result.Add(Upload(folder, f, AudioMediaType));
        return result;
    }

    private IMedia? UploadDemoPdf()
    {
        var existing = MediaIn(PdfFolder).FirstOrDefault();
        if (existing != null) return existing;
        var file = Path.Combine(DemoDir, LibrarySeed.DemoPdf);
        return System.IO.File.Exists(file) ? Upload(Folder(PdfFolder), file, Constants.Conventions.MediaTypes.File) : null;
    }
}
