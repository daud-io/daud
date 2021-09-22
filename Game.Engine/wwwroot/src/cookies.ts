import { set as CookieSet, get as CookieGet, remove as CookieRemove, CookieAttributes } from "js-cookie";

export class Cookies
{
    static set(name: string, value: string | object, options?: CookieAttributes): string | undefined
    {
        return CookieSet(name, value, options);
    }
    static get(name: string): string | undefined
    {
        return CookieGet(name);
    }
    static remove(name: string)
    {
        CookieRemove(name);
    }
}