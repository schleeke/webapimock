var table = null;
var selectedRequestId = 0;
/*
    .ctor()
    Executed on document load.
*/
$( document ).ready(function() {
  getRequestData();  
});


function gotoHome() {
  window.document.location.href = 'default.html';
}

function getRequestData() {
  var url = getRootUrl();
  url = url + 'request';
  $.ajax({
    url: url,
    error: function(request, status, errorText) { alert(errorText); },
    success: OnRequestDataReceived,  
  });
}

function gotoHelp() {
  window.document.location.href = 'help.html';
}

function OnRequestDataReceived(data) {
  $('#request-table-body').empty();
  $('#request-table-body').hide();
  if(data == null || data == '') { return; }
  data.forEach(element => {
    var rawRowContent = "<tr id=\"request-id-" + element.id + "\"><td>" + element.id + "</td><td>" + element.route + "</td><td>" + element.httpMethod + "</td><td>" + element.query + "</td><td>" + element.body + "</td><td>" + element.responseId + "</td></tr>";
    $('#request-table-body').append(rawRowContent);
  });
  $('#request-table-body').show();
  table = $('#request-table').DataTable({
    columnDefs: [
      {"visible": false, "targets": 0},
      {"visible": false, "targets": 5}
    ]
  });
  $('#request-table-body').on('click', 'tr', onRequestRowClicked);
}

function onRequestRowClicked() {
  if ( $(this).hasClass('table-active') ) {
    $(this).removeClass('table-active'); }
  else {
    table.$('tr.table-active').removeClass('table-active');
    $(this).addClass('table-active'); }
  var rowId = $(this).attr('id');
  var requestId = rowId.substring(11);
  $('#btn-request-delete').removeAttr('disabled');
  selectedRequestId = Number(requestId);

}

function getRootUrl() {
  var location = window.location.origin 
                  ? window.location.origin + '/'
                  : window.location.protocol + '/' + window.location.host + ':' + window.location.port + '/';
  return location;
}

function deleteRequest() {
  if(!confirm('Do you really want to delete the request with the id #' + selectedRequestId + '?')) { return; }
  var url = getRootUrl();
  url = url + 'request/' + selectedRequestId;
  $.ajax({
    url: url,
    method: 'DELETE',
    error: function(requst, status, errorText) { alert(errorText); },
    success: function() {
      var idString = "#request-id-" + selectedRequestId;
      $(idString).remove();
      $('#btn-request-delete').prop('disabled', true);
    }
  });

}