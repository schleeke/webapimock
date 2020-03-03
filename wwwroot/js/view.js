/*
    .ctor()
    Executed on document load.
*/
$( document ).ready(function() {
  $('[data-toggle="tooltip"]').tooltip();
});


function gotoHome() {
  window.document.location.href = 'default.html';
}

function gotoHelp() {
  window.document.location.href = 'help.html';
}
