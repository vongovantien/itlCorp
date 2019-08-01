import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable()
export class DataService {

    private messageSource = new BehaviorSubject({ "default": "hello world !" });
    currentMessage = this.messageSource.asObservable();

    constructor() { }

    // changeMessage(message: string) {
    //     this.messageSource.next(message);
    // }
    setData(key: string, value: any) {
        this.messageSource.next({ ...this.messageSource.value, [key]: value });
    }

}