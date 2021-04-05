"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/interophub").build();


connection.on("RotateCamera", function (rotationJson) {
    if (typeof (rotationJson) === "string") {
        const rotation = JSON.parse(rotationJson);
        document.viewer.rotateCameraToQuaternion(rotation);
    } else {
        console.error("incorrect argument");
    }
});

connection.on("OpenTour", function (tourJson) {
    if (typeof (tourJson) === "string") {
        const tour = JSON.parse(tourJson);
        document.viewer.show(tour);
        console.log(tour);
    } else {
        console.error("incorrect argument");
    }
});


connection.start().then(function () {
    console.log("connection established");
}).catch(function (err) {
    return console.error(err.toString());
});
