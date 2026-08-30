// Theme toggle button
const themeIcon = document.getElementById("themeIcon")
const themeToggle = document.getElementById("themeToggle")

themeToggle.addEventListener("click" , function(){

  const html = document.documentElement;

  if(html.getAttribute("data-theme") === "dark"){
    html.setAttribute("data-theme", "light");
    themeIcon.className = "bi bi-moon-stars";
  }
  else{
    html.setAttribute("data-theme" , "dark");
    themeIcon.className = "bi bi-sun"
  }
})

//Mobile nvbar 
const hamburgerBtn = document.getElementById("hamburgerBtn");
const mobilePanel = document.getElementById("mobilePanel");
hamburgerBtn.addEventListener("click", function(){
  const open = hamburgerBtn.classList.toggle("open");
  mobilePanel.classList.toggle("open");
  hamburgerBtn.setAttribute("aria-expanded", open);
});
mobilePanel.querySelectorAll("a").forEach(function(a){
  a.addEventListener("click", function(){
    hamburgerBtn.classList.remove("open");
    mobilePanel.classList.remove("open");
  });
});