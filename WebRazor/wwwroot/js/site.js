"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/hub").build();

connection.on("Reload", function () {
    var url = window.location.href;

    if (url.endsWith("Admin/Product")) {
        location.reload();
    }
});

connection.start().then().catch(function (err) {
    return console.log(err.toString());
});