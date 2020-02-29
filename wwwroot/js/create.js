function onHttpMethodSelect(value) {
    if (value == 'GET') {
        $('#request-body-row').hide();
    } else {
        $('#request-body-row').show();
    }
}