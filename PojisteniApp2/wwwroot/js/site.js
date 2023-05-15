﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Add your JavaScript code

// Disable select by Tab key for all elements with class disabled
// Add title to it's wrapper
function adjustDisabledElements() {
    const disabledElements = document.getElementsByClassName("disabled");
    for (const element of disabledElements) {
        element.setAttribute("tabindex", "-1");
        let titleWrapper = element.closest("span.disabled-title-wrapper");
        if (titleWrapper) {
            titleWrapper.setAttribute("title", "Pouze pro přihlášeného uživatele, autora nebo administrátora");
            titleWrapper.setAttribute("tabindex", "0");
        }
    }
}

//document.onload event not fired as expected, maybe because of ASP.NET CORE Razor View Engine
document.addEventListener("DOMContentLoaded", adjustDisabledElements());