import html from './assets/game.html?raw';
import './assets/game.css';
import 'whatwg-fetch';

window.console.log('boot.ts: after core-js');

const bootElement = document.getElementById('boot')!;
let baseURL:string = bootElement.attributes['data-static-url-base']?.value;
if (!baseURL || baseURL == '')
    baseURL = document.body.attributes['data-static-url-base']?.value;

if (!baseURL || baseURL == '')
    baseURL = '/';

if (!baseURL.endsWith('/'))
	baseURL = baseURL + '/';

document.body.attributes['data-static-url-base'] = baseURL;

bootElement.outerHTML = html;

if (baseURL != '/')
{
	var images = document.getElementsByTagName("img");
	for(let i = 0; i< images.length; i++)
	{
		var img = images[i];
		img.src = new URL(`${baseURL}${img.attributes['src'].value}`).toString();
	}
}

window.console.log('boot.ts: loading landing.ts');



import('./landing');