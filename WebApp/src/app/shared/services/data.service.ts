import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable()
export class DataService {

    private messageSource = new BehaviorSubject({ "default": "hello world !" });
    currentMessage = this.messageSource.asObservable();

    constructor() { }

    setData(key: string, value: any) {
        this.messageSource.next({ ...this.messageSource.value, [key]: value });
    }

    getData(): Observable<any> {
        return this.messageSource.pipe(
            map((data: any) => data)
        );
    }

    getDataByKey(key: string): Observable<any> {
        return this.messageSource.pipe(
            map((data: any) => data[key] || null)
        );
    }

}
