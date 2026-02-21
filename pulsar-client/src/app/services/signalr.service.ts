import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private hubConnection!: signalR.HubConnection;
  pingReceived$ = new Subject<any>();

  startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/pulsar')
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR connected'))
      .catch((err) => console.error('SignalR error:', err));

    this.hubConnection.on('ReceivePingResult', (data) => {
      this.pingReceived$.next(data);
    });
  }
}
