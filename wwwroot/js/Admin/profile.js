const imageInput = document.getElementById("profileImageInput");
const avatarImage = document.getElementById("avatarImage");
const avatarText = document.getElementById("avatarText");

imageInput.addEventListener("change", function () {
    const file = this.files[0];

    if (!file || !file.type.startsWith("image/")) {
        return;
    }

    const reader = new FileReader();

    reader.onload = function (e) {
        avatarImage.src = e.target.result;
        avatarImage.style.display = "block";
        avatarText.style.display = "none";
    };

    reader.readAsDataURL(file);
});