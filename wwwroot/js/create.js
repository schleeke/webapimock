/*
    Code behind file for create.html
*/
var canCreateNew = false;       // indicates if all information for creating a new request is available.
var isMethodSelected = false;   // indicates if a HTTP method is selected.
var isMimeTypeSelected = false; // indicates if a MIME type is selected.
var isResponseReady = false;    // indicates if all information for a valid response is available.

/*
    .ctor()
    Executed on document load.
*/
$( document ).ready(function() {
    validateResponse();
    setMimeTypeSelectorVisible();
    setResponseTypeVisibility();
    $("#btn-home").tooltip();
});

/*
    DOM event bindings
    Methods bound to DOM events within the HTML document.
*/
function onHttpMethodSelect(value) { // Executed when the selection of the HTTP method drop down is changed.
    isMethodSelected = true;
    if (value == 'GET') {
        $('#request-body-row').hide();
    } else {
        $('#request-body-row').show();
    }
    validateRequest();
}
function onMimeTypeSelect(value) { // Executed when the selection of the MIME type drop down is changed.
    isMimeTypeSelected = true;
    validateResponse();
}
function onRouteTextChange() { // Executed when the text of the route changes.
    validateRequest();
}
function onResponseContentTextChange() { // Executed when the text of the response content changes.
    setMimeTypeSelectorVisible();
    validateResponse();
}
function onResponseStatusCodeChanged() { // Executed when the text of the response HTTP status code changes.
    validateResponse();
}

/*
    Custom/internal code.
*/
function validateRequest() { // Checks if all information for a request is valid.
    var routeText = $('#form-route-text').val();
    var isBlank = routeText == "";
    var hasRouteText = !isBlank;
    canCreateNew = (isMethodSelected && hasRouteText && isResponseReady);
    if(canCreateNew) {
        //$('#btn-create-request').show();
        $('#btn-create-request').prop("disabled", false);
    }
    else {
        //$('#btn-create-request').hide();
        $('#btn-create-request').prop("disabled", true);
    }
}
function validateResponse() { // Checks if all information for a response is valid.
    var canContinue = true;
    var isNew = $('#form-use-new-response').is(':checked');
    if(isNew) {
        if(!isStatuscodeInputValid()) {
            canContinue = false; }
        if(canContinue) {
            var responseContent = $('#form-request-responsecontent').val();
            var isEmpty = responseContent == "";
            if(!isEmpty) {
                canContinue = isMimeTypeSelected; }
        }
        isResponseReady = canContinue;
    } else {
        isResponseReady = false;
    }


    validateRequest();
}
function setMimeTypeSelectorVisible() { // Sets the MIME type drop down selector visibility according to the response content.
    var responseContent = $('#form-request-responsecontent').val();
    var isEmpty = responseContent == "";
    if(isEmpty) {
        $('#response-mime-type-row').hide();
    } else {
        $('#response-mime-type-row').show();
    }
}
function isStatuscodeInputValid() { // Checks if the value for the HTTP status code is valid.
    var statusCodeText = $('#form-request-statuscode').val();
    var isNumber = !isNaN(statusCodeText);
    if(!isNumber) { return false; }
    var value = parseInt(statusCodeText);
    return (value >= 100 && value < 1000);
}
function setResponseTypeVisibility() { // Sets the visibility of the different response type areas.
    var isNew = $('#form-use-new-response').is(':checked');
    if(isNew) {
        $('#existing-response-area').hide();
        $('#new-response-area').show();
    } else {
        $('#new-response-area').hide();
        $('#existing-response-area').show();
        getExistingResponses();
    }
    validateResponse();
}
function gotoHome() {
    window.document.location.href = 'default.html';
}
function getExistingResponses() {
    var url = window.location;
    var url = url.protocol + "//" + url.host + "/response";
    console.log("Calling " + url);
    $.ajax({
        url: url,
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            if (data == "") {
                return; }
            var count = data.length;
            $('#responsetable').empty();
            for (var i = 0; i < count; i++) {
                var item = data[i];
                $('#responsetable').append('<tr><td>' + item.statusCode + '</td><td>' + item.response + '</td><td>' + item.mimeType + '</td></tr>');
            }
        },
        error: function (request, status, errorText) {
            console.log("AJAX call failed: " + errorText);
            alert(errorText);
        }
    });

}