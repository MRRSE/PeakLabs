const pwInput = document.getElementById("loginPassword");
const togglePw = document.getElementById("togglePw");
const togglePwIcon = document.getElementById("togglePwIcon");
togglePw.addEventListener("click", function () {
  const show = pwInput.type === "password";
  pwInput.type = show ? "text" : "password";
  togglePwIcon.className = show ? "bi bi-eye-slash" : "bi bi-eye";
  togglePw.setAttribute("aria-label", show ? "Hide password" : "Show password");
});
