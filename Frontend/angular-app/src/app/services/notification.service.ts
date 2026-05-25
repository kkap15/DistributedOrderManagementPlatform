import { Injectable, NgZone, signal } from "@angular/core";
import * as signalR from "@microsoft/signalr";
import { AppNotification } from "../interfaces/Notification";

@Injectable({ providedIn: 'root'})
export class NotificationService{
    private hubConnection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5002/hubs/notifications')
        .withAutomaticReconnect()
        .build();

    latestOrderNotification = signal<any>(null);
    latestPaymentNotification = signal<any>(null);
    notifications = signal<AppNotification[]>([]);

    constructor(private zone: NgZone) {
        this.hubConnection.on('OrderCreated', (data) => {
            this.zone.run(() => {
                this.latestOrderNotification.set(data);
                this.notifications.update(n => [
                    {type: 'order', message: `Order ${data.orderId} created`, time: new Date()},
                    ...n
                ]);
            });
        });

        this.hubConnection.on('PaymentProcessed', (data) => {
            this.zone.run(() => {
                this.latestPaymentNotification.set(data);
                this.notifications.update(n => [
                    {
                        type: 'payment',
                        message: `Payment ${data.paymentId} processed`,
                        time: new Date()
                    },
                    ...n
                ]);
            });
        });
    }
    
    async start(): Promise<void> {
        try {
            await this.hubConnection.start();
            console.log('SignalR connected');
        }
        catch (err) {
            console.error('SignalR error:', err);
            setTimeout(() => this.start(), 5000)
        }
    }
}