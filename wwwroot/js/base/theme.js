// Theme toggle button
const themeIcon = document.getElementById("themeIcon");
const themeToggle = document.getElementById("themeToggle");

const COOKIE_NAME = "Theme";

// Save theme in browser cookie
function saveTheme(theme) {
    document.cookie =
        `${COOKIE_NAME}=${theme}; max-age=2592000; path=/; SameSite=Lax`;
}

// Read saved theme
function getSavedTheme() {
    const match = document.cookie.match(/(?:^|;\s*)Theme=(light|dark)/);
    return match ? match[1] : "light";
}

// Apply theme
function applyTheme(theme) {
    document.documentElement.setAttribute("data-theme", theme);

    if (themeIcon) {
        themeIcon.className =
            theme === "dark" ? "bi bi-sun" : "bi bi-moon-stars";
    }
}

// Restore theme when page loads
applyTheme(getSavedTheme());

// Toggle theme
if (themeToggle) {
    themeToggle.addEventListener("click", function () {
        const currentTheme =
            document.documentElement.getAttribute("data-theme");

        const newTheme = currentTheme === "dark" ? "light" : "dark";

        applyTheme(newTheme);
        saveTheme(newTheme);
    });
}