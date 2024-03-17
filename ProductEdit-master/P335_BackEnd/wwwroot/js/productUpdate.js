const imgInput = document.querySelector('.img-input');
const imgPreview = document.querySelector('.img-preview');
const imgDel = document.querySelector('.delll');

imgInput.addEventListener('change', (e) => {
    let img = e.target.files[0]
    let blobUrl = URL.createObjectURL(img)
    imgPreview.setAttribute('src', blobUrl)
})

imgDel.addEventListener('click', () => {
    imgPreview.setAttribute('src', "");
    imgDel.remove();
})