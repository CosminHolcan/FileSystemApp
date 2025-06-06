import { ReactNode } from "react";

export interface CustomNotification {
    id: number;
    message: string;
};

export interface NotificationContextType {
    notify: (message: string, duration?: number) => void;
};

export interface NotificationProviderProps {
    children: ReactNode;
};