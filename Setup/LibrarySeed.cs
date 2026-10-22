using HcmPortal.Infrastructure;

namespace HcmPortal.Setup;

public record SeedItem(string Topic, string Type, string Title, int Year, string Summary);

public record SeedPeriod(string Name, int From, int To, string Description, SeedItem[] Items);

/// <summary>
/// Dữ liệu demo cho cây thư mục Thư viện số. Tên và mốc năm 5 giai đoạn phải khớp tài liệu của khách hàng;
/// nội dung tư liệu là bản demo, Ban biên tập rà soát trước khi dùng chính thức.
/// </summary>
public static class LibrarySeed
{
    public const string RootName = "Thư viện số";
    public const string RootDescription = "Kho tư liệu về cuộc đời, sự nghiệp Chủ tịch Hồ Chí Minh, tổ chức theo 5 giai đoạn lịch sử.";

    public const string TopicEvents = "Sự kiện";
    public const string TopicActivities = "Hoạt động";
    public const string TopicPlaces = "Địa danh";
    public const string TopicPeople = "Nhân vật";
    public const string TopicDocuments = "Tư liệu";

    public static readonly string[] Topics = { TopicEvents, TopicActivities, TopicPlaces, TopicPeople, TopicDocuments };

    public const string DemoPdf = "tai-lieu-demo.pdf";
    public const string AuthorDemo = "Ban Biên tập (bản demo)";

    public static readonly SeedPeriod[] Periods =
    {
        new("1890–1911: Thời niên thiếu và hình thành chí hướng", 1890, 1911,
            "Tuổi thơ trên quê hương Nghệ An, những năm học tập ở Huế và hình thành ý chí ra đi tìm đường cứu nước.",
            new SeedItem[]
            {
                new(TopicEvents, DocTypes.Article, "Ngày 19/5/1890: Người ra đời tại làng Hoàng Trù", 1890,
                    "Mốc khởi đầu cuộc đời Chủ tịch Hồ Chí Minh tại quê ngoại, xã Kim Liên, huyện Nam Đàn, tỉnh Nghệ An."),
                new(TopicActivities, DocTypes.Article, "Những năm học tập tại Trường Quốc học Huế", 1907,
                    "Thời gian học tập ở Huế giúp người thanh niên tiếp xúc với tư tưởng mới và nuôi dưỡng lòng yêu nước."),
                new(TopicPlaces, DocTypes.Article, "Làng Sen – Kim Liên, quê hương thời niên thiếu", 1895,
                    "Giới thiệu cụm di tích quê nội, quê ngoại gắn với tuổi thơ của Người."),
                new(TopicPeople, DocTypes.Article, "Người cha – người thầy đầu tiên", 1901,
                    "Ảnh hưởng của gia đình nhà nho yêu nước tới nhân cách và chí hướng của người con."),
                new(TopicDocuments, DocTypes.PdfDocument, "Tập tư liệu: Thời niên thiếu", 1911,
                    "Tập tư liệu điện tử tổng hợp các mốc chính giai đoạn 1890–1911 (bản demo)."),
                new(TopicDocuments, DocTypes.Video, "Phim tư liệu: Quê hương Nam Đàn", 1900,
                    "Phim ngắn giới thiệu quê hương Nam Đàn (video demo)."),
                new(TopicDocuments, DocTypes.Audio, "Sách nói: Tuổi thơ ở làng Sen", 1895,
                    "Sách nói kể lại những câu chuyện tuổi thơ của Người (âm thanh demo)."),
            }),
        new("1911–1920: Tìm đường cứu nước", 1911, 1920,
            "Rời bến Nhà Rồng, đi qua nhiều quốc gia, vừa lao động vừa học tập để tìm con đường giải phóng dân tộc.",
            new SeedItem[]
            {
                new(TopicEvents, DocTypes.Article, "Ngày 5/6/1911: Rời bến Nhà Rồng ra đi tìm đường cứu nước", 1911,
                    "Người thanh niên Nguyễn Tất Thành rời Tổ quốc, bắt đầu hành trình qua nhiều châu lục."),
                new(TopicActivities, DocTypes.Article, "Vừa lao động vừa học tập ở nước ngoài", 1915,
                    "Những năm làm nhiều nghề khác nhau để sống và tìm hiểu đời sống nhân dân lao động các nước."),
                new(TopicPlaces, DocTypes.Article, "Bến Nhà Rồng – nơi bắt đầu hành trình", 1911,
                    "Di tích gắn với ngày ra đi tìm đường cứu nước, nay là Bảo tàng Hồ Chí Minh – chi nhánh TP. Hồ Chí Minh."),
                new(TopicPeople, DocTypes.Article, "Những người bạn trên các chuyến tàu viễn dương", 1912,
                    "Tình bạn với những người lao động gặp gỡ trên hành trình vượt đại dương."),
                new(TopicDocuments, DocTypes.PdfDocument, "Tập tư liệu: Hành trình tìm đường cứu nước", 1920,
                    "Tập tư liệu điện tử về giai đoạn 1911–1920 (bản demo)."),
                new(TopicDocuments, DocTypes.Video, "Phim tư liệu: Vượt đại dương", 1913,
                    "Phim ngắn về hành trình qua các đại dương (video demo)."),
                new(TopicDocuments, DocTypes.Audio, "Sách nói: Những chuyến đi xa", 1914,
                    "Sách nói về những năm tháng bôn ba nơi đất khách (âm thanh demo)."),
            }),
        new("1920–1930: Hoạt động quốc tế và chuẩn bị thành lập Đảng", 1920, 1930,
            "Tìm thấy con đường cách mạng vô sản, hoạt động trong phong trào quốc tế và chuẩn bị về tư tưởng, tổ chức cho việc thành lập Đảng.",
            new SeedItem[]
            {
                new(TopicEvents, DocTypes.Article, "Ngày 3/2/1930: Hội nghị thành lập Đảng", 1930,
                    "Hội nghị hợp nhất các tổ chức cộng sản, thành lập Đảng Cộng sản Việt Nam."),
                new(TopicActivities, DocTypes.Article, "Năm 1925: Thành lập Hội Việt Nam Cách mạng Thanh niên", 1925,
                    "Tổ chức tiền thân, đào tạo cán bộ và truyền bá lý luận cách mạng về trong nước."),
                new(TopicPlaces, DocTypes.Article, "Quảng Châu – nơi mở lớp huấn luyện cán bộ", 1926,
                    "Địa điểm diễn ra các lớp huấn luyện cán bộ cách mạng đầu tiên."),
                new(TopicPeople, DocTypes.Article, "Những học viên đầu tiên của Hội Thanh niên", 1926,
                    "Lớp thanh niên yêu nước được đào tạo, trở thành lực lượng nòng cốt về sau."),
                new(TopicDocuments, DocTypes.PdfDocument, "Tập tư liệu: Chuẩn bị thành lập Đảng", 1930,
                    "Tập tư liệu điện tử về giai đoạn 1920–1930 (bản demo)."),
                new(TopicDocuments, DocTypes.Video, "Phim tư liệu: Mùa xuân năm 1930", 1930,
                    "Phim ngắn về sự kiện thành lập Đảng (video demo)."),
                new(TopicDocuments, DocTypes.Audio, "Sách nói: Con đường đến với lý luận cách mạng", 1920,
                    "Sách nói về bước ngoặt tư tưởng năm 1920 (âm thanh demo)."),
            }),
        new("1930–1945: Chuẩn bị và lãnh đạo Cách mạng Tháng Tám", 1930, 1945,
            "Trở về nước, trực tiếp lãnh đạo phong trào, chuẩn bị lực lượng và lãnh đạo Tổng khởi nghĩa giành chính quyền.",
            new SeedItem[]
            {
                new(TopicEvents, DocTypes.Article, "Ngày 2/9/1945: Tuyên ngôn độc lập tại Quảng trường Ba Đình", 1945,
                    "Khai sinh nước Việt Nam Dân chủ Cộng hòa."),
                new(TopicActivities, DocTypes.Article, "Năm 1941: Về nước, sống và làm việc tại Pác Bó", 1941,
                    "Trực tiếp lãnh đạo phong trào cách mạng, thành lập Mặt trận Việt Minh."),
                new(TopicPlaces, DocTypes.Article, "Tân Trào – thủ đô khu giải phóng", 1945,
                    "Nơi diễn ra Quốc dân Đại hội, quyết định Tổng khởi nghĩa tháng 8/1945."),
                new(TopicPeople, DocTypes.Article, "Đồng bào Pác Bó che chở cán bộ cách mạng", 1941,
                    "Sự đùm bọc của đồng bào các dân tộc vùng biên giới đối với cách mạng."),
                new(TopicDocuments, DocTypes.PdfDocument, "Tập tư liệu: Cách mạng Tháng Tám", 1945,
                    "Tập tư liệu điện tử về giai đoạn 1930–1945 (bản demo)."),
                new(TopicDocuments, DocTypes.Video, "Phim tư liệu: Mùa thu năm 1945", 1945,
                    "Phim ngắn về những ngày Cách mạng Tháng Tám (video demo)."),
                new(TopicDocuments, DocTypes.Audio, "Sách nói: Những ngày ở lán Nà Nưa", 1945,
                    "Sách nói về thời gian chuẩn bị Tổng khởi nghĩa (âm thanh demo)."),
            }),
        new("1945–1969: Lãnh đạo kháng chiến và xây dựng đất nước", 1945, 1969,
            "Lãnh đạo xây dựng chính quyền mới, hai cuộc kháng chiến và công cuộc xây dựng đất nước; để lại bản Di chúc lịch sử.",
            new SeedItem[]
            {
                new(TopicEvents, DocTypes.Article, "Năm 1965: Bắt đầu viết bản Di chúc", 1965,
                    "Bản Di chúc được viết và bổ sung trong nhiều năm, là di sản tinh thần vô giá."),
                new(TopicActivities, DocTypes.Article, "Năm 1954: Trở về Thủ đô", 1954,
                    "Trở về Hà Nội, cùng Trung ương lãnh đạo công cuộc khôi phục và xây dựng đất nước."),
                new(TopicPlaces, DocTypes.Article, "Nhà sàn trong khu Phủ Chủ tịch", 1958,
                    "Nơi ở và làm việc giản dị từ năm 1958."),
                new(TopicPeople, DocTypes.Article, "Những người phục vụ bên Bác", 1960,
                    "Câu chuyện về những người từng sống và làm việc bên Chủ tịch Hồ Chí Minh."),
                new(TopicDocuments, DocTypes.PdfDocument, "Tập tư liệu: Kháng chiến và kiến quốc", 1969,
                    "Tập tư liệu điện tử về giai đoạn 1945–1969 (bản demo)."),
                new(TopicDocuments, DocTypes.Video, "Phim tư liệu: Ngôi nhà sàn", 1958,
                    "Phim ngắn giới thiệu ngôi nhà sàn (video demo)."),
                new(TopicDocuments, DocTypes.Audio, "Sách nói: Bác Hồ với thiếu nhi", 1960,
                    "Sách nói những câu chuyện về Bác với thiếu nhi (âm thanh demo)."),
            }),
    };

    /// <summary>Bài Biên niên có sẵn được gắn chéo vào giai đoạn tương ứng (FR-10). Khoá: một phần tiêu đề bài.</summary>
    public static readonly (string TitlePart, int[] PeriodIndexes)[] CrossLinks =
    {
        ("1890 – 1911", new[] { 0 }),
        ("1911 – 1930", new[] { 1, 2 }),
        ("1930 – 1945", new[] { 3 }),
        ("1945 – 1969", new[] { 4 }),
        ("Tóm tắt tiểu sử", new[] { 0, 1, 2, 3, 4 }),
    };

    public static string Body(SeedPeriod p, SeedItem i) =>
        $"<p>{i.Summary}</p>" +
        $"<h3>Bối cảnh giai đoạn {p.From}–{p.To}</h3><p>{p.Description}</p>" +
        "<p>Nội dung chi tiết sẽ được Ban biên tập bổ sung từ nguồn tư liệu chính thức.</p>";
}
