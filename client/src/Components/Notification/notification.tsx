import { createContext, useCallback, useContext, useState } from "react";
import { notificationBadgeClassName, notificationContainerClassName } from "./notification.styles";
import { CustomNotification, NotificationContextType, NotificationProviderProps } from "./notification.types";

const NotificationContext = createContext<NotificationContextType | undefined>(
    undefined
);

export function NotificationProvider({ children }: NotificationProviderProps) {
    const [notifications, setNotifications] = useState<CustomNotification[]>([]);

    const notify = useCallback(
        (message: string, duration: number = 5000) => {
            const id = Date.now();
            setNotifications((prev) => [...prev, { id, message }]);

            setTimeout(() => {
                setNotifications((prev) => prev.filter((n) => n.id !== id));
            }, duration);
        },
        []
    );

    return (
        <NotificationContext.Provider value={{ notify }}>
            {children}
            <div className={notificationContainerClassName}>
                {notifications.map(({ id, message }) => (
                    <div key={id} className={notificationBadgeClassName}>
                        {message}
                    </div>
                ))}
            </div>
        </NotificationContext.Provider>
    );
}

export function useNotification(): NotificationContextType["notify"] {
    const context = useContext(NotificationContext);
    if (!context) {
        throw new Error(
            "useNotification must be used within a NotificationProvider"
        );
    }
    return context.notify;
}