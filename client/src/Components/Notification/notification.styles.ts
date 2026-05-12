import { mergeStyles } from "@fluentui/react";

export const notificationContainerClassName: string = mergeStyles({
    position: "fixed",
    top: 110,
    right: 15,
    display: "flex",
    flexDirection: "column",
    gap: 10,
    zIndex: 1000
});

export const notificationBadgeClassName: string = mergeStyles({
    backgroundColor: "#f2f4f8",
    color: "#0078d4",
    padding: "15px 20px",
    borderRadius: 5,
    boxShadow: "0 2px 10px rgba(0,0,0,0.3)",
    minWidth: 250,
    opacity: 1,
    animation: "fadeinout 3s ease forwards",
});