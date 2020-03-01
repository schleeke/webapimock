/*
    .ctor()
    Executed on document load.
*/
$( document ).ready(function() {
  $("#btn-home").tooltip();
});


function gotoHome() {
  window.document.location.href = 'default.html';
}
