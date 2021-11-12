import 'whatwg-fetch';

if (!Element.prototype.matches) {
    Element.prototype.matches = (<any>Element.prototype).msMatchesSelector ||
        (<any>Element.prototype).webkitMatchesSelector;
}

if (!Element.prototype.closest) {
    Element.prototype.closest = function (s: string) {
        var el: any = this;

        do {
            if (Element.prototype.matches.call(el, s)) return el;
            el = el.parentElement || el.parentNode;
        } while (el !== null && el.nodeType === 1);
        return null;
    };
}
