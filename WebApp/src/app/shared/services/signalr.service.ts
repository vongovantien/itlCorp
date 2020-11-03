import { Injectable } from '@angular/core';
import { SystemConstants } from '@constants';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
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
            .withAutomaticReconnect()
            .withUrl(`http${environment.AUTHORIZATION.requireHttps ? 's' : ''}://${environment.HOST.SYSTEM}/notification`, { accessTokenFactory: () => this.getToken() }).build();

        this.hubConnection
            .start()
            .then(() => {
                console.log("SignalR was connected");
                return this.hubConnection.invoke('getConnectionId');
            })
            .then((connectionId: string) => {
                console.log("ConnectionId", connectionId);
                this.connectionId = connectionId;
                return this.hubConnection.invoke('GetConnectionIds');
            })
            .then((connectionIds: any) => {
                console.log("ConnectionIds", connectionIds);
            })
            .catch(err => {
                console.log('Error while starting connection ' + err)
            });
    }

    disConection() {
        this.hubConnection.stop();
    }

    getConnectionState(): HubConnectionState {
        return this.hubConnection.state;
    }

    listenEvent(eventName: string, cb: (...arrg: any[]) => void) {
        this.hubConnection.on(eventName, cb);
    }

    invoke(eventName: string, cb: any) {
        this.hubConnection.invoke(eventName, cb);
    }

    getToken(): string {
        return localStorage.getItem(SystemConstants.ACCESS_TOKEN);
    }
}
