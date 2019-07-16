import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable()
export class GlobalState {

    public _data = new BehaviorSubject<Object>({ data: 'default' });

    public _dataStream$ = this._data.asObservable();

    public _subscriptions: Map<string, Function[]> = new Map<string, Function[]>();

    constructor() {
        this._dataStream$.subscribe((data: any) => this._onEvent(data));
    }

    notifyDataChanged(event: string | number, value: any) {

        const current = this._data[event];
        if (current !== value) {
            this._data[event] = value;

            this._data.next({
                event,
                data: this._data[event],
            });
        }
    }

    subscribe(event: string, callback: Function) {
        const subscribers = this._subscriptions.get(event) || [];
        subscribers.push(callback);
        this._subscriptions.set(event, subscribers);
    }

    _onEvent(data: any) {
        const subscribers = this._subscriptions.get(data['event']) || [];
        subscribers.forEach((callback) => {
            callback.call(null, data['data']);
        });
    }

    getCurrentData(event: string) {
        return this._data[event] || null;
    }
}