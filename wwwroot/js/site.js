
(() => {
  const articleClickKey = "peakLabs:article-click";

  document.addEventListener("click", (event) => {
    if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;

    const link = event.target.closest("a[href]");
    if (!link || link.target === "_blank" || link.hasAttribute("download")) return;

    const url = new URL(link.href, window.location.href);
    if (url.origin !== window.location.origin) return;

    const match = url.pathname.match(/^\/Article\/SingleArticle\/(\d+)\/?$/i);
    if (!match) return;

    try {
      sessionStorage.setItem(articleClickKey, JSON.stringify({ id: match[1], at: Date.now() }));
    } catch {
      // View recording is skipped when session storage is unavailable.
    }
  }, true);
})();
