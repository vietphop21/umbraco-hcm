namespace HcmPortal.Setup;

/// <summary>
/// Một số bài dài viết riêng (tự biên soạn từ các mốc lịch sử phổ thông) cho các mục quan trọng.
/// Key = tiêu đề bài trong SeedData. Bài không có trong đây sẽ dùng nội dung ghép tự động.
/// </summary>
public static class SeedArticles
{
    public static readonly Dictionary<string, (string Summary, string Body)> Rich = new()
    {
        ["Tóm tắt tiểu sử Chủ tịch Hồ Chí Minh"] = (
            "Chủ tịch Hồ Chí Minh (1890 – 1969), lúc nhỏ tên là Nguyễn Sinh Cung, quê ở làng Kim Liên, huyện Nam Đàn, tỉnh Nghệ An; là người sáng lập Đảng Cộng sản Việt Nam và nước Việt Nam Dân chủ Cộng hòa.",
            "<h3>Thời niên thiếu</h3>" +
            "<p>Người sinh ngày 19/5/1890 trong một gia đình nhà nho yêu nước ở xứ Nghệ. Tuổi thơ gắn với làng Hoàng Trù (quê ngoại) và làng Sen (quê nội), sau đó theo cha vào Huế học tập. Những năm tháng ấy giúp Người sớm chứng kiến cảnh nước mất, nhà tan và nuôi dưỡng chí hướng cứu nước.</p>" +
            "<h3>Ra đi tìm đường cứu nước</h3>" +
            "<p>Ngày 5/6/1911, từ bến cảng Nhà Rồng (Sài Gòn), người thanh niên Nguyễn Tất Thành rời Tổ quốc, bắt đầu hành trình kéo dài gần 30 năm qua nhiều quốc gia ở châu Âu, châu Phi, châu Mỹ và châu Á. Người vừa lao động kiếm sống, vừa học tập, khảo sát đời sống nhân dân lao động các nước.</p>" +
            "<div class=\"note-box\">Năm 1920, Người tiếp cận con đường cách mạng vô sản; năm 1930 chủ trì hội nghị hợp nhất các tổ chức cộng sản, thành lập Đảng Cộng sản Việt Nam.</div>" +
            "<p>{FIGURE}</p>" +
            "<h3>Trở về Tổ quốc, lãnh đạo cách mạng</h3>" +
            "<p>Đầu năm 1941, Người về nước, sống và làm việc tại Pác Bó (Cao Bằng), trực tiếp lãnh đạo phong trào cách mạng. Ngày 2/9/1945, tại Quảng trường Ba Đình (Hà Nội), Người đọc bản Tuyên ngôn độc lập, khai sinh nước Việt Nam Dân chủ Cộng hòa.</p>" +
            "<p>Trong các cuộc kháng chiến sau đó, Người cùng Trung ương lãnh đạo toàn dân vừa kháng chiến vừa kiến quốc. Người qua đời ngày 2/9/1969 tại Hà Nội, để lại bản Di chúc lịch sử và di sản tinh thần vô giá cho dân tộc.</p>" +
            "<p><em>Bài viết tóm tắt phục vụ demo; nội dung chính thức do Ban biên tập cập nhật.</em></p>"),

        ["Biên niên sự kiện giai đoạn 1890 – 1911"] = (
            "Các mốc chính từ khi Người ra đời tại Nghệ An đến khi rời Tổ quốc ra đi tìm đường cứu nước.",
            "<ul><li><b>19/5/1890</b>: Sinh tại làng Hoàng Trù, xã Kim Liên, huyện Nam Đàn, Nghệ An.</li>" +
            "<li><b>1895 – 1901</b>: Theo gia đình vào Huế, sau đó trở về quê.</li>" +
            "<li><b>1906 – 1909</b>: Học tại Trường Quốc học Huế.</li>" +
            "<li><b>1910</b>: Dạy học tại Trường Dục Thanh (Phan Thiết).</li>" +
            "<li><b>5/6/1911</b>: Rời bến Nhà Rồng ra đi tìm đường cứu nước.</li></ul>" +
            "<p>{FIGURE}</p><p>Giai đoạn này hình thành nền tảng văn hóa, lòng yêu nước và ý chí tự lập – những yếu tố quan trọng cho hành trình cách mạng về sau.</p>"),

        ["Biên niên sự kiện giai đoạn 1911 – 1930"] = (
            "Hành trình qua nhiều châu lục, tìm thấy con đường cứu nước và chuẩn bị thành lập Đảng.",
            "<ul><li><b>1911 – 1917</b>: Làm nhiều nghề, đến nhiều nước ở châu Âu, châu Phi, châu Mỹ.</li>" +
            "<li><b>1919</b>: Gửi Bản yêu sách của nhân dân An Nam tới Hội nghị Véc-xây.</li>" +
            "<li><b>1920</b>: Tham gia sáng lập Đảng Cộng sản Pháp.</li>" +
            "<li><b>1925</b>: Thành lập Hội Việt Nam Cách mạng Thanh niên.</li>" +
            "<li><b>3/2/1930</b>: Chủ trì hội nghị thành lập Đảng Cộng sản Việt Nam.</li></ul><p>{FIGURE}</p>"),

        ["Biên niên sự kiện giai đoạn 1930 – 1945"] = (
            "Từ khi thành lập Đảng đến Cách mạng Tháng Tám và ngày khai sinh nước Việt Nam Dân chủ Cộng hòa.",
            "<ul><li><b>28/1/1941</b>: Về nước, sống và làm việc tại Pác Bó (Cao Bằng).</li>" +
            "<li><b>5/1941</b>: Chủ trì Hội nghị Trung ương 8, thành lập Mặt trận Việt Minh.</li>" +
            "<li><b>8/1945</b>: Tại Tân Trào (Tuyên Quang), Quốc dân Đại hội quyết định Tổng khởi nghĩa.</li>" +
            "<li><b>2/9/1945</b>: Đọc Tuyên ngôn độc lập tại Quảng trường Ba Đình.</li></ul><p>{FIGURE}</p>"),

        ["Biên niên sự kiện giai đoạn 1945 – 1969"] = (
            "Lãnh đạo xây dựng chính quyền mới, kháng chiến và kiến quốc; để lại bản Di chúc lịch sử.",
            "<ul><li><b>1946</b>: Kêu gọi toàn quốc kháng chiến.</li>" +
            "<li><b>1954</b>: Trở về Thủ đô, làm việc tại Phủ Chủ tịch.</li>" +
            "<li><b>1958</b>: Chuyển sang ở và làm việc tại nhà sàn trong khu Phủ Chủ tịch.</li>" +
            "<li><b>5/1965</b>: Bắt đầu viết bản Di chúc.</li>" +
            "<li><b>2/9/1969</b>: Qua đời tại Hà Nội.</li></ul><p>{FIGURE}</p>"),

        ["Những năm tháng tuổi thơ trên quê hương Nghệ An"] = (
            "Làng Sen, làng Hoàng Trù – nơi nuôi dưỡng tâm hồn và lòng yêu nước của cậu bé Nguyễn Sinh Cung.",
            "<p>Quê hương Nam Đàn có truyền thống hiếu học và yêu nước. Những mái nhà tranh, lũy tre, ao sen và tiếng ru của mẹ là những ký ức đầu đời gắn bó suốt cuộc đời Người.</p>" +
            "<p>{FIGURE}</p><p>Ngày nay, Khu di tích Kim Liên gồm cụm di tích quê nội, quê ngoại và nhiều điểm di tích liên quan, mỗi năm đón hàng triệu lượt khách về thăm, đặc biệt dịp kỷ niệm ngày sinh 19/5.</p>"),

        ["Hành trình ba mươi năm bôn ba tìm đường cứu nước"] = (
            "Từ bến Nhà Rồng năm 1911 đến khi trở về Pác Bó năm 1941 – ba thập kỷ đi qua nhiều quốc gia.",
            "<p>Không theo con đường của các bậc tiền bối, người thanh niên quyết định sang phương Tây để tìm hiểu tận gốc rễ vấn đề. Với hai bàn tay trắng, Người làm phụ bếp, quét tuyết, rửa ảnh… để vừa sống vừa học.</p>" +
            "<div class=\"note-box\">Hành trình ấy giúp Người rút ra nhận thức: ở đâu nhân dân lao động cũng chung khát vọng tự do, và giải phóng dân tộc phải gắn với giải phóng con người.</div>" +
            "<p>{FIGURE}</p>"),
    };

    /// <summary>Mô tả di tích cho ô Bảo tàng 3D (tự viết).</summary>
    public static readonly Dictionary<string, string> MuseumQuery = new()
    {
        ["Khu di tích Phủ Chủ tịch"] = "Presidential Palace",
        ["Khu di tích Kim Liên"] = "stilt house",
        ["Khu di tích Bến Nhà Rồng"] = "Bến Nhà Rồng",
        ["Khu di tích Pác Bó"] = "Cốc Bó",
        ["Khu di tích ATK Định Hóa"] = "house",
        ["Khu di tích Tân Trào"] = "Đền thờ",
        ["Bảo tàng Hồ Chí Minh"] = "Museum",
        ["Khu di tích K9"] = "Stilt House",
        ["Nhà lưu niệm tại Huế"] = "Mausoleum",
    };
}
