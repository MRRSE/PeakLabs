document.addEventListener("click", (event) => {
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
