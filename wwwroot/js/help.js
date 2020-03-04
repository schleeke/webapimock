/*
    .ctor()
    Executed on document load.
*/
$( document ).ready(function() {
    $('[data-toggle="tooltip"]').tooltip();
    getAllUrls();
});


function getRootUrl() {
  var location = window.location.origin 
                  ? window.location.origin + '/'
                  : window.location.protocol + '/' + window.location.host + ':' + window.location.port + '/';
  return location;
}

function getGuiUrl() {
  var location = getRootUrl();
  location = location + 'gui';
  return location;
}

function getAllUrls() {
    var rootUrl = getRootUrl();
    var url = rootUrl + 'tools/mockupprefix';
    $.ajax({
        url: url,
        success: function (data) {
            if (data === null) { return; }
            if (rootUrl.startsWith('http') == false) { return; }
            $('#server-root-url').text(rootUrl);
            $('#server-gui-url').text(rootUrl + 'gui');
            $('#server-mock-url').text(rootUrl + data);
            $('#server-def-url').text(rootUrl + 'swagger/v1/swagger.json');
        }
    });
}