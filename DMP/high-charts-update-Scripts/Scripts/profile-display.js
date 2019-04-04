$(document).ready(function () {
    $("#userdetail").click(function (e) {
        window.document.location = '/Profile/ProfileDetail?profileId=' + profileId;
    });

    $('#changepassword').click(function () {
        window.document.location = '/Account/ResetPassword';
    });

    $("#logoff").click(function (e, n) {
        $.ajax({
            type: "POST",
            url: '/Account/LogOff',
            contentType: "application/json; charset=utf-8",
            cache: false,
        }).done(function (dmpId) {
            window.location.replace('/Home/index');
        }).error(function (xhr, status, err) {
            alert(err);
            console.log(err);
        });
        return false;
    });
});