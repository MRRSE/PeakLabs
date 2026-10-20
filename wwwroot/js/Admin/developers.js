(() => {
  const editor = document.getElementById("developerEditor");
  const form = document.getElementById("developerForm");
  const page = document.getElementById("developerPage");
  if (!editor || !form || !page) return;

  const token = document.querySelector("#developerAntiforgeryToken input[name='__RequestVerificationToken']")?.value;
  const errorBox = document.getElementById("developerFormError");
  const toast = document.getElementById("developerToast");
  const imagePreview = document.getElementById("developerImagePreview");
  let previewUrl;
  const fields = Object.fromEntries([...form.elements].filter((element) => element.name).map((element) => [element.name, element]));
  let toastTimer;

  function notify(message) {
    toast.textContent = message;
    toast.classList.add("visible");
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.remove("visible"), 3200);
  }

  function open(item = {}) {
    form.reset();
    if (previewUrl) URL.revokeObjectURL(previewUrl);
    previewUrl = null;
    imagePreview.hidden = !item.profileImage;
    imagePreview.src = item.profileImage || "";
    fields.Id.value = item.id || 0;
    fields.Name.value = item.name || "";
    fields.JobTitle.value = item.jobTitle || "";
    fields.Email.value = item.email || "";
    fields.Bio.value = item.bio || "";
    fields.LinkedInUrl.value = item.linkedin || "";
    fields.GitHubUrl.value = item.github || "";
    fields.IsActive.checked = item.active === true || item.active === "True";
    errorBox.hidden = true;
    editor.hidden = false;
    fields.Name.focus();
  }
  function close() { editor.hidden = true; }

  fields.ProfileImage.addEventListener("change", () => {
    if (previewUrl) URL.revokeObjectURL(previewUrl);
    previewUrl = fields.ProfileImage.files[0] ? URL.createObjectURL(fields.ProfileImage.files[0]) : null;
    imagePreview.src = previewUrl || "";
    imagePreview.hidden = !previewUrl;
  });

  document.getElementById("developerAddButton").addEventListener("click", () => open());
  document.getElementById("developerCancel").addEventListener("click", close);
  editor.addEventListener("click", (event) => { if (event.target === editor) close(); });

  document.addEventListener("click", async (event) => {
    const edit = event.target.closest(".developer-edit");
    if (edit) {
      open({ id: edit.dataset.id, name: edit.dataset.name, jobTitle: edit.dataset.jobTitle, email: edit.dataset.email, bio: edit.dataset.bio, linkedin: edit.dataset.linkedin, github: edit.dataset.github, profileImage: edit.dataset.profileImage, active: edit.dataset.active });
      return;
    }

    const toggle = event.target.closest(".developer-toggle");
    if (toggle) {
      toggle.disabled = true;
      const body = new FormData();
      body.append("id", toggle.dataset.id);
      body.append("__RequestVerificationToken", token || "");
      try {
        const response = await fetch(page.dataset.toggleUrl, { method: "POST", body, headers: { Accept: "application/json" } });
        const result = await response.json();
        if (!response.ok) throw new Error(result.message || "تغییر وضعیت انجام نشد.");
        notify(result.message);
        window.location.reload();
      } catch (error) { notify(error.message); toggle.disabled = false; }
      return;
    }

    const remove = event.target.closest(".developer-delete");
    if (remove && window.confirm(`توسعه‌دهنده «${remove.dataset.name}» حذف شود؟`)) {
      remove.disabled = true;
      const body = new FormData();
      body.append("id", remove.dataset.id);
      body.append("__RequestVerificationToken", token || "");
      try {
        const response = await fetch(page.dataset.deleteUrl, { method: "POST", body, headers: { Accept: "application/json" } });
        const result = await response.json();
        if (!response.ok) throw new Error(result.message || "حذف انجام نشد.");
        notify(result.message);
        window.location.reload();
      } catch (error) { notify(error.message); remove.disabled = false; }
    }
  });

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    errorBox.hidden = true;
    const button = form.querySelector("[type='submit']");
    button.disabled = true;
    const body = new FormData(form);
    body.append("__RequestVerificationToken", token || "");
    try {
      const response = await fetch(page.dataset.saveUrl, { method: "POST", body, headers: { Accept: "application/json" } });
      const result = await response.json();
      if (!response.ok) throw new Error(result.message || "ذخیره انجام نشد.");
      close();
      window.location.reload();
    } catch (error) {
      errorBox.textContent = error.message;
      errorBox.hidden = false;
    } finally { button.disabled = false; }
  });
})();
