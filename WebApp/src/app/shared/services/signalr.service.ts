import { Injectable } from '@angular/core';
import { SystemConstants } from '@constants';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { environment } from 'src/environments/environment';

@Injectable({
    providedIn: 'root'
})
export class SignalRService {

    private connectionId: string;
    private hubConnection: HubConnection;

    constructor() { }

    startConnection() {
        this.hubConnection = new HubConnectionBuilder()
            .configureLogging(LogLevel.Debug)
            .withUrl(`http://${environment.HOST.SYSTEM}/notification`, { accessTokenFactory: () => this.getToken() }).build();

        this.hubConnection
            .start()
            .then(() => {
                console.log("SignalR was connected");
                return this.hubConnection.invoke('getConnectionId');
            })
            .then((connectionId: string) => {
                console.log("ConnectionId", connectionId);
                this.connectionId = connectionId;
                return this.hubConnection.invoke('getConnectionUser');
            })
            .then((userData: any) => {
                console.log("user connect Info", userData);
            })
            .catch(err => console.log('Error while starting connection ' + err));
    }

    listenEvent(eventName: string, cb: (...arrg: any[]) => void) {
        this.hubConnection.on(eventName, cb);
    }

    invoke(eventName: string, cb: (...arrg: any[]) => void) {
        this.hubConnection.invoke(eventName, cb);
    }

    getToken(): string {
        return localStorage.getItem(SystemConstants.ACCESS_TOKEN);
    }
}
