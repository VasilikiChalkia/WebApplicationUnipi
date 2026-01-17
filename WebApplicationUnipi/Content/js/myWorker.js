onconnect = function (a) {

    var port = a.ports[0];

    port.onmessage = function (a) {

        var arrived = a.data.reply;
        var page = a.data.extra;
        //console.log(arrived);
        var timeLeft = arrived * 60;
        var notiTime = 300;
        var currentTime;
        var currentCond;
        console.log(page);

        if (page === "close") {
            //clearInterval(timer);
            self.close();
        }

        var timer = setInterval(function () {

            timeLeft--;

            if (timeLeft === notiTime) {
                currentCond = "still";
                port.postMessage({ reply: currentCond, extra: notiTime });
            }

            if (timeLeft === 0) {
                currentCond = "over";
                currentTime = 0;
                port.postMessage({ reply: currentCond, extra: timeLeft });
                clearInterval(timer);
            }

            console.log(timeLeft);

        }, 1000);

    };

    //port.start();
};