(() => {
    const imageInput = document.getElementById("profileImageInput");
    const imageButton = document.getElementById("profileImageButton");
    const avatarImage = document.getElementById("avatarImage");
    const avatarText = document.getElementById("avatarText");

    if (!imageInput || !imageButton || !avatarImage || !avatarText) return;

    imageButton.addEventListener("click", () => imageInput.click());

    imageInput.addEventListener("change", () => {
        const [file] = imageInput.files;
        if (!file || !file.type.startsWith("image/")) return;

        const reader = new FileReader();
        reader.addEventListener("load", () => {
            avatarImage.src = reader.result;
            avatarImage.classList.add("is-visible");
            avatarText.classList.add("is-hidden");
        });
        reader.readAsDataURL(file);
    });
})();
