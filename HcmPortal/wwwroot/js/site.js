(function () {
    // Đồng hồ ngày giờ tiếng Việt
    var clock = document.getElementById('clock');
    var days = ['Chủ nhật', 'Thứ hai', 'Thứ ba', 'Thứ tư', 'Thứ năm', 'Thứ sáu', 'Thứ bảy'];
    function pad(n) { return n < 10 ? '0' + n : n; }
    function tick() {
        if (!clock) return;
        var d = new Date();
        clock.textContent = days[d.getDay()] + ', ' + pad(d.getDate()) + '/' + pad(d.getMonth() + 1) + '/' + d.getFullYear()
            + ' - ' + pad(d.getHours()) + ':' + pad(d.getMinutes()) + ':' + pad(d.getSeconds());
    }
    tick(); setInterval(tick, 1000);

    // Tabs Multimedia
    document.querySelectorAll('.tab[data-tab]').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var wrap = btn.closest('section');
            wrap.querySelectorAll('.tab').forEach(function (b) { b.classList.remove('active'); });
            wrap.querySelectorAll('.tab-pane').forEach(function (p) { p.classList.remove('active'); });
            btn.classList.add('active');
            document.getElementById(btn.dataset.tab).classList.add('active');
        });
    });

    // Carousel Bảo tàng 3D: nút trái/phải + tự chạy
    document.querySelectorAll('[data-carousel]').forEach(function (el) {
        var track = el.querySelector('.car-track');
        function step(dir) {
            var w = track.firstElementChild ? track.firstElementChild.offsetWidth + 16 : 300;
            var atEnd = track.scrollLeft + track.clientWidth >= track.scrollWidth - 5;
            if (dir > 0 && atEnd) track.scrollLeft = 0; else track.scrollLeft += dir * w;
        }
        el.querySelector('.prev').addEventListener('click', function () { step(-1); });
        el.querySelector('.next').addEventListener('click', function () { step(1); });
        var timer = setInterval(function () { step(1); }, 4000);
        el.addEventListener('mouseenter', function () { clearInterval(timer); });
        el.addEventListener('mouseleave', function () { timer = setInterval(function () { step(1); }, 4000); });
    });

    // Nút lên đầu trang
    var top = document.querySelector('.to-top');
    window.addEventListener('scroll', function () { top.classList.toggle('show', window.scrollY > 400); });
    top.addEventListener('click', function (e) { e.preventDefault(); window.scrollTo({ top: 0, behavior: 'smooth' }); });

    // Lightbox album ảnh
    if (window.GLightbox) GLightbox({ selector: '.glightbox' });
})();

// Đổi cỡ chữ bài viết (A- A A+)
(function () {
    var body = document.getElementById('detail-body');
    if (!body) return;
    var base = 15, size = base;
    document.querySelectorAll('[data-font]').forEach(function (b) {
        b.addEventListener('click', function () {
            var v = +b.dataset.font;
            size = v === 0 ? base : Math.min(22, Math.max(12, size + v));
            body.style.fontSize = size + 'px';
        });
    });
})();
