(function () {
    var clock = document.getElementById('clock');
    var days = clock ? JSON.parse(clock.dataset.days) : [];
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

    var themeBtn = document.querySelector('.theme-btn');
    if (themeBtn) themeBtn.addEventListener('click', function () {
        var root = document.documentElement, b = themeBtn.dataset;
        var next = root.dataset.theme === b.dark ? b.light : b.dark;
        root.dataset.theme = next;
        try { localStorage.setItem(b.storageKey, next); } catch (e) { }
    });
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

// Trợ năng: phóng chữ tới 200% (yêu cầu gói thầu), ghi nhớ lựa chọn
(function () {
    var fab = document.querySelector('.a11y-fab');
    if (!fab) return;
    var d = document.documentElement, s = fab.dataset;
    var min = +s.min, max = +s.max, step = +s.step;
    var label = fab.querySelector('.a11y-zoom');
    function current() { return Math.round((parseFloat(getComputedStyle(d).getPropertyValue('--a11y-zoom')) || 1) * 100); }
    function render(z) {
        label.textContent = z + '%';
        fab.classList.toggle('zoomed', z !== min);
    }
    function setZoom(z) {
        z = Math.min(max, Math.max(min, z));
        d.style.setProperty('--a11y-zoom', z / 100);
        render(z);
        try { localStorage.setItem(s.zoomKey, z); } catch (e) { }
    }
    render(current());
    fab.addEventListener('click', function (e) {
        var b = e.target.closest('[data-a11y]');
        if (!b) return;
        var a = b.dataset.a11y;
        if (a === 'zoom-in') setZoom(current() + step);
        else if (a === 'zoom-out') setZoom(current() - step);
        else if (a === 'zoom-reset') setZoom(min);
    });
})();

// Xem tài liệu PDF dạng lật trang; chỉ xem trực tuyến (chặn chuột phải, kéo ảnh, chọn chữ, phím lưu/in)
(function () {
    var box = document.querySelector('.flipbook');
    if (!box || !window.pdfjsLib || !window.St) return;
    var stage = box.querySelector('.flipbook-stage');
    var pageLabel = box.querySelector('.flipbook-page');
    pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';

    ['contextmenu', 'dragstart', 'selectstart', 'copy'].forEach(function (ev) {
        box.addEventListener(ev, function (e) { e.preventDefault(); });
    });
    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && /^[spu]$/i.test(e.key)) e.preventDefault();
    });

    pdfjsLib.getDocument({ url: box.dataset.pdf }).promise.then(function (pdf) {
        var jobs = [];
        for (var i = 1; i <= pdf.numPages; i++) {
            jobs.push(pdf.getPage(i).then(function (page) {
                var vp = page.getViewport({ scale: 1.6 });
                var c = document.createElement('canvas');
                c.width = vp.width; c.height = vp.height;
                return page.render({ canvasContext: c.getContext('2d'), viewport: vp }).promise.then(function () {
                    return { url: c.toDataURL('image/jpeg', 0.85), w: vp.width, h: vp.height };
                });
            }));
        }
        return Promise.all(jobs);
    }).then(function (pages) {
        stage.innerHTML = '';
        var book = document.createElement('div');
        stage.appendChild(book);
        var flip = new St.PageFlip(book, {
            width: Math.round(pages[0].w / 2), height: Math.round(pages[0].h / 2),
            size: 'stretch', minWidth: 260, maxWidth: 520, minHeight: 360, maxHeight: 740,
            showCover: true, mobileScrollSupport: false, usePortrait: true
        });
        flip.loadFromImages(pages.map(function (p) { return p.url; }));
        function update() { pageLabel.textContent = (flip.getCurrentPageIndex() + 1) + ' / ' + flip.getPageCount(); }
        flip.on('flip', update);
        update();
        box.querySelector('[data-flip="prev"]').addEventListener('click', function () { flip.flipPrev(); });
        box.querySelector('[data-flip="next"]').addEventListener('click', function () { flip.flipNext(); });
    }).catch(function () {
        stage.innerHTML = '<p class="flipbook-status">' + box.dataset.error + '</p>';
    });
})();
