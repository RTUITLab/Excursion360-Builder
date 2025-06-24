"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/interophub").build();

connection.on("OpenTour", function (tourJson) {
    if (typeof (tourJson) !== "string") {
        console.error("incorrect argument");
        return;
    }
    const tourData = JSON.parse(tourJson);
    const { tour, rotation } = tourData;
    location.hash = "";
    document.viewer.show(tour, true);
    console.log(tour);
    document.viewer.rotateCameraToQuaternion(rotation);
});

connection.start().then(function () {
    console.log("connection established");
}).catch(function (err) {
    return console.error(err.toString());
});
