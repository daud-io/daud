import html from './assets/game.html?raw';
import css from './assets/game.css';
import 'whatwg-fetch';

window.console.log('boot.ts: after core-js');

const styleTag = document.createElement('style');
styleTag.innerHTML = css;

const bootElement = document.getElementById('boot')!;
let baseURL = bootElement.attributes['data-static-url-base']?.value;
if (!baseURL || baseURL == '')
    baseURL = document.body.attributes['data-static-url-base']?.value;

if (!baseURL || baseURL == '')
    baseURL = location.href;

document.body.attributes['data-static-url-base'] = baseURL;

bootElement.outerHTML = html;
document.head.appendChild(styleTag);

var images = document.getElementsByTagName("img");
for(let i = 0;i < images.length; i++)
{
    var img = images[i];
    img.src = new URL(`${img.attributes['src'].value}`, baseURL).toString();
}


window.console.log('boot.ts: loading landing.ts');

/**
 * Element.closest() polyfill
 * https://developer.mozilla.org/en-US/docs/Web/API/Element/closest#Polyfill
 */
 if (!Element.prototype.closest) {
	if (!Element.prototype.matches) {
		Element.prototype.matches = (<any>Element.prototype).msMatchesSelector || Element.prototype.webkitMatchesSelector;
	}
	Element.prototype.closest = function (s) {
		var el = this;
		var ancestor:any = this;
		if (!document.documentElement.contains(el)) return null;
		do {
			if (ancestor.matches(s)) return ancestor;
			ancestor = ancestor.parentElement;
		} while (ancestor !== null);
		return null;
	};
}

import('./landing');