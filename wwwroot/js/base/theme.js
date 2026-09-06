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
