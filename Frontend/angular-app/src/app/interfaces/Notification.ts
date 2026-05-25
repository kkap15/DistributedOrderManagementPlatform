export interface AppNotification {
    type: 'order' | 'payment';
    message: string;
    time: Date;
}