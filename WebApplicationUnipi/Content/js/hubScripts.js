$(document).ready(function () {

    document.querySelector(".loader") && document.querySelector(".loader").classList.remove('shown');

    

    //show - hide password
    let password = document.getElementById('Password');
    let toggler = document.getElementById('togglePass');
    showHidePassword = () => {
        if (password.type == 'password') {
            password.setAttribute('type', 'text');
            toggler.classList.add('fa-eye-slash');
        } else {
            toggler.classList.remove('fa-eye-slash');
            password.setAttribute('type', 'password');
        }
    };
    if (password) {
        toggler.addEventListener('click', showHidePassword);
    }

    $(".view-content").on("click", function () {
        if ($(this).hasClass("smallMnView")) return;
        sideMenuToggle();
    })

    if ($(".view-content").find(".dx.dxm-image-l").length) {
        $(".view-content").find(".dx.dxm-image-l").on("click", function () {
            window.removeEventListener("beforeunload", loaderOn)
            setTimeout(() => {
                window.addEventListener('beforeunload', loaderOn);
            }, 50)
        })
    }

});

const validate_Mail = (mail) => {
    let emailReg = new RegExp(/^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$/i);
    return emailReg.test(mail);
}
function underlineMove(poso) { //Views that have Tabs on top (moves underline below the active tab)
    let left = Number(poso - 1) * 210;
    document.querySelector(".tab").style.setProperty('--left', left + "px");
}

window.addEventListener('beforeunload', loaderOn);
window.addEventListener('resize', function () {
    if (!document.querySelectorAll('.notif-open').length) return;
    document.querySelector('.notif-open').classList.remove('notif-open');
});
window.addEventListener('click', function (e) {
    if ([...$(e.target).parents()].find(el => el.classList.contains('notif-area')) !== undefined) return;
    document.querySelector('.notif-open') !== null &&
        document.querySelector('.notif-open').classList.remove('notif-open');
});

function loaderOn() {
    document.querySelector(".loader").classList.add('shown');
}

//expects date: Day/Month/Year format as a string.
function fixEndDateAccordingToStart(startDayInp, endDayInp, seperator = '-') {
    const startD = startDayInp.value.split(seperator).map(s => parseInt(s));
    const endD = endDayInp.value.split(seperator).map(e => parseInt(e));
    if (!endD.length) return;

    const sDateObj = new Date(startD[2], startD[1] - 1, startD[0]);
    const eDateObj = new Date(endD[2], endD[1] - 1, endD[0]);

    if (sDateObj.getTime() > eDateObj.getTime()) {
        endDayInp.value = startDayInp.value;
    }
}

function fixEndTimeAccordingToStart(startTimeInp, endTimeInp) {
    const startT = startTimeInp.value.split(':').map(s => parseInt(s));
    const endT = endTimeInp.value.split(':').map(e => parseInt(e));
    if (!endT.length) return;

    const [sTimeObj, eTimeObj] = [new Date(), new Date()];
    sTimeObj.setHours(startT[0], startT[1], 0, 0);
    eTimeObj.setHours(endT[0], endT[1], 0, 0);
    if (sTimeObj.getTime() > eTimeObj.getTime()) {
        endTimeInp.value = startTimeInp.value;
    }
}

function sideMenuToggle() {
    $(".sideMenu").toggleClass("smallMenu");
    $(".view-content").hasClass("smallMnView") ? $(".view-content").addClass("fullView").removeClass("smallMnView") : $(".view-content").addClass("smallMnView").removeClass("fullView");
    $(".sideMenu a").each(function (index, value) {
        //$(this).find("div").find("div").toggleClass("hideTxt");
        $(this).find("div").find("div").hasClass("hideTxt") ? $(this).find("div").find("div").removeClass("hideTxt").addClass("showTxt") : $(this).find("div").find("div").addClass("hideTxt").removeClass("showTxt");
    });
    $('#menu-icon').toggleClass("open");
}

function escapeBeforeUnload() { // used in all export and download actions to prevent loader from staying on screen forever 
    window.removeEventListener("beforeunload", loaderOn)
    setTimeout(() => {
        window.addEventListener('beforeunload', loaderOn);
    }, 50);

}

