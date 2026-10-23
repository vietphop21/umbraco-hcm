using System.Net;
using HcmPortal.Infrastructure;

namespace HcmPortal.Setup;

/// <summary>
/// Nội dung trang tĩnh viết từ Đề xuất site map (21/10/2026); chỉ nêu mục đích, cấu trúc và tiện ích,
/// không đưa dữ kiện lịch sử. Ban biên tập có thể sửa lại trong backoffice.
/// </summary>
public static class SiteMapContent
{
    public const string ContactBody =
        "<p>Mọi ý kiến đóng góp, đề nghị cung cấp hoặc bổ sung tư liệu xin gửi về Ban Biên tập theo biểu mẫu dưới đây.</p>";

    public static string IntroBody()
    {
        var sections = string.Concat(SiteMap.Sections
            .Where(s => s.UrlName != SiteMap.Intro)
            .Select(s => $"<li><a href=\"/{s.UrlName}/\"><strong>{WebUtility.HtmlEncode(s.Name)}</strong></a> – {WebUtility.HtmlEncode(s.Description)}</li>"));

        return
            "<p><strong>Không gian Văn hóa Hồ Chí Minh trên nền tảng số</strong> là cơ sở dữ liệu chuyên đề do " +
            $"{SiteMap.OrgName} xây dựng, tập hợp sách, tài liệu, hình ảnh, phim và sách nói về cuộc đời, sự nghiệp, " +
            "tư tưởng, đạo đức, phong cách Chủ tịch Hồ Chí Minh, phục vụ bạn đọc tra cứu, học tập và nghiên cứu.</p>" +
            "<h2>Nội dung chính</h2><ul>" + sections + "</ul>" +
            "<h2>Tiện ích</h2><ul>" +
            $"<li><a href=\"/{SiteMap.Search}/\">Tra cứu đa tiêu chí</a>: tìm theo nhan đề, tác giả, năm, loại hình và giai đoạn lịch sử.</li>" +
            $"<li><a href=\"/{SiteMap.Periods}/\">Cây thư mục giai đoạn lịch sử</a>: duyệt tư liệu theo từng giai đoạn.</li>" +
            "<li>Thanh trợ năng: phóng chữ, tương phản cao, đọc nội dung bằng giọng nói.</li>" +
            "<li>Đọc tài liệu PDF trực tuyến, xem phim tư liệu và nghe sách nói ngay trên trang.</li>" +
            "</ul>";
    }
}
