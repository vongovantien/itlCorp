import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { environment } from 'src/environments/environment';

@Injectable({
    providedIn: 'root'
})
export class SignalRService {
    private hubConnection: HubConnection;

    constructor() { }

    startConnection() {
        this.hubConnection = new HubConnectionBuilder()
            .configureLogging(LogLevel.Debug)
            .withUrl(`http://${environment.HOST.SYSTEM}/notification`).build();

        this.hubConnection
            .start()
            .then(() => console.log("SignalR was connected"))
            .catch(err => console.log('Error while starting connection ' + err));
    }

    listenEvent(eventName: string, cb: (...arrg: any[]) => void) {
        this.hubConnection.on(eventName, cb);
    }

    disconnect() {
        this.hubConnection.stop();
    }
}
