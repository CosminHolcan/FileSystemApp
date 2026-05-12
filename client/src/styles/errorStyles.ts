import { mergeStyles } from "@fluentui/react";
import React from "react";


export const errorMessageClassName: string = mergeStyles({
    fontSize: 17,
    color: "red",
    marginBottom: "15px"
});

export const authErrorMessageStyles: React.CSSProperties = {
    fontFamily: "Grotesco",
    fontSize: 17,
    color: "red",
    marginLeft: "5vw",
    marginTop: "5vh"
};

export const authErrorMessageSmallMarginStyles: React.CSSProperties = {
    fontFamily: "Grotesco",
    fontSize: 15,
    color: "red",
    marginLeft: "5vw",
    marginTop: "3vh"
};
