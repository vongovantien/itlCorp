import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { SystemConstants } from 'src/constants/system.const';

@Injectable({ providedIn: 'root' })
export class ApiService {

    baseUrl: string = '//';
    private _headers: {} = {};
    constructor(protected _http: HttpClient,
    ) {
    }

    setHeaders(headers: any = {}) {
        this._headers = Object.assign({}, this._headers, headers);
        return this;
    }

    // set resorce to request
    setUrl(url: string = ''): string {
        return `${this.baseUrl}/${url}`;
    };

    getCurrentLanguage() {
        return localStorage.getItem(SystemConstants.CURRENT_LANGUAGE);
    }

    getCurrentVersion() {
        return localStorage.getItem(SystemConstants.CURRENT_VERSION);
    }

    post(url: string, data?: any, params?: any, headers?: any) {
        return this._http
            .post(this.setUrl(url), data, {
                params,
                headers: Object.assign({}, this._headers, headers)
            });
    }

    postFile(url: string, files: any, name: string = null, params?: any) {
        if (files.length === 0) {
            return;
        }
        const formData = new FormData();
        for (const file of files) {
            formData.append(name || file.name, file);
        }
        // const params = new HttpParams();
        const options = {
            params: params,
            reportProgress: true,
            headers: new HttpHeaders({
                'accept': 'application/json'
            })
        };
        return this._http.post(this.setUrl(url), formData, options);
    }

    putFile(url: string, files: any, name: string = null, params?: any) {
        if (files.length === 0) {
            return;
        }
        const formData = new FormData();
        for (const file of files) {
            formData.append(name, file);
        }
        const options = {
            params: params,
            reportProgress: true,
            headers: new HttpHeaders({
                'accept': 'application/json'
            })
        };
        return this._http.put(this.setUrl(url), formData, options);
    }

    putFormData(url: string, files: File[] = [], body?: any, name: string = null, params?: any) {
        // haven't both files and body.
        if (files.length <= 0 && Object.keys(body).length === 0 && body.constructor === Object) {
            return;
        }
        // declare formData;
        const formData = new FormData();
        // append file
        for (const file of files) {
            formData.append(name, file);
        }

        // set body
        for (const key in body) {
            if (Object.prototype.hasOwnProperty.call(body, key)) {
                formData.set(key, body[key]);
            }
        }
        //
        const options = {
            params: params,
            reportProgress: true,
            headers: new HttpHeaders({
                'accept': 'application/json',
            })
        };
        return this._http.put(this.setUrl(url), formData, options);

    }

    put(url: string, data?: any, params?: any, headers?: any) {
        return this._http
            .put(this.setUrl(url), data, {
                params,
                headers: Object.assign({}, this._headers, headers)
            });
    }

    downloadfile(url: string, data?: any, params?: any, headers?: any) {
        if (data !== null && data !== undefined) {
            return this._http.post(this.setUrl(url), data, {
                params,
                headers: Object.assign({}, this._headers, headers),
                responseType: 'arraybuffer'
            });
        } else {
            return this._http.get(this.setUrl(url), {
                params,
                headers: Object.assign({}, this._headers, headers),
                responseType: 'arraybuffer'
            });
        }
    }

    get(url: string = '', params?: any, headers: any = {}, isBaseHref: boolean = true) {
        return this._http.get(isBaseHref ? this.setUrl(url) : url, { params, headers });
    }

    delete(url: string, params?: any, headers?: any) {
        return this._http.delete(this.setUrl(url), { params, headers });
    }
}
