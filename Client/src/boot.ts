import html from './assets/game.html?raw';
import css from './assets/game.css';

const styleTag = document.createElement('style');
styleTag.innerHTML = css;

const bootElement = document.getElementById('boot')!;
const baseURL = bootElement.attributes['data-static-url-base'].value
    ?? document.body.attributes['data-static-url-base'].value;
document.body.attributes['data-static-url-base'] = baseURL;

bootElement.outerHTML = html;
document.head.appendChild(styleTag);

document.querySelectorAll('img')
    .forEach(img => {
        img.src = new URL(`${img.attributes['src'].value}`, baseURL).toString();
    });

import('./landing');