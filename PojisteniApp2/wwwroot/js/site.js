// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Add your JavaScript code

// Disable select by Tab key for all elements with class disabled
function disableForTabKey() {
    const disabledElements = document.getElementsByClassName("disabled");
    for (const element of disabledElements) {
        element.setAttribute("tabindex", "-1");
    }
}

//document.onload event not fired as expected, maybe because of ASP.NET CORE Razor View Engine
document.addEventListener("DOMContentLoaded", function () {
    disableForTabKey();
});