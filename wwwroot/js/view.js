var table = null;
var responseTable = null;

$( document ).ready(function() {
  getRequestData();
  getResponseData();
});

/**
 * Navigates home.
 * 
 * sets the current location to default.html.
 */
function gotoHome() {
  window.document.location.href = 'default.html';
}

/**
 * Reads all requests.
 * 
 * Makes an ajax call to retrieve the JSON data for the requests.
 */
async function getRequestData() {
  var url = getRootUrl();
  url = url + 'request';
  $.ajax({
    url: url,
    error: function(request, status, errorText) { alert(errorText); },
    success: OnRequestDataReceived,  
  });
}

/**
 * Reads all responses.
 *
 * Makes an ajax call to retrieve the JSON data for the responses.
 */
async function getResponseData() {
  var url = getRootUrl();
  url = url + 'response';
  $.ajax({
    url: url,
    error: function(request, status, errorText) { alert(errorText); },
    success: OnResponseDataReceived,  
  });

}

/**
 * Navigates to help.
 *
 * sets the current location to help.html.
 */
function gotoHelp() {
  window.document.location.href = 'help.html';
}

/**
 * Called when all responses are received.
 *
 * AJAX callback method for getting the responses.
 * Draws the data to the response table.
 * 
 * @param   data    An array of response objects.
 */
function OnResponseDataReceived(data) {
  $('#response-table-body').empty();
  $('#response-table-body').hide();
  if(data == null || data == '') { return; }
  data.forEach(element => {
      var rawRowContent = "<tr id=\"response-id-" + element.id + "\"><td>" + element.id + "</td><td>" + element.statusCode + "</td><td>" + element.mimeType + "</td><td>" + element.response + "</td><td><button type=\"button\" class=\"btn btn-danger btn-sm\" onclick=\"removeResponse("  + element.id +  ")\"><i class=\"material-icons fas fa-trash-alt\"></i></button></td></tr>";
    $('#response-table-body').append(rawRowContent);
  });
    $('#response-table-body').show();
  responseTable = $('#response-table').DataTable();
    $('#response-table-body').on('click', 'tr', onResponseRowClicked);
}

/**
 * Called when all request are received.
 *
 * AJAX callback method for getting the request.
 * Draws the data to the request table.
 *
 * @param   data    An array of request objects.
 */
function OnRequestDataReceived(data) {
  $('#request-table-body').empty();
  $('#request-table-body').hide();
  if(data == null || data == '') { return; }
  data.forEach(element => {
      var rawRowContent = "<tr id=\"request-id-" + element.id + "\"><td>" + element.route + "</td><td>" + element.httpMethod + "</td><td>" + element.query + "</td><td>" + element.body + "</td><td><button type=\"button\" class=\"btn btn-danger btn-sm\" onclick=\"removeRequest(" + element.id +  ")\"><i class=\"material-icons fas fa-trash-alt\"></i></button></td></tr>";
    $('#request-table-body').append(rawRowContent);
  });
    $('#request-table-body').show();
  table = $('#request-table').DataTable();
    $('#request-table-body').on('click', 'tr', onRequestRowClicked);
}

/**
 * Executed when a row in the response table is clicked.
 *
 * Handles selecting the clicked row and deselecting the old one.
 */
function onResponseRowClicked() {
  if ( $(this).hasClass('table-active') ) {
    $(this).removeClass('table-active'); }
  else {
    responseTable.$('tr.table-active').removeClass('table-active');
    $(this).addClass('table-active'); }
}

/**
 * Executed when a row in the request table is clicked.
 *
 * Handles selecting the clicked row and deselecting the old one.
 */
function onRequestRowClicked() {
  if ( $(this).hasClass('table-active') ) {
    $(this).removeClass('table-active'); }
  else {
    table.$('tr.table-active').removeClass('table-active');
    $(this).addClass('table-active'); }
}

/**
 * Returns the base url of the application/server.
 *
 * Extracts the root URL from the current location.
 */
function getRootUrl() {
  var location = window.location.origin 
                  ? window.location.origin + '/'
        : window.location.protocol + '/' + window.location.host + ':' + window.location.port + '/';
  return location;
}

/**
 * Called when an error during an AJAX call occurs.
 * @param {any} request     The HTTP request of the failed call.
 * @param {any} status      The status of the failed call.
 * @param {any} errorText   The error description text.
 */
function onErrorOccured(request, status, errorText) {
    console.error(errorText);
    alert('An error occured: ' + errorText);
}

/**
 * Removes a response from the database.
 *
 * Executes the AJAX call needed to perform the task.
 * 
 * @param   id  The id of the response to remove.
 * @param   askForConfirmation Toggles the confirmation dialog.
 */
function removeResponse(id, askForConfirmation = true) {
    console.log('Attempting to remove response #' + id + ' from database...');
    var rootUrl = getRootUrl();
    var ajaxUrl = rootUrl + 'response/' + id + '/request';
    $.ajax({
        url: ajaxUrl,
        error: onErrorOccured,
        success: function (requests) {
            var requestAmount = requests.length;
            console.log(requests);
            if (askForConfirmation) {
                if (!confirm('Do you really want to delete the response #' + id + '?\n' + requestAmount + ' requests will be deleted, as well.')) {
                    return; } }
            for (var i = 0; i < requestAmount; i++) {
                var req = requests[i];
                removeRequest(req.id, false);
            }
            //for (var req in requests) {
            //    removeRequest(req.id, false); }
            ajaxUrl = rootUrl + 'response/' + id;
            console.log('calling ' + ajaxUrl);
            $.ajax({
                url: ajaxUrl,
                method: 'DELETE',
                error: onErrorOccured,
                success: function () {
                    removeResponseRow(id);
                    console.info('Successfully removed response #' + id + '.');
                }
            });
        }
    });

}

/**
 * Removes a request from the database.
 *
 * Executes the AJAX call needed to perform the task.
 *
 * @param   id  The id of the request to remove.
 * @param   askForConfirmation   Toggles confirmation dialog on and off.
 */
function removeRequest(id, askForConfirmation = true) {
    console.log('Attempting to remove request #' + id + ' from database...');
    if (askForConfirmation) {
        if (!confirm('Do you really want to delete the request with the id #' + id + '?')) {
            return; } }
    var rootUrl = getRootUrl();
    var ajaxUrl = rootUrl + 'request/' + id;
    $.ajax({
        url: ajaxUrl,
        method: 'DELETE',
        error: onErrorOccured,
        success: function () {
            removeRequestRow(id);
            console.info('Successfully removed request #' + id + '.');
        }
    });
}

/**
 * Removes a response row from the table.
 *
 * @param   id  The id of the response whose row to remove.
 */
function removeResponseRow(id) {
    var idString = "#response-id-" + id;
    responseTable.row($(idString)).remove().draw();
    $('#request-table').DataTable();
}

/**
 * Removes a request row from the table.
 *
 * @param   id  The id of the request whose row to remove.
 */
function removeRequestRow(id) {
    var idString = "#request-id-" + id;
    table.row($(idString)).remove().draw();
}