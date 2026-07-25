// Maison Aeternum — shared client behavior: theme toggle, toasts, scroll reveal.
(function () {
    "use strict";

    var THEME_KEY = "maison-aeternum-theme";

    function applyTheme(theme) {
        document.documentElement.setAttribute("data-theme", theme);
        var toggle = document.querySelector("[data-theme-toggle] i");
        if (toggle) {
            toggle.className = theme === "dark" ? "bi bi-moon-stars" : "bi bi-sun";
        }
    }

    function initTheme() {
        // Dark is the Maison's signature look — default to it regardless of OS
        // preference unless the visitor has explicitly chosen light mode before.
        var stored = localStorage.getItem(THEME_KEY);
        applyTheme(stored || "dark");

        var toggleBtn = document.querySelector("[data-theme-toggle]");
        if (toggleBtn) {
            toggleBtn.addEventListener("click", function () {
                var current = document.documentElement.getAttribute("data-theme") || "dark";
                var next = current === "dark" ? "light" : "dark";
                localStorage.setItem(THEME_KEY, next);
                applyTheme(next);
            });
        }
    }

    function initScrollReveal() {
        var targets = document.querySelectorAll(".reveal");
        if (!targets.length) return;

        if (!("IntersectionObserver" in window)) {
            targets.forEach(function (el) { el.classList.add("is-visible"); });
            return;
        }

        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add("is-visible");
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.01, rootMargin: "0px 0px -10% 0px" });

        targets.forEach(function (el) { observer.observe(el); });
    }

    function showToast(message, type) {
        var host = document.querySelector(".toast-host");
        if (!host) return;

        var icon = type === "success" ? "bi-check-circle-fill"
            : type === "error" ? "bi-exclamation-circle-fill"
            : "bi-info-circle-fill";

        var toast = document.createElement("div");
        toast.className = "toast-maison " + (type || "info");
        toast.innerHTML = '<i class="bi ' + icon + '"></i><span>' + message + "</span>";
        host.appendChild(toast);

        setTimeout(function () {
            toast.style.opacity = "0";
            toast.style.transform = "translateX(30px)";
            setTimeout(function () { toast.remove(); }, 250);
        }, 4200);
    }

    function initCountUp() {
        var targets = document.querySelectorAll("[data-countup]");
        if (!targets.length) return;

        function animate(el) {
            var target = parseFloat(el.getAttribute("data-countup")) || 0;
            var duration = 1400;
            var start = performance.now();

            function step(now) {
                var progress = Math.min((now - start) / duration, 1);
                var eased = 1 - Math.pow(1 - progress, 3);
                el.textContent = Math.round(target * eased).toLocaleString();
                if (progress < 1) requestAnimationFrame(step);
            }
            requestAnimationFrame(step);
        }

        if (!("IntersectionObserver" in window)) {
            targets.forEach(animate);
            return;
        }

        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    animate(entry.target);
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.4 });

        targets.forEach(function (el) { observer.observe(el); });
    }

    function initServerToasts() {
        var data = document.getElementById("server-toast-data");
        if (!data) return;

        var message = data.getAttribute("data-message");
        var type = data.getAttribute("data-type");
        if (message) showToast(message, type);
    }

    window.MaisonAeternum = window.MaisonAeternum || {};
    window.MaisonAeternum.showToast = showToast;

    document.addEventListener("DOMContentLoaded", function () {
        initTheme();
        initScrollReveal();
        initCountUp();
        initServerToasts();
    });
})();
