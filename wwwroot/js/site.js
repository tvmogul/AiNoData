document.addEventListener("DOMContentLoaded", function () {
    //alert("$(document).ready Loads First!");

    if (!sessionStorage.getItem("splashShown")) {
        showSplashModal();
        sessionStorage.setItem("splashShown", "true");
    }

    // Check if the splash modal was already shown
    if (!localStorage.getItem("splashShown")) {
        // Call your function
        showSplashModal();
        // Mark as shown so it doesn't show again
        localStorage.setItem("splashShown", "true");
    }

    localStorage.setItem('bankLetterFilter', 'A');

});

document.getElementById('btnCreateCompany').addEventListener('click', function () {
    //alert('Create Company clicked');
    window.location.href = '/Company/Index';
});

document.getElementById('btnAddAccounts').addEventListener('click', function () {
    //alert('Add Accounts clicked');
    window.location.href = '/Account/Index';
});

document.getElementById('btnMarketing').addEventListener('click', function () {
    const helpUrl = encodeURIComponent("https://ainetprofit.com/manual");
    fetch(`/Home/LaunchHelp?url=${helpUrl}`)
        .then(response => {
            if (!response.ok) throw new Error("Failed to launch help");
            console.log("Help launched");
        })
        .catch(error => console.error("Error:", error));
});


