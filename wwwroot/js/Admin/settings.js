(() => {
  window.saveSiteSettings = async () => {
    const form = document.getElementById("settingsForm");
    if (!form.reportValidity()) return;

    const fields = {
      siteTitle: "settingSiteTitle",
      siteDescription: "settingSiteDescription",
      siteUrl: "settingSiteUrl",
      seoTitle: "settingSeoTitle",
      seoDescription: "settingSeoDescription",
      ogImage: "settingOgImage",
      contactEmail: "settingContactEmail",
      contactPhone: "settingContactPhone",
      address: "settingAddress",
      linkedin: "settingLinkedin",
      github: "settingGithub",
      instagram: "settingInstagram",
    };

    const payload = Object.fromEntries(Object.entries(fields).map(([key, id]) => [key, document.getElementById(id).value]));
    const button = document.querySelector('button[form="settingsForm"]');
    button.disabled = true;
    try {
      const response = await fetch("/Admin/System/SaveSettings", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          RequestVerificationToken: form.querySelector('input[name="__RequestVerificationToken"]').value,
        },
        body: JSON.stringify(payload),
      });
      const result = await response.json();
      if (!response.ok) throw new Error(result.message || "ذخیره تنظیمات انجام نشد.");
      if (typeof window.toastMsg === "function") window.toastMsg(result.message);
    } catch (error) {
      if (typeof window.toastMsg === "function") window.toastMsg(error.message || "ذخیره تنظیمات انجام نشد.");
      else window.alert(error.message || "ذخیره تنظیمات انجام نشد.");
    } finally {
      button.disabled = false;
    }
  };

  const modal = document.getElementById("modal");
  if (!modal) return;

  const content = {
    hero: {
      title: "عنوان اصلی صفحهٔ نخست",
      description: "معرفی کوتاه سایت در ابتدای صفحهٔ اصلی نمایش داده می‌شود.",
    },
  };

  window.openContentSettings = (section) => {
    const data = content[section];
    if (!data) return;

    document.getElementById("modalTitle").textContent = "ویرایش محتوای Hero";
    document.getElementById("modalBody").innerHTML = `
      <div class="form-grid">
        <div class="field full">
          <label for="contentTitle">عنوان اصلی</label>
          <input id="contentTitle" type="text" maxlength="100" placeholder="${escapeAttribute(data.title)}" required>
          <small class="muted">عنوانی که در ابتدای صفحهٔ اصلی دیده می‌شود.</small>
        </div>
        <div class="field full">
          <label for="contentDescription">معرفی کوتاه</label>
          <textarea id="contentDescription" maxlength="300" placeholder="${escapeHtml(data.description)}" required></textarea>
          <small class="muted">حداکثر ۳۰۰ کاراکتر</small>
        </div>
      </div>`;

    const saveButton = modal.querySelector(".dialog-foot .btn-primary");
    saveButton.onclick = () => saveContentSettings(section);
    modal.classList.add("open");
    document.getElementById("contentTitle").focus();
  };

  window.saveContentSettings = (section) => {
    const title = document.getElementById("contentTitle");
    const description = document.getElementById("contentDescription");
    if (!title.reportValidity() || !description.reportValidity()) return;

    content[section] = { title: title.value.trim(), description: description.value.trim() };
    document.getElementById("heroTitlePreview").textContent = content[section].title;
    document.getElementById("heroDescriptionPreview").textContent = content[section].description;
    closeModal();
    if (typeof window.toastMsg === "function") window.toastMsg("محتوای Hero ذخیره شد");
  };

  const originalClose = window.closeModal;
  window.closeModal = () => {
    modal.classList.remove("open");
    const saveButton = modal.querySelector(".dialog-foot .btn-primary");
    saveButton.onclick = () => window.saveModal();
    if (originalClose && typeof originalClose === "function") originalClose();
  };

  function escapeHtml(value) {
    return value.replace(/[&<>"']/g, (char) => ({
      "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;",
    })[char]);
  }

  function escapeAttribute(value) {
    return escapeHtml(value);
  }
})();
