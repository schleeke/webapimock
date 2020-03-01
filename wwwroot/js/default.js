/*
    .ctor()
    Executed on document load.
*/
$( document ).ready(function() {
    $('body').bootstrapMaterialDesign();
    hideViewRequestsButton();
    hideAlert();
    hideCreateRequestButton();
    getAmountOfMockupRequests();
});

function getAmountOfMockupRequests() {
    var url = window.location;
    var url = url.protocol + "//" + url.host + "/request";
    $.ajax({
        url: url,
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            toggleCreateRequestsButton();
            if (data == "") {
                setRequestContLabelText("No requests in database.");
                return; }
            var count = data.length;
            setRequestContLabelText(count + " Request(s) in database.");
            toggleViewRequestsButton();
        },
        error: function (request, status, errorText) {
            hideRequestContentLabelText();
            showAlertToast('Unable to read requests', status + ':' + errorText);
        }
    });
}

function setRequestContLabelText(text) {
    $("#request-count-label").text(text);
}
function hideRequestContentLabelText() {
    $("#request-count-label").hide();
}
function hideViewRequestsButton() {
    $("#btn-view-requests").hide();
}
function toggleViewRequestsButton() {
    $("#btn-view-requests").toggle();
}
function hideCreateRequestButton() {
    $("#btn-create-requests").hide();
}
function toggleCreateRequestsButton() {
    $("#btn-create-requests").toggle();
}
function showAlertToast(header, message) {
    $("#webapi-alert-header").text(header);
    $("#webapi-alert-message").text(message);
    $('#webapi-alert').show();
}
function hideAlert() {
    $('#webapi-alert').hide();
}
