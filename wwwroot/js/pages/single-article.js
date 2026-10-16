(() => {
  const articleId = window.peakLabsArticleId;
  const token = document.querySelector("#articleViewToken input[name='__RequestVerificationToken']")?.value;
  const countElement = document.getElementById("articleViewCount");

  async function recordArticleView() {
    if (!articleId || !token || !countElement) return;

    let clickIntent;
    try {
      clickIntent = JSON.parse(sessionStorage.getItem("peakLabs:article-click") || "null");
      sessionStorage.removeItem("peakLabs:article-click");
    } catch {
      return;
    }
    if (!clickIntent || String(clickIntent.id) !== String(articleId) || Date.now() - clickIntent.at > 15000) return;

    const body = new FormData();
    body.append("__RequestVerificationToken", token);
    try {
      const response = await fetch(`/Article/RecordView?id=${encodeURIComponent(articleId)}`, {
        method: "POST",
        body,
        credentials: "same-origin",
        headers: { Accept: "application/json" },
      });
      if (!response.ok) throw new Error("View could not be recorded");
      const result = await response.json();
      countElement.textContent = Number(result.viewCount).toLocaleString("fa-IR");
    } catch (error) {
      console.warn("Article view count could not be updated.", error);
    }
  }

  async function copyArticleLink() {
    if (navigator.clipboard?.writeText) {
      await navigator.clipboard.writeText(window.location.href);
      return;
    }

    const input = document.createElement("textarea");
    input.value = window.location.href;
    input.setAttribute("readonly", "");
    input.style.position = "fixed";
    input.style.opacity = "0";
    document.body.append(input);
    input.select();
    const copied = document.execCommand("copy");
    input.remove();
    if (!copied) throw new Error("Link could not be copied");
  }

  document.querySelectorAll("[data-share]").forEach((button) => {
    button.addEventListener("click", async () => {
      const url = encodeURIComponent(window.location.href);
      const title = encodeURIComponent(document.title);

      if (button.dataset.share === "x") {
        window.open(`https://twitter.com/intent/tweet?url=${url}&text=${title}`, "_blank", "noopener,noreferrer");
      } else if (button.dataset.share === "linkedin") {
        window.open(`https://www.linkedin.com/sharing/share-offsite/?url=${url}`, "_blank", "noopener,noreferrer");
      } else {
        try {
          await copyArticleLink();
          if (typeof toastMsg === "function") toastMsg("پیوند مقاله کپی شد.");
        } catch {
          if (typeof toastMsg === "function") toastMsg("کپی پیوند ممکن نشد.");
        }
      }
    });
  });

  recordArticleView();
})();
