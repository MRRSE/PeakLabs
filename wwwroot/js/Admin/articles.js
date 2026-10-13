document.addEventListener("change", (event) => {
  if (event.target.id !== "articleCoverImage") return;
  const name = document.getElementById("articleCoverImageName");
  if (name) name.textContent = event.target.files?.[0]?.name || "";
});

document.addEventListener("submit", async (event) => {
  const form = event.target;
  if (!(form instanceof HTMLFormElement) || form.id !== "articleForm") return;

  event.preventDefault();
  const errorBox = document.getElementById("articleFormErrors");
  const saveButton = document.querySelector("#modal .dialog-foot .btn-primary");
  const token = document.querySelector(
    "#articleAntiforgeryToken input[name='__RequestVerificationToken']",
  )?.value;

  errorBox.hidden = true;
  errorBox.textContent = "";
  if (!token) {
    errorBox.textContent = "درخواست معتبر نیست. صفحه را تازه‌سازی کنید و دوباره تلاش کنید.";
    errorBox.hidden = false;
    return;
  }

  if (saveButton) saveButton.disabled = true;
  const formData = new FormData(form);
  formData.append("__RequestVerificationToken", token);

  try {
    const response = await fetch(form.action, {
      method: "POST",
      body: formData,
      headers: { Accept: "application/json" },
    });
    const result = await response.json();
    if (!response.ok) throw new Error(result.message || "ذخیره مقاله انجام نشد.");

    closeModal();
    toastMsg(result.message || "مقاله ذخیره شد.");
    window.setTimeout(() => window.location.reload(), 500);
  } catch (error) {
    errorBox.textContent = error.message || "ارتباط با سرور برقرار نشد. دوباره تلاش کنید.";
    errorBox.hidden = false;
    if (saveButton) saveButton.disabled = false;
  }
});

document.addEventListener("click", (event) => {
  const editButton = event.target.closest(".article-edit-button");
  if (editButton) {
    const article = (window.articles || []).find((item) => item.id === Number(editButton.dataset.articleId));
    if (article) openModal("articleEdit", article);
    return;
  }

  const deleteButton = event.target.closest(".article-delete-button");
  if (deleteButton) {
    const article = (window.articles || []).find((item) => item.id === Number(deleteButton.dataset.articleId));
    if (article) openArticleDeleteModal(article.id, article.title);
  }
});

document.addEventListener("input", (event) => {
  if (event.target.id === "articleSearch") filterArticles();
});
document.addEventListener("change", (event) => {
  if (event.target.id === "articleCategoryFilter" || event.target.id === "articleStatusFilter") filterArticles();
});

function filterArticles() {
  const search = (document.getElementById("articleSearch")?.value || "").trim().toLocaleLowerCase();
  const category = document.getElementById("articleCategoryFilter")?.value || "";
  const status = document.getElementById("articleStatusFilter")?.value || "";
  const rows = [...document.querySelectorAll(".article-row")];
  let visibleCount = 0;

  rows.forEach((row) => {
    const matches = (!search || row.dataset.search.toLocaleLowerCase().includes(search))
      && (!category || row.dataset.categoryId === category)
      && (!status || row.dataset.status === status);
    row.hidden = !matches;
    if (matches) visibleCount++;
  });
  const noResults = document.getElementById("articlesNoResults");
  if (noResults) noResults.hidden = rows.length === 0 || visibleCount > 0;
}

async function deletePendingArticle() {
  const id = pendingDeleteArticleId;
  const token = document.querySelector("#articleAntiforgeryToken input[name='__RequestVerificationToken']")?.value;
  const saveButton = document.querySelector("#modal .dialog-foot .btn-primary");
  if (!id || !token) return;
  if (saveButton) saveButton.disabled = true;

  const formData = new FormData();
  formData.append("id", id);
  formData.append("__RequestVerificationToken", token);
  try {
    const response = await fetch("/Admin/Articles/Delete", { method: "POST", body: formData, headers: { Accept: "application/json" } });
    const result = await response.json();
    if (!response.ok) throw new Error(result.message || "حذف مقاله انجام نشد.");
    closeModal();
    toastMsg(result.message || "مقاله حذف شد.");
    window.setTimeout(() => window.location.reload(), 500);
  } catch (error) {
    toastMsg(error.message || "ارتباط با سرور برقرار نشد. دوباره تلاش کنید.");
    if (saveButton) saveButton.disabled = false;
  }
}
