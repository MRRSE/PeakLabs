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
let articleTags = [];

function articleSectionTemplate(number) {
  return `<div class="article-section" data-section="${number}">
          <div class="article-section-head">
            <span class="article-section-number">قسمت ${number}</span>
            ${number > 1 ? `<button type="button" class="icon-btn section-remove" onclick="removeArticleSection(this)" title="حذف بخش"><i class="bi bi-trash3"></i></button>` : ""}
          </div>
          <div class="form-grid">
            <div class="field">
              <label>نوع عنوان</label>
              <select name="Sections[${number - 1}].HeadingType">
                <option value="H2">H2</option>
                <option value="H3">H3</option>
                <option value="H4">H4</option>
                <option value="">بدون عنوان</option>
              </select>
            </div>
            <div class="field">
              <label>عنوان بخش</label>
              <input type="text" name="Sections[${number - 1}].Title" maxlength="100" placeholder="عنوان این بخش...">
            </div>
            <div class="field full">
              <label>محتوای بخش</label>
              <textarea name="Sections[${number - 1}].Content" maxlength="50000" style="min-height:150px" placeholder="محتوای این بخش را بنویسید..."></textarea>
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

function openModal(type) {
  modalType = type;
  document.getElementById("modal").classList.add("open");

  const title = {
    article: "ایجاد مقاله",
    portfolio: "نمونه کار جدید",
    developer: "توسعه دهنده جدید",
  }[type];

  document.getElementById("modalTitle").textContent = title;
  let body = "";

  if (type === "article") {
    articleSectionCount = 1;
    articleTags = [];

    body = `<div class="form-grid">
            <div class="field full">
              <label>تصویر کاور</label>
              <div class="upload" onclick="this.querySelector('input').click()">
                <i class="bi bi-cloud-arrow-up"></i>
                <strong style="display:block;margin-top:7px">برای آپلود کلیک کنید یا تصویر را اینجا رها کنید</strong>
                <small>PNG، JPG یا WebP · اندازه پیشنهادی 1600×1000</small>
                <input type="file" accept="image/*" hidden>
              </div>
            </div>

            <div class="field full">
              <label>عنوان مقاله</label>
              <input type="text" maxlength="60" placeholder="عنوان مقاله به فارسی">
              <small class="muted">حداکثر ۶۰ کاراکتر</small>
            </div>

            <div class="field full">
              <label>توضیح کوتاه</label>
              <textarea maxlength="160" placeholder="توضیح کوتاه برای کارت..."></textarea>
              <small class="muted">حداکثر ۱۶۰ کاراکتر</small>
            </div>

            <div class="field">
              <label>دسته‌بندی</label>
              <select>
                <option>Architecture</option>
                <option>AI</option>
                <option>Cloud</option>
                <option>ASP.NET</option>
              </select>
            </div>

            <div class="field">
              <label>هشتگ</label>
              <div style="display:flex;gap:8px">
                <input id="articleTagInput" type="text" maxlength="30" placeholder="مثلاً ASP.NET">
                <button type="button" class="btn btn-outline" onclick="addArticleTag()">
                  <i class="bi bi-plus-lg"></i>
                  افزودن
                </button>
              </div>
              <div id="articleTags" class="article-tags"></div>
            </div>

            <div class="field full">
              <div style="display:flex;align-items:center;justify-content:space-between;gap:10px;margin-bottom:10px">
                <div>
                  <label style="margin:0">بخش‌بندی مقاله</label>
                  <small class="muted">مقاله را به چند بخش جدا تقسیم کنید و برای هر بخش عنوان انتخاب کنید.</small>
                </div>
                <span class="tiny">عنوان برای هر بخش</span>
              </div>

              <div id="articleSections" class="article-sections">
                ${articleSectionTemplate(1)}
              </div>

              <button type="button" class="btn btn-outline section-add" onclick="addArticleSection()">
                <i class="bi bi-plus-lg"></i>
                افزودن قسمت جدید
              </button>
            </div>

            <div class="field full">
              <small class="muted"><i class="bi bi-clock"></i> تاریخ انتشار هنگام ذخیره مقاله به‌صورت خودکار توسط سیستم ثبت می‌شود.</small>
            </div>
          </div>`;
  }

  if (type === "portfolio")
    body = `<div class="form-grid"><div class="field full"><label>تصویر کاور</label><div class="upload" onclick="this.querySelector('input').click()"><i class="bi bi-image"></i><strong style="display:block;margin-top:7px">برای آپلود کلیک کنید یا تصویر را اینجا رها کنید</strong><small>PNG، JPG یا WebP · اندازه پیشنهادی 1600×1000</small><input type="file" accept="image/*" hidden></div></div><div class="field"><label>عنوان · FA</label><input maxlength="60" placeholder="نام پروژه"></div><div class="field"><label>لینک</label><input maxlength="60" placeholder="exampel.com"></div><div class="field full"><label>توضیح کوتاه · FA</label><textarea maxlength="160"></textarea>`;

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
  if (type === "teamMember") {
    body = teamMemberModalBody();
  }

  document.getElementById("modalBody").innerHTML = body;
}

function closeModal() {
  document.getElementById("modal").classList.remove("open");
}
function saveModal() {
  closeModal();
  toastMsg("ذخیره شد");
}
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

window.addArticleTag = function () {
  const input = document.getElementById("articleTagInput");
  if (!input) return;

  const value = input.value.trim();
  if (!value) return;

  if (articleTags.includes(value)) {
    input.value = "";
    return;
  }

  articleTags.push(value);
  input.value = "";
  renderArticleTags();
};

window.removeArticleTag = function (index) {
  articleTags.splice(index, 1);
  renderArticleTags();
};

function renderArticleTags() {
  const container = document.getElementById("articleTags");
  if (!container) return;

  container.innerHTML = articleTags
    .map(
      (tag, index) => `
          <span class="article-tag">
            #${tag}
            <button type="button" onclick="removeArticleTag(${index})" title="حذف هشتگ">
              <i class="bi bi-x-lg"></i>
            </button>
          </span>
        `,
    )
    .join("");
}

function accountModalBody(account = {}) {
  const selected = account.perms || [];
  const options = [
    ["all", "مدیریت کامل"],
    ["dashboard", "نمای کلی داشبورد"],
    ["articles", "مدیریت مقالات"],
    ["portfolio", "مدیریت نمونه‌کارها"],
    ["team", "مدیریت اعضای تیم"],
    ["analytics", "آمار و تحلیل"],
    ["settings", "تنظیمات"],
  ];
  return `<div class="form-grid">
          <div class="field"><label>نام</label><input id="accountFirstName" maxlength="40" placeholder="نام"></div>
          <div class="field"><label>نام خانوادگی</label><input id="accountLastName" maxlength="60" placeholder="نام خانوادگی"></div>
          <div class="field"><label>نام کاربری</label><input id="accountUsername" maxlength="50" placeholder="نام کاربری"></div>
          <div class="field"><label>ایمیل</label><input id="accountEmail" type="email" maxlength="120" placeholder="name@peaklabs.dev"></div>
          <div class="field"><label>نقش</label><select id="accountRole"><option value="Editor" ${account.role === "Editor" ? "selected" : ""}>ویرایشگر</option><option value="Lead developer" ${account.role === "Lead developer" ? "selected" : ""}>سرپرست توسعه</option><option value="Content Admin" ${account.role === "Content Admin" ? "selected" : ""}>مدیر محتوا</option><option value="Super Admin" ${account.role === "Super Admin" ? "selected" : ""}>مدیر ارشد</option></select></div>
          <div class="field"><label>وضعیت حساب</label><select id="accountStatus"><option value="online" ${account.status !== "offline" ? "selected" : ""}>فعال</option><option value="offline" ${account.status === "offline" ? "selected" : ""}>غیرفعال</option></select></div>
          <div class="field"><label>${account.id ? "رمز عبور جدید (اختیاری)" : "رمز عبور اولیه"}</label><input id="accountPassword" type="password" minlength="8" maxlength="100" placeholder="حداقل ۸ کاراکتر"></div>
          <div class="field"><label>تکرار رمز عبور</label><input id="accountPasswordConfirm" type="password" minlength="8" maxlength="100" placeholder="تکرار رمز عبور"></div>
          <div class="field full"><label>دسترسی‌ها</label><select id="accountPermissions" multiple size="6">${options.map(([value, label]) => `<option value="${value}" ${selected.includes(value) ? "selected" : ""}>${label}</option>`).join("")}</select><div class="multi-select-note">برای انتخاب چند مورد، در ویندوز کلید Ctrl و در مک کلید Command را نگه دارید.</div></div>
        </div>`;
}

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
