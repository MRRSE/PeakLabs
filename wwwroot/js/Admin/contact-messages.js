(() => {
  const page = document.getElementById("contactMessagesPage");
  if (!page) return;

  const token = document.querySelector("#contactMessagesToken input[name='__RequestVerificationToken']")?.value || "";
  const dialog = document.getElementById("contactMessageDialog");
  const dialogTitle = document.getElementById("contactMessageDialogTitle");
  const dialogSender = document.getElementById("contactMessageDialogSender");
  const dialogBody = document.getElementById("contactMessageDialogBody");

  document.querySelectorAll(".contact-message-preview").forEach((button) => {
    button.addEventListener("click", () => {
      dialogTitle.textContent = button.dataset.subject || "پیام";
      dialogSender.textContent = button.dataset.sender || "";
      dialogBody.textContent = button.dataset.message || "";
      dialog.showModal();
    });
  });

  document.getElementById("contactMessageDialogClose").addEventListener("click", () => dialog.close());
  dialog.addEventListener("click", (event) => {
    if (event.target === dialog) dialog.close();
  });

  document.querySelectorAll(".contact-message-answered").forEach((checkbox) => {
    checkbox.addEventListener("change", async () => {
      checkbox.disabled = true;
      const body = new FormData();
      body.append("id", checkbox.dataset.id);
      body.append("answered", String(checkbox.checked));
      body.append("__RequestVerificationToken", token);
      try {
        const response = await fetch(page.dataset.updateUrl, {
          method: "POST",
          body,
          headers: { Accept: "application/json" }
        });
        const result = await response.json();
        if (!response.ok) throw new Error(result.message || "تغییر وضعیت پیام انجام نشد.");
        window.location.reload();
      } catch (error) {
        checkbox.checked = !checkbox.checked;
        checkbox.disabled = false;
        window.alert(error.message || "ارتباط با سرور برقرار نشد.");
      }
    });
  });
})();
