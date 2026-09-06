//Mobile navbar 
const hamburgerBtn = document.getElementById("hamburgerBtn");
const mobilePanel = document.getElementById("mobilePanel");
hamburgerBtn.addEventListener("click", function () {
  const open = hamburgerBtn.classList.toggle("open");
  mobilePanel.classList.toggle("open");
  hamburgerBtn.setAttribute("aria-expanded", open);
});
mobilePanel.querySelectorAll("a").forEach(function (a) {
  a.addEventListener("click", function () {
    hamburgerBtn.classList.remove("open");
    mobilePanel.classList.remove("open");
  });
});