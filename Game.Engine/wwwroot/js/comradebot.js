import { Controls } from "./controls";

const canvas = document.getElementById("gameCanvas");

var a = 0;
var k = 0;
var i = 0
var dance1 = setInterval(danceFunction, 150);

function danceFunction() {
	setAngle(45 + k * 90, 10000);
	k = (k == 1) ? 0 : 1;
	i++;
	if (i == 30) {
		i = 0;
		clearInterval(dance1);
		var dance2 = setInterval(function(){
			setAngle(225 + k * 90, 10000);
			k = (k == 1) ? 0 : 1;
			i++;
			if (i == 30) {
				i = 0;
				clearInterval(dance2);
				var dance3 = setInterval(function(){
					setAngle(50*i, 10000);
					i++;
					if (i == 100) {
						i = 0;
						clearInterval(dance3);
						var dance4 = setInterval(function(){
							setAngle(-50*i, 10000);
							i++;
							if (i == 100) {
								i = 0;
								clearInterval(dance4);
								var dance5 = setInterval(function(){
									setAngle(0, k * 10000);
									k = (k == 1) ? 0 : 1;
									i++;
									if (i == 100) {
										i = 0;
										clearInterval(dance5);
										var dance6 = setInterval(function(){
											setAngle(180, k * 10000);
											k = (k == 1) ? 0 : 1;
											i++;
											if (i == 100) {
												i = 0;
												clearInterval(dance6);
												dance1 = setInterval(danceFunction, 150);
											}
										}, 200);
									}
								}, 200);
							}
						}, 40);
					}
				}, 40);
			}
		}, 150);
	}
}

function setAngle(a, d) {
	a = Math.radians(a);
	Controls.angle = a;
	Controls.mouseX = d * Math.cos(a) + canvas.width / 2;
	Controls.mouseY = d * Math.sin(a) + canvas.height / 2;
}

Math.radians = function(degrees) {
  return degrees * Math.PI / 180;
};