document.addEventListener("change", (event) => {
  if (event.target.id !== "portfolioImage") return;
  const name = document.getElementById("portfolioImageName");
  const file = event.target.files?.[0];
  if (name) name.textContent = file?.name || "";

  const preview = document.getElementById("portfolioImagePreview");
  if (preview && file) preview.src = URL.createObjectURL(file);
});

document.addEventListener("click", (event) => {
  const editButton = event.target.closest(".portfolio-edit-button");
  if (editButton) {
    openModal("portfolioEdit", {
      id: editButton.dataset.portfolioId,
      name: editButton.dataset.portfolioName,
      description: editButton.dataset.portfolioDescription,
      link: editButton.dataset.portfolioLink,
      imagePath: editButton.dataset.portfolioImage,
    });
    return;
  }

  const deleteButton = event.target.closest(".portfolio-delete-button");
  if (deleteButton) openPortfolioDeleteModal(deleteButton);
});

document.addEventListener("submit", async (event) => {
  const form = event.target;
  if (!(form instanceof HTMLFormElement) || form.id !== "portfolioForm") return;

  event.preventDefault();
  const errorBox = document.getElementById("portfolioFormErrors");
  const saveButton = document.querySelector("#modal .dialog-foot .btn-primary");
  const token = document.querySelector(
    "#portfolioAntiforgeryToken input[name='__RequestVerificationToken']",
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
    if (!response.ok) throw new Error(result.message || "ذخیره نمونه‌کار انجام نشد.");

    closeModal();
    toastMsg(result.message || "نمونه‌کار ذخیره شد.");
    window.setTimeout(() => window.location.reload(), 500);
  } catch (error) {
    errorBox.textContent = error.message || "ارتباط با سرور برقرار نشد. دوباره تلاش کنید.";
    errorBox.hidden = false;
    if (saveButton) saveButton.disabled = false;
  }
});
