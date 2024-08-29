function windows (id, url) {
    $.ajax({ url: url, cache: false, async: false, type: "GET" })
    .done(function (e) { $(id).html(e); })
    .fail(function (e) { alert(e); });
}


function windowAppend(id, url) {
    $.ajax({ url: url, cache: false, async: false, type: "GET" })
    .done(function (e) {
        $(id).append(e); 
    })
    .fail(function (e) {
        alert(e);
     });
}

function windowAppendData(id, url, data) {
    $.ajax({ url: url, cache: false, async: false, type: "post", data: data, datatype: "json" })
    .done(function (e) {
        $(id).append(e);
    })
    .fail(function (e) {
        alert(e);
    });
}

