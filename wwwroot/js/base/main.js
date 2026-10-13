// Reveal in animation on Y , by scroll
const revealElements = document.querySelectorAll(".reveal");

const observer = new IntersectionObserver((entries) => {
  entries.forEach((entry) => {
    if (entry.isIntersecting) {
      entry.target.classList.add("in");
    }
  });
});

revealElements.forEach((element) => {
  observer.observe(element);
});

// Reveal in animation on X , by scroll
const revealElementsX = document.querySelectorAll(".reveal-X");

const observerX = new IntersectionObserver((entries) => {
  entries.forEach((entry) => {
    if (entry.isIntersecting) {
      entry.target.classList.add("in");
    }
  });
});

revealElementsX.forEach((element) => {
  observer.observe(element);
});

// Reveal in animation on X Revers , by scroll
const revealElementsXRev = document.querySelectorAll(".reveal-X-Rev");

const observerXRev = new IntersectionObserver((entries) => {
  entries.forEach((entry) => {
    if (entry.isIntersecting) {
      entry.target.classList.add("in");
    }
  });
});

revealElementsXRev.forEach((element) => {
  observer.observe(element);
});

// Counter animation
(() => {
  const peaklabsCounters = document.querySelectorAll(".counter");

  const peaklabsCounterObserver = new IntersectionObserver(
    (entries, observer) => {
      entries.forEach((entry) => {
        if (!entry.isIntersecting) return;

        const peaklabsCounter = entry.target;
        const peaklabsTarget = Number(peaklabsCounter.dataset.target);

        let peaklabsCurrent = 0;

        const peaklabsDuration = 1000;
        const peaklabsStartTime = performance.now();

        const peaklabsAnimateCounter = (currentTime) => {
          const peaklabsProgress = Math.min(
            (currentTime - peaklabsStartTime) / peaklabsDuration,
            1,
          );

          peaklabsCurrent = Math.floor(peaklabsProgress * peaklabsTarget);

          peaklabsCounter.textContent = peaklabsCurrent;

          if (peaklabsProgress < 1) {
            requestAnimationFrame(peaklabsAnimateCounter);
          }
        };

        requestAnimationFrame(peaklabsAnimateCounter);

        observer.unobserve(peaklabsCounter);
      });
    },
  );

  peaklabsCounters.forEach((counter) => {
    peaklabsCounterObserver.observe(counter);
  });
})();

//Open modals

let articleSectionCount = 1;

function articleSectionTemplate(number, section = {}) {
  const headingType = section.headingType === undefined ? "H2" : section.headingType;
  return `<div class="article-section" data-section="${number}">
          <div class="article-section-head">
            <span class="article-section-number">قسمت ${number}</span>
            ${number > 1 ? `<button type="button" class="icon-btn section-remove" onclick="removeArticleSection(this)" title="حذف بخش"><i class="bi bi-trash3"></i></button>` : ""}
          </div>
          <div class="form-grid">
            <div class="field">
              <label>نوع عنوان</label>
              <select name="Sections[${number - 1}].HeadingType">
                <option value="H2" ${headingType === "H2" ? "selected" : ""}>H2</option>
                <option value="H3" ${headingType === "H3" ? "selected" : ""}>H3</option>
                <option value="H4" ${headingType === "H4" ? "selected" : ""}>H4</option>
                <option value="" ${!headingType ? "selected" : ""}>بدون عنوان</option>
              </select>
            </div>
            <div class="field">
              <label>عنوان بخش</label>
              <input type="text" name="Sections[${number - 1}].Title" maxlength="100" placeholder="عنوان این بخش..." value="${escapeHtml(section.title || "")}">
            </div>
            <div class="field full">
              <label>محتوای بخش</label>
              <textarea name="Sections[${number - 1}].Content" maxlength="50000" required style="min-height:150px" placeholder="محتوای این بخش را بنویسید...">${escapeHtml(section.content || "")}</textarea>
            </div>
          </div>
        </div>`;
}



function addArticleSection() {
  articleSectionCount++;
  const container = document.getElementById("articleSections");
  if (container)
    container.insertAdjacentHTML(
      "beforeend",
      articleSectionTemplate(articleSectionCount),
    );
}
function removeArticleSection(button) {
  const section = button.closest(".article-section");
  if (section) section.remove();
  document
    .querySelectorAll("#articleSections .article-section")
    .forEach(
      (s, i) =>
        (s.querySelector(".article-section-number").textContent =
          `${t("section")} ${i + 1}`),
    );
}

let modalType = null;
let pendingDeleteAccountId = null;
let pendingDeletePortfolioId = null;
let pendingDeleteArticleId = null;
let pendingAccountStatus = null;

function portfolioModalBody(item = {}, isEdit = false) {
  const formAction = isEdit ? "/Admin/Portfolio/Update" : "/Admin/Portfolio/Create";
  const projectImage = isEdit
    ? `<div class="field full"><label>تصویر فعلی</label><img id="portfolioImagePreview" class="portfolio-image-preview" src="${escapeHtml(item.image)}" alt="تصویر ${escapeHtml(item.name)}"><small id="portfolioImageName" class="muted">برای تغییر تصویر، فایل جدید انتخاب کن.</small></div>`
    : `<small id="portfolioImageName" class="muted"></small>`;
  return `<form id="portfolioForm" action="${formAction}" method="post" enctype="multipart/form-data">
      ${isEdit ? `<input type="hidden" name="Id" value="${escapeHtml(item.id)}">` : ""}
      <div id="portfolioFormErrors" class="portfolio-form-errors" role="alert" hidden></div>
      <div class="form-grid">
        <div class="field full"><label for="portfolioImage">${isEdit ? "تصویر جدید (اختیاری)" : "تصویر پروژه"}</label><div class="upload" onclick="document.getElementById('portfolioImage').click()"><i class="bi bi-image"></i><strong style="display:block;margin-top:7px">برای آپلود تصویر کلیک کنید</strong><small>PNG، JPG یا WebP · حداکثر ۵ مگابایت</small><input id="portfolioImage" name="ProjectImage" type="file" accept="image/png,image/jpeg,image/webp" ${isEdit ? "" : "required"} hidden></div>${projectImage}</div>
        <div class="field"><label for="portfolioName">نام پروژه</label><input id="portfolioName" name="ProjectName" maxlength="120" required placeholder="نام پروژه" value="${escapeHtml(item.name)}"></div>
        <div class="field"><label for="portfolioLink">لینک پروژه</label><input id="portfolioLink" name="ProjectLink" type="url" maxlength="500" required placeholder="https://example.com" value="${escapeHtml(item.link)}"></div>
        <div class="field full"><label for="portfolioDescription">توضیحات پروژه</label><textarea id="portfolioDescription" name="ProjectDescription" maxlength="1000" required placeholder="توضیح کوتاه درباره پروژه">${escapeHtml(item.description)}</textarea></div>
        ${isEdit ? "" : `<div class="field full"><small class="muted"><i class="bi bi-clock"></i> زمان ثبت پروژه هنگام ذخیره به‌صورت خودکار ثبت می‌شود.</small></div>`}
      </div></form>`;
}

function articleModalBody(article = {}, isEdit = false) {
  articleSectionCount = Math.max(article.sections?.length || 0, 1);
  const categoryOptions = (window.articleCategories || [])
    .map((category) => `<option value="${category.id}" ${Number(article.categoryId) === Number(category.id) ? "selected" : ""}>${escapeHtml(category.name)}</option>`)
    .join("");
  const sections = article.sections?.length ? article.sections : [{}];
  const sectionMarkup = sections.map((section, index) => articleSectionTemplate(index + 1, section)).join("");
  const cover = isEdit
    ? `<div class="field full"><label>تصویر فعلی</label><img class="portfolio-image-preview" src="${escapeHtml(article.coverImage)}" alt="تصویر ${escapeHtml(article.title)}"></div>`
    : "";

  return `<form id="articleForm" action="/Admin/Articles/${isEdit ? `Update?id=${encodeURIComponent(article.id)}` : "Create"}" method="post" enctype="multipart/form-data">
      ${isEdit ? `<input type="hidden" name="Id" value="${escapeHtml(article.id)}">` : ""}
      <div id="articleFormErrors" class="portfolio-form-errors" role="alert" hidden></div>
      <div class="form-grid">
        <div class="field full"><label for="articleCoverImage">${isEdit ? "تصویر کاور جدید (اختیاری)" : "تصویر کاور"}</label><div class="upload" onclick="if(event.target.id !== 'articleCoverImage') document.getElementById('articleCoverImage').click()"><i class="bi bi-cloud-arrow-up"></i><strong style="display:block;margin-top:7px">برای آپلود کلیک کنید</strong><small>PNG، JPG یا WebP · حداکثر ۵ مگابایت</small><input id="articleCoverImage" name="CoverImage" type="file" accept="image/png,image/jpeg,image/webp" ${isEdit ? "" : "required"} hidden></div><small id="articleCoverImageName" class="muted"></small>${cover}</div>
        <div class="field full"><label for="articleTitle">عنوان مقاله</label><input id="articleTitle" name="Title" maxlength="60" required value="${escapeHtml(article.title || "")}"></div>
        <div class="field full"><label for="articleShortDescription">توضیح کوتاه</label><textarea id="articleShortDescription" name="ShortDescription" maxlength="160" required>${escapeHtml(article.shortDescription || "")}</textarea></div>
        <div class="field"><label for="articleCategoryId">دسته‌بندی</label><select id="articleCategoryId" name="CategoryId" required>${categoryOptions}</select></div>
        <div class="field"><label for="articleStatus">وضعیت</label><select id="articleStatus" name="Status" required><option value="published" ${article.status === "published" || !article.status ? "selected" : ""}>منتشرشده</option><option value="draft" ${article.status === "draft" ? "selected" : ""}>پیش‌نویس</option></select></div>
        ${isEdit ? `<div class="field"><label>نویسنده</label><input type="text" value="${escapeHtml(article.authorName || "نامشخص")}" readonly></div>` : `<div class="field full"><small class="muted">نویسنده به‌صورت خودکار از حساب واردشده ثبت می‌شود.</small></div>`}
        <div class="field full"><label>بخش‌بندی مقاله</label><div id="articleSections" class="article-sections">${sectionMarkup}</div><button type="button" class="btn btn-outline section-add" onclick="addArticleSection()"><i class="bi bi-plus-lg"></i> افزودن قسمت جدید</button></div>
        ${isEdit ? "" : `<div class="field full"><small class="muted"><i class="bi bi-clock"></i> تاریخ انتشار هنگام ذخیره خودکار ثبت می‌شود.</small></div>`}
      </div></form>`;
}

function openModal(type, account = {}) {
  modalType = type;
  document.getElementById("modal").classList.add("open");

  const title = {
    article: "ایجاد مقاله",
    articleEdit: "ویرایش مقاله",
    portfolio: "نمونه کار جدید",
    portfolioEdit: "ویرایش نمونه‌کار",
    developer: "توسعه دهنده جدید",
    account: "ایجاد حساب کاربری",
    accountEdit: "ویرایش حساب کاربری",
  }[type];

  document.getElementById("modalTitle").textContent = title;
  let body = "";

  if (type === "article") body = articleModalBody();
  if (type === "articleEdit") body = articleModalBody(account, true);

  if (type === "portfolio") body = portfolioModalBody();
  if (type === "portfolioEdit") body = portfolioModalBody(account, true);

  if (type === "developer" || type === "admin")
    body = `
    <div class="form-grid">

        <div class="field">
            <label>نام</label>
            <input
                maxlength="60"
                placeholder="نام و نام خانوادگی"
            >
        </div>

        <div class="field">
            <label>ایمیل</label>
            <input
                type="email"
                maxlength="120"
                placeholder="name@peaklabs.dev"
            >
        </div>

        <div class="field">
            <label>نقش</label>
            <select>
                ${
                  type === "admin"
                    ? `
                            <option>مدیر محتوا</option>
                            <option>مدیر ارشد</option>
                        `
                    : `
                            <option>ویرایشگر</option>
                            <option>سرپرست تیم</option>
                        `
                }
            </select>
        </div>

        <div class="field">
            <label>دسترسی</label>
            <select>
                <option>آنلاین</option>
                <option>آفلاین</option>
            </select>
        </div>

        <div class="field full">
            <label>دسترسی‌ها</label>
            <input
                maxlength="200"
                placeholder="مقالات، نمونه‌کارها، آمار"
            >
        </div>

    </div>
`;
  if (type === "account") {
    body = accountModalBody();
  }
  if (type === "accountEdit") {
    body = accountModalBody({ ...account, isEdit: true });
  }
  if (type === "teamMember") {
    body = teamMemberModalBody();
  }

  document.getElementById("modalBody").innerHTML = body;
  if (type === "accountEdit") {
    const passwordInput = document.getElementById("accountPassword");
    const confirmInput = document.getElementById("accountPasswordConfirm");
    const saveButton = document.querySelector("#modal .dialog-foot .btn-primary");
    if (saveButton) {
      saveButton.dataset.defaultText ||= saveButton.textContent.trim();
      saveButton.textContent = "ذخیره تغییرات";
    }
    passwordInput?.addEventListener("input", () => {
      if (confirmInput) confirmInput.required = passwordInput.value.length > 0;
    });
  }
}

function closeModal() {
  document.getElementById("modal").classList.remove("open");
  document.querySelector("#modal .dialog")?.classList.remove("account-delete-dialog");
  const saveButton = document.querySelector("#modal .dialog-foot .btn-primary");
  if (saveButton) {
    saveButton.textContent = saveButton.dataset.defaultText || "ذخیره";
    saveButton.disabled = false;
  }
  modalType = null;
  pendingDeleteAccountId = null;
  pendingDeletePortfolioId = null;
  pendingDeleteArticleId = null;
  pendingAccountStatus = null;
}
function saveModal() {
  if (modalType === "article" || modalType === "articleEdit") {
    document.getElementById("articleForm")?.requestSubmit();
    return;
  }
  if (modalType === "deleteArticle") {
    deletePendingArticle();
    return;
  }
  if (modalType === "portfolio" || modalType === "portfolioEdit") {
    document.getElementById("portfolioForm")?.requestSubmit();
    return;
  }
  if (modalType === "account" || modalType === "accountEdit") {
    document.getElementById("accountForm")?.requestSubmit();
    return;
  }
  if (modalType === "deleteAccount") {
    deletePendingAccount();
    return;
  }
  if (modalType === "deletePortfolio") {
    deletePendingPortfolio();
    return;
  }
  if (modalType === "setAccountStatus") {
    setPendingAccountStatus();
    return;
  }

  closeModal();
  toastMsg("ذخیره شد");
}

function openArticleDeleteModal(id, title) {
  pendingDeleteArticleId = id;
  modalType = "deleteArticle";
  document.getElementById("modalTitle").textContent = "حذف مقاله";
  document.getElementById("modalBody").textContent = `مقاله «${title}» و بخش‌های آن حذف شوند؟`;
  document.querySelector("#modal .dialog-foot .btn-primary").textContent = "حذف مقاله";
  document.getElementById("modal").classList.add("open");
}

function openPortfolioDeleteModal(button) {
  pendingDeletePortfolioId = button.dataset.portfolioId;
  if (!pendingDeletePortfolioId) return;

  modalType = "deletePortfolio";
  const modal = document.getElementById("modal");
  const dialog = modal.querySelector(".dialog");
  const body = document.getElementById("modalBody");
  const confirmButton = modal.querySelector(".dialog-foot .btn-primary");
  dialog.classList.add("account-delete-dialog");
  document.getElementById("modalTitle").textContent = "حذف نمونه‌کار";
  body.replaceChildren();

  const content = document.createElement("div");
  content.className = "account-delete-confirmation";
  const icon = document.createElement("i");
  icon.className = "bi bi-trash3 account-delete-confirmation-icon";
  icon.setAttribute("aria-hidden", "true");
  const text = document.createElement("div");
  const name = document.createElement("p");
  name.className = "account-delete-confirmation-name";
  name.textContent = `نمونه‌کار «${button.dataset.portfolioName || "انتخاب‌شده"}» حذف شود؟`;
  const note = document.createElement("small");
  note.textContent = "تصویر ذخیره‌شدهٔ این نمونه‌کار هم حذف می‌شود.";
  text.append(name, note);
  content.append(icon, text);
  body.append(content);

  if (confirmButton) {
    confirmButton.dataset.defaultText ||= confirmButton.textContent.trim();
    confirmButton.textContent = "حذف نمونه‌کار";
    confirmButton.disabled = false;
  }
  modal.classList.add("open");
}

async function deletePendingPortfolio() {
  if (!pendingDeletePortfolioId) return;

  const token = document.querySelector(
    "#portfolioAntiforgeryToken input[name='__RequestVerificationToken']",
  )?.value;
  const confirmButton = document.querySelector("#modal .dialog-foot .btn-primary");
  if (!token) {
    toastMsg("درخواست معتبر نیست. صفحه را تازه‌سازی کنید و دوباره تلاش کنید.");
    return;
  }

  if (confirmButton) confirmButton.disabled = true;
  const formData = new FormData();
  formData.append("__RequestVerificationToken", token);
  formData.append("id", pendingDeletePortfolioId);

  try {
    const response = await fetch("/Admin/Portfolio/Delete", {
      method: "POST",
      body: formData,
      headers: { Accept: "application/json" },
    });
    const result = await response.json();
    if (!response.ok) throw new Error(result.message || "حذف نمونه‌کار انجام نشد.");

    closeModal();
    toastMsg(result.message || "نمونه‌کار حذف شد.");
    window.setTimeout(() => window.location.reload(), 500);
  } catch (error) {
    toastMsg(error.message || "ارتباط با سرور برقرار نشد. دوباره تلاش کنید.");
    if (confirmButton) confirmButton.disabled = false;
  }
}

function escapeHtml(value) {
  return String(value ?? "").replace(/[&<>"']/g, (character) => ({
    "&": "&amp;",
    "<": "&lt;",
    ">": "&gt;",
    '"': "&quot;",
    "'": "&#39;",
  })[character]);
}

function openDeleteAccountModal(button) {
  pendingDeleteAccountId = button.dataset.accountId;
  modalType = "deleteAccount";

  const modal = document.getElementById("modal");
  const dialog = modal.querySelector(".dialog");
  const title = document.getElementById("modalTitle");
  const body = document.getElementById("modalBody");
  const confirmButton = modal.querySelector(".dialog-foot .btn-primary");

  dialog.classList.add("account-delete-dialog");
  title.textContent = "حذف حساب کاربری";
  body.replaceChildren();

  const content = document.createElement("div");
  content.className = "account-delete-confirmation";
  const icon = document.createElement("i");
  icon.className = "bi bi-person-x account-delete-confirmation-icon";
  icon.setAttribute("aria-hidden", "true");
  const text = document.createElement("div");
  const name = document.createElement("p");
  name.className = "account-delete-confirmation-name";
  name.textContent = `حساب «${button.dataset.accountName || "کاربر انتخاب‌شده"}» حذف شود؟`;
  const note = document.createElement("small");
  note.textContent = "این کار قابل بازگشت نیست.";
  text.append(name, note);
  content.append(icon, text);
  body.append(content);

  if (confirmButton) {
    confirmButton.dataset.defaultText ||= confirmButton.textContent.trim();
    confirmButton.textContent = "حذف حساب";
    confirmButton.disabled = false;
  }

  modal.classList.add("open");
}

async function deletePendingAccount() {
  if (!pendingDeleteAccountId) return;

  const token = document.querySelector(
    "#accountAntiforgeryToken input[name='__RequestVerificationToken']",
  )?.value;
  const deleteUrl = document.getElementById("accountAntiforgeryToken")?.dataset.deleteUrl;
  const confirmButton = document.querySelector("#modal .dialog-foot .btn-primary");
  if (!token || !deleteUrl) {
    toastMsg("امکان حذف حساب در حال حاضر وجود ندارد.");
    return;
  }

  if (confirmButton) confirmButton.disabled = true;
  const formData = new FormData();
  formData.append("__RequestVerificationToken", token);
  formData.append("id", pendingDeleteAccountId);

  try {
    const response = await fetch(deleteUrl, {
      method: "POST",
      body: formData,
      headers: { Accept: "application/json" },
    });
    const result = await response.json();
    if (!response.ok) throw new Error(result.message || "حذف حساب انجام نشد.");

    closeModal();
    toastMsg(result.message || "حساب کاربری حذف شد.");
    window.setTimeout(() => window.location.reload(), 700);
  } catch (error) {
    toastMsg(error.message || "ارتباط با سرور برقرار نشد. دوباره تلاش کنید.");
    if (confirmButton) confirmButton.disabled = false;
  }
}

function openAccountStatusModal(button) {
  const willActivate = button.dataset.targetActive === "true";
  pendingAccountStatus = {
    accountId: button.dataset.accountId,
    accountName: button.dataset.accountName || "کاربر انتخاب‌شده",
    isActive: willActivate,
  };
  modalType = "setAccountStatus";

  const modal = document.getElementById("modal");
  const dialog = modal.querySelector(".dialog");
  const body = document.getElementById("modalBody");
  const confirmButton = modal.querySelector(".dialog-foot .btn-primary");
  dialog.classList.add("account-delete-dialog");
  document.getElementById("modalTitle").textContent = willActivate ? "فعال‌کردن حساب" : "غیرفعال‌کردن حساب";
  body.replaceChildren();

  const content = document.createElement("div");
  content.className = "account-delete-confirmation";
  const icon = document.createElement("i");
  icon.className = `bi ${willActivate ? "bi-person-check" : "bi-person-x"} account-delete-confirmation-icon`;
  icon.setAttribute("aria-hidden", "true");
  const text = document.createElement("div");
  const name = document.createElement("p");
  name.className = "account-delete-confirmation-name";
  name.textContent = `حساب «${pendingAccountStatus.accountName}» ${willActivate ? "فعال" : "غیرفعال"} شود؟`;
  const note = document.createElement("small");
  note.textContent = willActivate
    ? "کاربر می‌تواند دوباره با این حساب وارد شود."
    : "ورودهای بعدی با این حساب مسدود می‌شود.";
  text.append(name, note);
  content.append(icon, text);
  body.append(content);

  if (confirmButton) {
    confirmButton.dataset.defaultText ||= confirmButton.textContent.trim();
    confirmButton.textContent = willActivate ? "فعال‌کردن" : "غیرفعال‌کردن";
    confirmButton.disabled = false;
  }

  modal.classList.add("open");
}

async function setPendingAccountStatus() {
  if (!pendingAccountStatus) return;

  const token = document.querySelector(
    "#accountAntiforgeryToken input[name='__RequestVerificationToken']",
  )?.value;
  const statusUrl = document.getElementById("accountAntiforgeryToken")?.dataset.statusUrl;
  const confirmButton = document.querySelector("#modal .dialog-foot .btn-primary");
  if (!token || !statusUrl) {
    toastMsg("امکان تغییر وضعیت حساب در حال حاضر وجود ندارد.");
    return;
  }

  if (confirmButton) confirmButton.disabled = true;
  const formData = new FormData();
  formData.append("__RequestVerificationToken", token);
  formData.append("id", pendingAccountStatus.accountId);
  formData.append("isActive", String(pendingAccountStatus.isActive));

  try {
    const response = await fetch(statusUrl, {
      method: "POST",
      body: formData,
      headers: { Accept: "application/json" },
    });
    const result = await response.json();
    if (!response.ok) throw new Error(result.message || "وضعیت حساب تغییر نکرد.");

    closeModal();
    toastMsg(result.message || "وضعیت حساب تغییر کرد.");
    window.setTimeout(() => window.location.reload(), 700);
  } catch (error) {
    toastMsg(error.message || "ارتباط با سرور برقرار نشد. دوباره تلاش کنید.");
    if (confirmButton) confirmButton.disabled = false;
  }
}

document.addEventListener("submit", async (event) => {
  const form = event.target;
  if (!(form instanceof HTMLFormElement) || form.id !== "accountForm") return;

  event.preventDefault();
  const errorBox = document.getElementById("accountFormErrors");
  const saveButton = document.querySelector("#modal .dialog-foot .btn-primary");
  errorBox.hidden = true;
  errorBox.textContent = "";
  if (saveButton) saveButton.disabled = true;

  try {
    const response = await fetch(form.action, {
      method: "POST",
      body: new FormData(form),
      headers: { Accept: "application/json" },
    });
    const result = await response.json();

    if (!response.ok) {
      errorBox.textContent = (result.errors || ["ساخت حساب انجام نشد."]).join(" ");
      errorBox.hidden = false;
      return;
    }

    closeModal();
    toastMsg(result.message || "حساب کاربری ساخته شد.");
    window.setTimeout(() => window.location.reload(), 900);
  } catch {
    errorBox.textContent = "ارتباط با سرور برقرار نشد. دوباره تلاش کنید.";
    errorBox.hidden = false;
  } finally {
    if (saveButton) saveButton.disabled = false;
  }
});

function toastMsg(msg) {
  document.getElementById("toastText").textContent = msg;
  document.getElementById("toast").classList.add("show");
  clearTimeout(window._tm);
  window._tm = setTimeout(
    () => document.getElementById("toast").classList.remove("show"),
    2600,
  );
}

function renumberArticleSections() {
  const container = document.getElementById("articleSections");
  if (!container) return;

  const sections = [...container.querySelectorAll(".article-section")];
  articleSectionCount = sections.length;

  sections.forEach((section, index) => {
    const number = index + 1;
    section.dataset.section = number;
    section.querySelector(".article-section-number").textContent =
      `قسمت ${number}`;

    const heading = section.querySelector('select[name*="HeadingType"]');
    const title = section.querySelector('input[name*="Title"]');
    const content = section.querySelector('textarea[name*="Content"]');

    if (heading) heading.name = `Sections[${index}].HeadingType`;
    if (title) title.name = `Sections[${index}].Title`;
    if (content) content.name = `Sections[${index}].Content`;

    const oldRemove = section.querySelector(".section-remove");
    if (number === 1) {
      oldRemove?.remove();
    } else if (!oldRemove) {
      section
        .querySelector(".article-section-head")
        .insertAdjacentHTML(
          "beforeend",
          `<button type="button" class="icon-btn section-remove" onclick="removeArticleSection(this)" title="حذف بخش"><i class="bi bi-trash3"></i></button>`,
        );
    }
  });
}

window.addArticleSection = function () {
  const container = document.getElementById("articleSections");
  if (!container) return;

  const number = container.querySelectorAll(".article-section").length + 1;
  container.insertAdjacentHTML("beforeend", articleSectionTemplate(number));
  renumberArticleSections();
};

window.removeArticleSection = function (button) {
  const section = button.closest(".article-section");
  if (!section) return;

  section.remove();
  renumberArticleSections();
};

function accountModalBody(account = {}) {
  const editMode = account.isEdit === true;
  const antiforgeryContainer = document.getElementById("accountAntiforgeryToken");
  const antiforgeryToken = antiforgeryContainer?.querySelector(
    "input[name='__RequestVerificationToken']",
  )?.value ?? "";
  const formAction = editMode
    ? antiforgeryContainer?.dataset.updateUrl
    : antiforgeryContainer?.dataset.createUrl;
  const accountRole = account.roleName || "";
  const profileImage = account.profileImagePath
    ? `<img class="account-image-preview" src="${escapeHtml(account.profileImagePath)}" alt="تصویر پروفایل فعلی">`
    : "";
  const passwordRequired = editMode ? "" : "required";
  const passwordLabel = editMode ? "رمز عبور جدید (اختیاری)" : "رمز عبور اولیه";

  return `<form id="accountForm" class="form-grid account-form-grid" action="${escapeHtml(formAction || "")}" method="post" enctype="multipart/form-data" data-edit-mode="${editMode}">
          <input type="hidden" name="__RequestVerificationToken" value="${escapeHtml(antiforgeryToken)}">
          ${editMode ? `<input type="hidden" name="Id" value="${escapeHtml(account.id)}">` : ""}
          <div id="accountFormErrors" class="account-form-errors" role="alert" hidden></div>
          <div class="field full">
            <label>تصویر پروفایل</label>
            <div class="upload" onclick="this.querySelector('input').click()">
              ${profileImage || `<i class="bi bi-cloud-arrow-up"></i>`}
              <strong style="display:block;margin-top:7px">${editMode ? "برای جایگزینی تصویر کلیک کنید" : "برای آپلود تصویر کلیک کنید"}</strong>
              <small>PNG، JPG یا WebP · حداکثر ۲ مگابایت</small>
              <input type="file" name="ProfileImage" accept="image/png,image/jpeg,image/webp" hidden>
            </div>
          </div>
          <div class="account-section-heading"><span>اطلاعات حساب</span></div>
          <div class="field"><label for="accountFirstName">نام</label><input id="accountFirstName" name="FirstName" value="${escapeHtml(account.firstName)}" required maxlength="60" placeholder="نام"></div>
          <div class="field"><label for="accountLastName">نام خانوادگی</label><input id="accountLastName" name="LastName" value="${escapeHtml(account.lastName)}" required maxlength="80" placeholder="نام خانوادگی"></div>
          <div class="field"><label for="accountUsername">نام کاربری</label><input id="accountUsername" name="UserName" value="${escapeHtml(account.userName)}" required maxlength="50" placeholder="نام کاربری"></div>
          <div class="field"><label for="accountEmail">ایمیل</label><input id="accountEmail" name="Email" type="email" value="${escapeHtml(account.email)}" required maxlength="120" placeholder="name@peaklabs.dev"></div>
          <div class="field"><label for="accountRole">نقش</label><select id="accountRole" name="RoleName" required><option value="" disabled ${accountRole ? "" : "selected"}>انتخاب نقش</option><option value="Editor" ${accountRole === "Editor" ? "selected" : ""}>ویرایشگر</option><option value="ContentManager" ${accountRole === "ContentManager" ? "selected" : ""}>مدیر محتوا</option><option value="SuperAdmin" ${accountRole === "SuperAdmin" ? "selected" : ""}>مدیر ارشد</option></select></div>
          ${editMode ? "" : `<div class="field"><label for="accountStatus">وضعیت حساب</label><select id="accountStatus" name="IsActive"><option value="true" selected>فعال</option><option value="false">غیرفعال</option></select></div>`}
          <div class="field full"><label for="accountRoleDescription">توضیح کوتاه درباره نقش در شرکت</label><textarea id="accountRoleDescription" name="AuthorDescription" maxlength="240" placeholder="مثلاً نویسنده مقالات حوزه طراحی و توسعه وب">${escapeHtml(account.authorDescription)}</textarea><small class="muted">این توضیح می‌تواند کنار مقاله‌های این شخص نمایش داده شود.</small></div>
          <div class="account-section-heading"><span>امنیت ورود</span></div>
          <div class="field"><label for="accountPassword">${passwordLabel}</label><div class="password-input-wrap"><input id="accountPassword" name="Password" type="password" ${passwordRequired} minlength="8" maxlength="100" placeholder="${editMode ? "برای حفظ رمز فعلی خالی بگذارید" : "حداقل ۸ کاراکتر"}"><button class="password-visibility-toggle" type="button" data-password-toggle="accountPassword" aria-label="نمایش رمز عبور" aria-pressed="false"><i class="bi bi-eye" aria-hidden="true"></i></button></div><small class="muted">در صورت تغییر: حروف کوچک و بزرگ انگلیسی، عدد و نماد لازم است.</small></div>
          <div class="field"><label for="accountPasswordConfirm">تکرار رمز عبور${editMode ? " جدید" : ""}</label><div class="password-input-wrap"><input id="accountPasswordConfirm" name="ConfirmPassword" type="password" ${passwordRequired} minlength="8" maxlength="100" placeholder="تکرار رمز عبور"><button class="password-visibility-toggle" type="button" data-password-toggle="accountPasswordConfirm" aria-label="نمایش رمز عبور" aria-pressed="false"><i class="bi bi-eye" aria-hidden="true"></i></button></div></div>
        </form>`;
}

document.addEventListener("click", (event) => {
  const editButton = event.target.closest(".account-edit-button");
  if (editButton) {
    openModal("accountEdit", {
      id: editButton.dataset.accountId,
      firstName: editButton.dataset.firstName,
      lastName: editButton.dataset.lastName,
      userName: editButton.dataset.userName,
      email: editButton.dataset.email,
      roleName: editButton.dataset.roleName,
      authorDescription: editButton.dataset.authorDescription,
      profileImagePath: editButton.dataset.profileImagePath,
    });
    return;
  }

  const statusButton = event.target.closest(".account-status-button");
  if (statusButton) {
    openAccountStatusModal(statusButton);
    return;
  }

  const deleteButton = event.target.closest(".account-delete-button");
  if (deleteButton) {
    openDeleteAccountModal(deleteButton);
    return;
  }

  const toggle = event.target.closest("[data-password-toggle]");
  if (!toggle) return;

  const input = document.getElementById(toggle.dataset.passwordToggle);
  if (!input) return;

  const showPassword = input.type === "password";
  input.type = showPassword ? "text" : "password";
  toggle.setAttribute("aria-pressed", String(showPassword));
  toggle.setAttribute("aria-label", showPassword ? "پنهان کردن رمز عبور" : "نمایش رمز عبور");
  toggle.innerHTML = `<i class="bi ${showPassword ? "bi-eye-slash" : "bi-eye"}" aria-hidden="true"></i>`;
});

function teamMemberModalBody(member = {}) {
  return `<div class="form-grid">
          <div class="field full"><label>تصویر پروفایل</label><div class="upload" onclick="this.querySelector('input').click()"><i class="bi bi-cloud-arrow-up"></i><strong style="display:block;margin-top:7px">برای آپلود تصویر کلیک کنید</strong><small>PNG، JPG یا WebP</small><input type="file" accept="image/*" hidden></div></div>
          <div class="field"><label>نام</label><input id="teamFirstName" maxlength="40"" placeholder="نام"></div>
          <div class="field"><label>نام خانوادگی</label><input id="teamLastName" maxlength="60" placeholder="نام خانوادگی"></div>
          <div class="field"><label>عنوان شغلی</label><input id="teamPosition" maxlength="80" placeholder="مثلاً Backend Developer"></div>
          <div class="field"><label>وضعیت نمایش</label><select id="teamVisible"><option value="true" >نمایش داده شود</option><option value="false">مخفی باشد</option></select></div>
          <div class="field full"><label>توضیح کوتاه</label><textarea id="teamBio" maxlength="300" placeholder="توضیح کوتاه درباره عضو تیم..."></textarea></div>
          <div class="field"><label>ایمیل</label><input id="teamEmail" type="email" maxlength="120"  placeholder="name@peaklabs.dev"></div>
          <div class="field"><label>GitHub</label><input id="teamGithub" maxlength="200" " placeholder="https://github.com/..."></div>
          <div class="field full"><label>LinkedIn</label><input id="teamLinkedin" maxlength="200" placeholder="https://linkedin.com/in/..."></div>
        </div>`;
}
