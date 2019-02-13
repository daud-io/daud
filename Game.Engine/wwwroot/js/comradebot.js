import { Controls } from "./controls";
import { Connection } from "./connection";

const canvas = document.getElementById("gameCanvas");

var data,
	groups,
	body,
	d,
	a = 0,
	k = 0,
	i = 0,
	// dance1 = setInterval(danceFunction, 150), - for dance bot
	cycleDuration = 40;

	

// cycle
setInterval(function() {
	data = window.Game.cache.bodies;
	groups = window.Game.cache.groups;
	for (var key in data) {
		if (data.hasOwnProperty(key)) {
			body = data[key];
			d = sqrt(Math.pow(body.Position.X, 2) + Math.pow(body.Position.Y, 2));
			/*
			body.Sprite
			body.Position.X
			body.Position.Y
			body.Angle
			body.group.ID
			*/
		}
	}
}, cycleDuration);

function merger() {
	
}

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
								dance1 = setInterval(danceFunction, 150);
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