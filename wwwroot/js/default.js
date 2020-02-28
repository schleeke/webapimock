function onWindowLoad() {
    getAmountOfMockupRequests();
}

function getAmountOfMockupRequests() {
    $.ajax({
        url: '../request',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            var parsedData = JSON.parse(data);
            var count = parsedData.length;
            var foo = 'bar';
        },
        error: function (request, status, errorText) {}
    });
}