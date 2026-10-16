document.addEventListener("click", (event) => {
  const portfolioCard = event.target.closest(".js-portfolio-card");
  if (portfolioCard) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const portfolioId = portfolioCard.dataset.portfolioId;
    const projectUrl = portfolioCard.dataset.projectUrl;
    const clickedLink = event.target.closest("a.js-portfolio-view");

    if (token && portfolioId) {
      const body = new URLSearchParams({ __RequestVerificationToken: token });
      fetch(`/Portfolio/RecordView?id=${encodeURIComponent(portfolioId)}`, {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded;charset=UTF-8" },
        body,
        keepalive: true,
      })
        .then((response) => (response.ok ? response.json() : null))
        .then((result) => {
          if (!result || typeof result.viewCount !== "number") return;
          const count = portfolioCard.querySelector("[data-portfolio-view-number]");
          const countLabel = portfolioCard.querySelector(".article-card-views");
          if (count) count.textContent = new Intl.NumberFormat("fa-IR").format(result.viewCount);
          if (countLabel) {
            countLabel.setAttribute(
              "aria-label",
              `${new Intl.NumberFormat("fa-IR").format(result.viewCount)} بازدید`,
            );
          }
        })
        .catch(() => {});
    }

    if (!clickedLink && projectUrl) window.open(projectUrl, "_blank", "noopener,noreferrer");
    return;
  }

  const link = event.target.closest("a.js-article-view");
  if (!link) return;

  const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
  if (!token) return;

  const body = new URLSearchParams({ __RequestVerificationToken: token });
  const request = {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded;charset=UTF-8" },
    body,
  };
  const recordUrl = `/Article/RecordView?id=${encodeURIComponent(link.dataset.articleId)}`;

  if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey || link.target === "_blank") {
    fetch(recordUrl, { ...request, keepalive: true });
    return;
  }

  event.preventDefault();
  const destination = link.href;
  fetch(recordUrl, request)
    .catch(() => {})
    .finally(() => window.location.assign(destination));
});

document.addEventListener("keydown", (event) => {
  if ((event.key !== "Enter" && event.key !== " ") || !event.target.matches(".js-portfolio-card")) return;
  event.preventDefault();
  event.target.click();
});
