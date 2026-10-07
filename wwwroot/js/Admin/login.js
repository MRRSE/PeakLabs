const passwordInput = document.getElementById("loginPassword");
const passwordToggle = document.getElementById("togglePw");
const passwordToggleIcon = document.getElementById("togglePwIcon");

passwordToggle?.addEventListener("click", () => {
  const showPassword = passwordInput.type === "password";
  passwordInput.type = showPassword ? "text" : "password";
  passwordToggleIcon.className = showPassword ? "bi bi-eye-slash" : "bi bi-eye";
  passwordToggle.setAttribute(
    "aria-label",
    showPassword ? "پنهان کردن رمز عبور" : "نمایش رمز عبور",
  );
});

const providerModal = document.getElementById("providerModal");
const openProviderModal = () => {
  providerModal.classList.add("open");
  providerModal.setAttribute("aria-hidden", "false");
  document.getElementById("dismissProviderModal")?.focus();
};
const closeProviderModal = () => {
  providerModal.classList.remove("open");
  providerModal.setAttribute("aria-hidden", "true");
};

document.getElementById("githubBtn")?.addEventListener("click", openProviderModal);
document.getElementById("googleBtn")?.addEventListener("click", openProviderModal);
document.getElementById("closeProviderModal")?.addEventListener("click", closeProviderModal);
document.getElementById("dismissProviderModal")?.addEventListener("click", closeProviderModal);
providerModal?.querySelector("[data-close-provider-modal]")?.addEventListener("click", closeProviderModal);

document.addEventListener("keydown", (event) => {
  if (event.key === "Escape" && providerModal?.classList.contains("open")) {
    closeProviderModal();
  }
});
