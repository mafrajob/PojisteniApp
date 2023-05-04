// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Image preview based on tutorial: https://w3collective.com/preview-selected-img-file-input-js/
const chooseImg = document.getElementById("choose-img");
const profileImg = document.getElementById("profile-img");
const validationImg = document.getElementById("validation-img");

function getImgData() {
    const image = chooseImg.files[0];
    if (image) {
        if ((image.size / (1024 * 1024)) > 1) {
            validationImg.innerText = "Maximální velikost obrázku je 1 MB";
        }
        else {
            const fileReader = new FileReader();
            fileReader.readAsDataURL(image);
            fileReader.addEventListener("load", function () {
                profileImg.src = this.result;
            });
            validationImg.innerText = "";
        }
    }
}

chooseImg.addEventListener("change", function () {
    getImgData();
});