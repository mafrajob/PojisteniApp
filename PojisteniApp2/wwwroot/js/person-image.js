// Image preview based on tutorial: https://w3collective.com/preview-selected-img-file-input-js/
const chooseImg = document.getElementById("choose-img");
const profileImg = document.getElementById("profile-img");
const validationTextImg = document.getElementById("validation-text-img");
const maxSizeImg = parseFloat(document.getElementById("max-size-img").value);

function getImgData() {
    const image = chooseImg.files[0];
    if (image) {
        if ((image.size / (1024 * 1024)) > maxSizeImg) {
            validationTextImg.innerText = `Maximální velikost obrázku je ${maxSizeImg} MB`;
        }
        else {
            const fileReader = new FileReader();
            fileReader.readAsDataURL(image);
            fileReader.addEventListener("load", function () {
                profileImg.src = this.result;
            });
            validationTextImg.innerText = "";
        }
    }
}

chooseImg.addEventListener("change", function () {
    getImgData();
});