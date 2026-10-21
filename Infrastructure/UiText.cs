namespace HcmPortal.Infrastructure;

/// <summary>Chữ cố định của giao diện bản tiếng Anh. Khoá là câu tiếng Việt; câu chưa dịch giữ nguyên tiếng Việt.</summary>
public static class UiText
{
    public static readonly string[] ViDays = { "Chủ nhật", "Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy" };
    public static readonly string[] EnDays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

    public static readonly IReadOnlyDictionary<string, string> En = new Dictionary<string, string>
    {
        ["Trang chủ"] = "Home",
        ["Tìm kiếm"] = "Search",
        ["Tìm"] = "Search",
        ["Nhập từ khóa tìm kiếm (có dấu hoặc không dấu)..."] = "Enter search keywords...",
        ["Nhập từ khóa (có dấu hoặc không dấu)..."] = "Enter keywords...",
        ["Tìm thấy"] = "Found",
        ["kết quả cho"] = "results for",
        ["Liên kết website"] = "Related websites",
        ["-- Chọn liên kết --"] = "-- Select a link --",
        ["Địa chỉ"] = "Address",
        ["Lên đầu trang"] = "Back to top",
        ["Xem chi tiết »"] = "Read more »",
        ["Chuyên mục chưa có bài viết."] = "No articles in this section yet.",
        ["Giảm cỡ chữ"] = "Smaller text",
        ["Cỡ chữ mặc định"] = "Default text size",
        ["Tăng cỡ chữ"] = "Larger text",
        ["In bài viết"] = "Print",
        ["Gửi email"] = "Email",
        ["Sao chép liên kết"] = "Copy link",
        ["Đã sao chép"] = "Copied",
        ["Video đang được cập nhật"] = "Video coming soon",
        ["Nghe audio"] = "Listen",
        ["File audio đang được cập nhật."] = "Audio coming soon.",
        ["Tin cùng chuyên mục"] = "Related articles",
        ["Trước"] = "Previous",
        ["Sau"] = "Next",
        ["Biên niên tiểu sử"] = "Chronology",
        ["Cảm ơn bạn! Thư đã được gửi tới ban biên tập."] = "Thank you! Your message has been sent to the editorial team.",
        ["Họ và tên *"] = "Full name *",
        ["Số điện thoại"] = "Phone number",
        ["Nội dung *"] = "Message *",
        ["Gửi liên hệ"] = "Send",
        ["Phân trang"] = "Pagination",
        ["Trang đầu"] = "First page",
        ["Trang trước"] = "Previous page",
        ["Trang sau"] = "Next page",
        ["Trang cuối"] = "Last page",
        ["Chế độ sáng/tối"] = "Light/dark mode",
        ["Chuyển sang tiếng Anh"] = "Switch to Vietnamese",
    };

    /// <summary>Tên tiếng Anh cho khung menu và trang tĩnh; bài viết do biên tập viên tự dịch.</summary>
    public static readonly IReadOnlyDictionary<string, string> EnNodeNames = new Dictionary<string, string>
    {
        ["Trang chủ"] = "Home",
        ["Tin tức"] = "News",
        ["Cuộc đời, sự nghiệp"] = "Life and Career",
        ["Biên niên tiểu sử"] = "Chronology",
        ["Tiểu sử"] = "Biography",
        ["Tư tưởng, đạo đức, phong cách Hồ Chí Minh"] = "Ho Chi Minh's Thought, Ethics and Style",
        ["Nghiên cứu"] = "Research",
        ["Nguồn gốc hình thành"] = "Origins",
        ["Nội dung tư tưởng đạo đức"] = "Ethical Thought",
        ["Tủ sách điện tử Hồ Chí Minh"] = "Ho Chi Minh E-Library",
        ["Tác phẩm của Hồ Chí Minh"] = "Works by Ho Chi Minh",
        ["Tác phẩm về Hồ Chí Minh"] = "Works about Ho Chi Minh",
        ["Hồ Chí Minh và thế giới"] = "Ho Chi Minh and the World",
        ["Bạn bè quốc tế"] = "International Friends",
        ["Hoạt động quốc tế"] = "International Activities",
        ["Học và làm theo Bác"] = "Following Uncle Ho's Example",
        ["Di chúc"] = "Testament",
        ["Gương điển hình"] = "Role Models",
        ["Văn bản"] = "Documents",
        ["Bảo tàng 3D"] = "3D Museum",
        ["Tư liệu Ảnh"] = "Photo Archive",
        ["Tư liệu Audio"] = "Audio Archive",
        ["Tư liệu Video"] = "Video Archive",
        ["Infographics"] = "Infographics",
        ["Megastory"] = "Megastory",
        ["Giới thiệu"] = "About",
        ["Liên hệ"] = "Contact",
        ["Sơ đồ website"] = "Sitemap",
        ["Tìm kiếm"] = "Search",
    };

    public const string EnSlogan = "Documentation – Learning – Research Portal";
}
